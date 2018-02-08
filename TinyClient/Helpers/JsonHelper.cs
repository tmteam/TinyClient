using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TinyClient.Helpers
{
    public static class JsonHelper
    {
        public const string DateTimeFormatString = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";

        private static readonly JsonSerializerSettings SerializerSettings;

        static JsonHelper()
        {
            SerializerSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateFormatString = DateTimeFormatString,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = true } }
            };
        }
        public static T Deserialize<T>(Stream stream)
        {
            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var serializer = JsonSerializer.Create(SerializerSettings);
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        public static string Serialize(object value)
        {
            using (var stringWriter = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                var serializer = JsonSerializer.Create(SerializerSettings);
                serializer.Serialize(jsonWriter, value);
                return stringWriter.ToString();
            }
        }

        public static T Deserialize<T>(string value)
        {
            using (var streamReader = new StringReader(value))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var serializer = JsonSerializer.Create(SerializerSettings);
                return serializer.Deserialize<T>(jsonReader);
            }
        }
    }
}
