using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Patcher.Hooks;

namespace Oxide.Patcher.Common.JsonHelpers
{
    /// <summary>
    /// Converter to handle the writing of hooks back into the json file
    /// </summary>
    public class HooksListConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is List<Hook> hooks))
            {
                throw new InvalidCastException("Could not cast object to List<Hook> when writing hook list");
            }

            HookRef[] refs = new HookRef[hooks.Count];
            for (int i = 0; i < refs.Length; i++)
            {
                HookRef hookRef = refs[i] = new HookRef();

                hookRef.Hook = hooks[i];
                hookRef.Type = hookRef.Hook.GetType().Name;
                hookRef.Hook.BaseHookName = hookRef.Hook.BaseHook?.Name;
            }

            serializer.Serialize(writer, refs);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Hook).IsAssignableFrom(objectType);
        }
    }
}
