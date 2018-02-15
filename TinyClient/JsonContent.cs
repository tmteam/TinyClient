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


        private JsonContent(string stringContent)
        {
            Content = stringContent;
        }
        public JsonContent(object content)
        {
            if(content!=null)
                Content = JsonHelper.Serialize(content);
        }

        public string ContentType => HttpMediaTypes.JsonUtf8;

        public string Content { get; }

        public void WriteTo(Stream stream, Uri host)
        {
            if (string.IsNullOrWhiteSpace(Content))
                return;

            var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(Content);
            writer.Flush();
        }
    }
}