using System;
using System.Text;
using TinyClient.Helpers;

namespace TinyClient
{
    public class JsonContent: IContent
    {
        public static JsonContent CreateEmpty() => new JsonContent(stringContent: null);

        public static JsonContent CreateForObject(object jsonSerializableObject)
        {
            if (jsonSerializableObject == null)
                return CreateEmpty();
            var bodyParam = JsonHelper.Serialize(jsonSerializableObject);
            return CreateForJsonString(bodyParam);
        }

        public static JsonContent CreateForJsonString(string jsonString) => new JsonContent(stringContent: jsonString);

        private readonly string _content;

        private JsonContent(string stringContent)
        {
            _content = stringContent;
        }
        public JsonContent(object content)
        {
            if(content!=null)
                _content = JsonHelper.Serialize(content);
        }

        public string ContentType => HttpMediaTypes.Json;
        public byte[] GetDataFor(Uri host)
        {
            if (string.IsNullOrWhiteSpace(_content))
                return new byte[0];
            return Encoding.UTF8.GetBytes(_content);
        }
    }
}