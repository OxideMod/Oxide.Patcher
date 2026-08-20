using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Oxide.Patcher.Docs
{
    public static class DocsCache
    {
        private class CacheEntry
        {
            public DateTime LastWriteTimeUtc { get; set; }
            public DocsData Data { get; set; }
        }

        private static readonly ConcurrentDictionary<string, CacheEntry> _cache = new ConcurrentDictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Loads and caches DocsData from the specified file path, reloading only when the file on disk changes.
        /// </summary>
        public static DocsData GetDocs(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            try
            {
                DateTime currentWriteTime = File.GetLastWriteTimeUtc(filePath);

                if (_cache.TryGetValue(filePath, out CacheEntry entry) && entry.LastWriteTimeUtc == currentWriteTime)
                {
                    return entry.Data;
                }

                string json = File.ReadAllText(filePath);
                DocsData data = JsonConvert.DeserializeObject<DocsData>(json);

                if (data != null)
                {
                    _cache[filePath] = new CacheEntry
                    {
                        LastWriteTimeUtc = currentWriteTime,
                        Data = data
                    };
                }

                return data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DocsCache] Error loading docs from '{filePath}': {ex}");
                return null;
            }
        }

        /// <summary>
        /// Looks up the CodeAfterInjection for the given hook name in the specified docs file.
        /// </summary>
        public static string GetDocsPlacement(string filePath, string hookName)
        {
            DocsData data = GetDocs(filePath);
            if (data?.Hooks == null || string.IsNullOrEmpty(hookName))
            {
                return null;
            }

            DocsHook hook = data.Hooks.FirstOrDefault(h => string.Equals(h.Name, hookName, StringComparison.OrdinalIgnoreCase))
                         ?? data.Hooks.FirstOrDefault(h => string.Equals(h.HookName, hookName, StringComparison.OrdinalIgnoreCase));

            return hook?.CodeAfterInjection;
        }
    }
}
