namespace Oxide.Patcher.Common.Extensions
{
    public static class StringEx
    {
        /// <summary>
        /// Get the string slice between the two indexes.
        /// Inclusive for start index, exclusive for end index.
        /// </summary>
        public static string Slice(this string source, int start, int end)
        {
            if (end < 0) // Keep this for negative end support
            {
                end = source.Length + end;
            }
            int len = end - start; // Calculate length
            return source.Substring(start, len); // Return Substring of length
        }
    }
}
