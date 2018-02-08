using System;
using System.Text;
using TinyClient.Helpers;

namespace TinyClient
{
    public class JsonContent: IContent
    {
        private readonly object _content;

        public JsonContent(object content)
        {
            _content = content;
        }

        public string ContentType => HttpMediaTypes.Json;
        public byte[] GetDataFor(Uri host)
        {
            return CreateContent(_content);
        }


        private byte[] CreateContent(object content)
        {
            if (content == null)
                return new byte[0];
            var bodyParam = JsonHelper.Serialize(content);

            return Encoding.UTF8.GetBytes(bodyParam);
        }
    }
}