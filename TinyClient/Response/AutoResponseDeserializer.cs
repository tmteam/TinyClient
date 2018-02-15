using System;
using System.IO;
using TinyClient.Helpers;

namespace TinyClient.Response
{
    public class AutoResponseDeserializer: IResponseDeserializer
    {
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        public IHttpResponse Deserialize(ResponseInfo responseInfo, Stream dataStream)
        {
            var contentType = responseInfo.GetHeaderValueOrNull(HttpHelper.ContentTypeHeader);

            return GetSerializerFor(contentType)
                .Deserialize(responseInfo, dataStream);
        }

        /// <exception cref="NotSupportedException"></exception>
        private IResponseDeserializer GetSerializerFor(string contentType)
        {
            if(string.IsNullOrWhiteSpace(contentType))
                return new EmptyResponseDeserializer();

            if (contentType.StartsWith("text"))
                return new TextResponseDeserialaizer();

            if(contentType.StartsWith(HttpMediaTypes.Json))
                return new TextResponseDeserialaizer();

            throw new NotSupportedException($"contentType {contentType} is not supported");
        }
    }
}
