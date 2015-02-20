using System;
using System.Collections.Generic;

using OxidePatcher.Hooks;

using Newtonsoft.Json;

namespace OxidePatcher
{
    /// <summary>
    /// A set of changes to make to an assembly
    /// </summary>
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
        /// Initializes a new instance of the Manifest class
        /// </summary>
        public Manifest()
        {
            // Fill in defaults
            Hooks = new List<Hook>();
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
                    if (reader.TokenType != JsonToken.PropertyName) return null;
                    string propname = (string)reader.Value;
                    if (!reader.Read()) return null;
                    switch (propname)
                    {
                        case "AssemblyName":
                            manifest.AssemblyName = (string)reader.Value;
                            break;
                        case "Hooks":
                            if (reader.TokenType != JsonToken.StartArray) return null;
                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                if (reader.TokenType != JsonToken.StartObject) return null;
                                if (!reader.Read()) return null;
                                if (reader.TokenType != JsonToken.PropertyName) return null;
                                if ((string)reader.Value != "Type") return null;
                                if (!reader.Read()) return null;
                                string hooktype = (string)reader.Value;
                                Type t = Hook.GetHookType(hooktype);
                                if (t == null) throw new Exception("Unknown hook type");
                                Hook hook = Activator.CreateInstance(t) as Hook;
                                if (!reader.Read()) return null;
                                if (reader.TokenType != JsonToken.PropertyName) return null;
                                if ((string)reader.Value != "Hook") return null;
                                if (!reader.Read()) return null;
                                serializer.Populate(reader, hook);
                                if (!reader.Read()) return null;

                                manifest.Hooks.Add(hook);
                            }
                            break;
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
                }

                writer.WritePropertyName("Hooks");
                serializer.Serialize(writer, refs);

                writer.WriteEndObject();
            }
        }
    }
}
