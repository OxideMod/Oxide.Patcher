using Newtonsoft.Json;
using Oxide.Patcher.Fields;
using Oxide.Patcher.Hooks;
using Oxide.Patcher.Modifiers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Oxide.Patcher
{
    /// <summary>
    /// A set of changes to make to an assembly
    /// </summary>
    [JsonConverter(typeof(Converter))]
    public class Manifest
    {
        /// <summary>
        /// Gets or sets the name of the assembly in the target directory
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the hooks contained in this project
        /// </summary>
        public List<Hook> Hooks { get; set; }

        /// <summary>
        /// Gets or sets the changed modifiers in this project
        /// </summary>
        public List<Modifier> Modifiers { get; set; }

        /// <summary>
        /// Gets or sets the additional fields in this project
        /// </summary>
        public List<Field> Fields { get; set; }

        private static string[] ValidExtensions => new[] { ".dll", ".exe" };

        /// <summary>
        /// Initializes a new instance of the Manifest class
        /// </summary>
        public Manifest()
        {
            // Fill in defaults
            Hooks = new List<Hook>();
            Modifiers = new List<Modifier>();
            Fields = new List<Field>();
        }

        public class Converter : JsonConverter
        {
            private struct HookRef
            {
                public string Type;
                public Hook Hook;
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Manifest);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                Manifest manifest = existingValue != null ? existingValue as Manifest : new Manifest();
                while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType != JsonToken.PropertyName)
                    {
                        return null;
                    }

                    string propname = (string)reader.Value;
                    if (!reader.Read())
                    {
                        return null;
                    }

                    switch (propname)
                    {
                        case "AssemblyName":
                            manifest.AssemblyName = (string)reader.Value;
                            if (!IsExtensionValid(manifest.AssemblyName))
                            {
                                manifest.AssemblyName += ".dll";
                            }

                            break;

                        case "Hooks":
                            if (reader.TokenType != JsonToken.StartArray)
                            {
                                return null;
                            }

                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                if (reader.TokenType != JsonToken.StartObject)
                                {
                                    return null;
                                }

                                if (!reader.Read())
                                {
                                    return null;
                                }

                                if (reader.TokenType != JsonToken.PropertyName)
                                {
                                    return null;
                                }

                                if ((string)reader.Value != "Type")
                                {
                                    return null;
                                }

                                if (!reader.Read())
                                {
                                    return null;
                                }

                                string hooktype = (string)reader.Value;
                                Type t = Hook.GetHookType(hooktype);
                                if (t == null)
                                {
                                    throw new Exception("Unknown hook type");
                                }

                                Hook hook = Activator.CreateInstance(t) as Hook;
                                if (!reader.Read())
                                {
                                    return null;
                                }

                                if (reader.TokenType != JsonToken.PropertyName)
                                {
                                    return null;
                                }

                                if ((string)reader.Value != "Hook")
                                {
                                    return null;
                                }

                                if (!reader.Read())
                                {
                                    return null;
                                }

                                serializer.Populate(reader, hook);
                                if (!reader.Read())
                                {
                                    return null;
                                }

                                if (!Path.HasExtension(hook.AssemblyName))
                                {
                                    hook.AssemblyName += ".dll";
                                }

                                manifest.Hooks.Add(hook);
                            }
                            break;

                        case "Modifiers":
                            if (reader.TokenType != JsonToken.StartArray)
                            {
                                return null;
                            }

                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                if (reader.TokenType != JsonToken.StartObject)
                                {
                                    return null;
                                }

                                Modifier modifier = Activator.CreateInstance(typeof(Modifier)) as Modifier;
                                serializer.Populate(reader, modifier);

                                if (!Path.HasExtension(modifier.AssemblyName))
                                {
                                    modifier.AssemblyName += ".dll";
                                }

                                manifest.Modifiers.Add(modifier);
                            }
                            break;

                        case "Fields":
                            if (reader.TokenType != JsonToken.StartArray)
                            {
                                return null;
                            }

                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                if (reader.TokenType != JsonToken.StartObject)
                                {
                                    return null;
                                }

                                Field field = Activator.CreateInstance(typeof(Field)) as Field;
                                serializer.Populate(reader, field);

                                if (!Path.HasExtension(field.AssemblyName))
                                {
                                    field.AssemblyName += ".dll";
                                }

                                manifest.Fields.Add(field);
                            }
                            break;
                    }
                }
                foreach (Hook hook in manifest.Hooks)
                {
                    if (!string.IsNullOrWhiteSpace(hook.BaseHookName))
                    {
                        foreach (Hook baseHook in manifest.Hooks)
                        {
                            if (baseHook.Name.Equals(hook.BaseHookName))
                            {
                                hook.BaseHook = baseHook;
                                break;
                            }
                        }
                        if (hook.BaseHook == null)
                        {
                            throw new Exception("BaseHook missing: " + hook.BaseHookName);
                        }
                    }
                }
                return manifest;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Manifest manifest = value as Manifest;

                writer.WriteStartObject();

                writer.WritePropertyName("AssemblyName");
                writer.WriteValue(manifest.AssemblyName);

                HookRef[] refs = new HookRef[manifest.Hooks.Count];
                for (int i = 0; i < refs.Length; i++)
                {
                    refs[i].Hook = manifest.Hooks[i];
                    refs[i].Type = refs[i].Hook.GetType().Name;
                    refs[i].Hook.BaseHookName = refs[i].Hook.BaseHook != null ? refs[i].Hook.BaseHook.Name : null;
                }

                writer.WritePropertyName("Hooks");
                serializer.Serialize(writer, refs);

                writer.WritePropertyName("Modifiers");
                serializer.Serialize(writer, manifest.Modifiers);

                writer.WritePropertyName("Fields");
                serializer.Serialize(writer, manifest.Fields);

                writer.WriteEndObject();
            }

            private bool IsExtensionValid(string assembly)
            {
                string ext = Path.GetExtension(assembly);
                foreach (string extension in ValidExtensions)
                {
                    if (extension == ext)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
