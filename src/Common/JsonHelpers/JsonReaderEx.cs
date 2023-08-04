using Newtonsoft.Json;
using Oxide.Patcher.Hooks;

namespace Oxide.Patcher.Common.JsonHelpers
{
    public static class JsonReaderEx
    {
        /// <summary>
        /// Move the reader to the next token that matches the target token type
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="tokenType">Target type of the token to read up to</param>
        public static void ReadToNextTokenOfType(this JsonReader reader, JsonToken tokenType)
        {
            while (reader.Read())
            {
                if (reader.TokenType == tokenType)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Get the next value of the next property matching the target property name
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="propertyName">Target property name</param>
        /// <typeparam name="T">Type to cast to and return</typeparam>
        /// <returns>Value of the property casted to T</returns>
        public static T GetNextPropertyValue<T>(this JsonReader reader, string propertyName) where T : class
        {
            while (reader.Read())
            {
                //If it is a property and matches the name then move onto the value token and return
                if (reader.TokenType == JsonToken.PropertyName && reader.Value as string == propertyName && reader.Read())
                {
                    return reader.Value as T;
                }
            }

            return null;
        }

        /// <summary>
        /// Populate the next hook object with the hook data
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="serialiser">Current json serialiser in the read method</param>
        /// <param name="hook">Hook data to populate with</param>
        public static void PopulateNextHookObj(this JsonReader reader, JsonSerializer serialiser, Hook hook)
        {
            while (reader.Read())
            {
                //If it is the "Hook" property, move onto the next StartObject token and populate
                if (reader.TokenType == JsonToken.PropertyName && reader.Value as string == "Hook" && reader.Read())
                {
                    serialiser.Populate(reader, hook);
                    return;
                }
            }
        }
    }
}
