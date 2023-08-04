using System;
using Newtonsoft.Json;
using Oxide.Patcher.Hooks;

namespace Oxide.Patcher.Common.JsonHelpers
{
    public class HookConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string hookTypeName = reader.GetNextPropertyValue<string>("Type");
            if (string.IsNullOrEmpty(hookTypeName))
            {
                throw new Exception("Could not read the hook type");
            }

            Type hookType = Hook.GetHookType(hookTypeName);
            if (hookType == null)
            {
                throw new Exception($"Unknown hook type '{hookTypeName}'");
            }

            if (!(Activator.CreateInstance(hookType) is Hook hookDefinition))
            {
                throw new Exception("Failed to create instance of hook definition");
            }

            reader.PopulateNextHookObj(serializer, hookDefinition);
            reader.ReadToNextTokenOfType(JsonToken.EndObject);

            return hookDefinition;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Hook).IsAssignableFrom(objectType);
        }
    }
}
