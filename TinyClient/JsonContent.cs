using System;
using System.IO;
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

        public string ContentType => HttpMediaTypes.JsonUtf8;
        public void WriteTo(Stream stream, Uri host)
        {
            if (string.IsNullOrWhiteSpace(_content))
                return;

            var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(_content);
            writer.Flush();
        }
    }
}