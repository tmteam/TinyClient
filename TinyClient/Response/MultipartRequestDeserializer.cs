using System.Collections.Generic;
using System.IO;
using System.Net;
using TinyClient.Helpers;

namespace TinyClient.Response
{
    

    public class MultipartRequestDeserializer : IResponseDeserializer
    {
        private readonly IResponseDeserializer[] _subresponseDeserializers;

        public MultipartRequestDeserializer(IResponseDeserializer[] subresponseDeserializers)
        {
            _subresponseDeserializers = subresponseDeserializers;
        }

        /// <exception cref="InvalidDataException"></exception>
        public IHttpResponse Deserialize(ResponseInfo responseInfo, Stream dataStream)
        {
            var contentType = responseInfo.GetHeaderValueOrNull(HttpHelper.ContentTypeHeader);
            if(string.IsNullOrWhiteSpace(contentType))
                throw new InvalidDataException("Content-Type header missed");

            var boundary = BatchSerializeHelper.GetBoundaryStringOrThrow(contentType);

            var subresponses = new List<IHttpResponse>(_subresponseDeserializers.Length);
            using (var reader = new PeekableStreamReader(dataStream))
            {
                if (reader.ReadUntilFirstNonEmptyLine())
                {
                    while (true)
                    {
                        var subrequest = ParseNextSubresponseFrom(reader, boundary);
                        subresponses.Add(subrequest);
                        if (reader.PeekLine() == BatchSerializeHelper.GetCloseBoundaryString(boundary))
                            break;
                    }
                }
            }
            if(subresponses.Count!= _subresponseDeserializers.Length)
                throw new InvalidDataException($"Actual and expected items count are not equal. Actual: {subresponses.Count}, expected: {_subresponseDeserializers.Length}");

            return new HttpResponse<IHttpResponse[]>(responseInfo, subresponses.ToArray());
        }

        /// <exception cref="InvalidDataException"></exception>
        private HttpResponse<string> ParseNextSubresponseFrom(PeekableStreamReader reader, string boundary)
        {
            var currentLine = reader.ReadFirstNonEmptyLine();
            if (currentLine != BatchSerializeHelper.GetOpenBoundaryString(boundary))
                throw new InvalidDataException("Response boundary missed");

            currentLine = reader.ReadFirstNonEmptyLineOrThrow("Content type header not found");
            //  if (currentLine?.Trim() != HttpHelper.HttpRequestContentTypeHeaderString)
            if (!currentLine.Trim().StartsWith(HttpHelper.ContentTypeHeader))
                throw new InvalidDataException("Content type has invalid format");

            currentLine = reader.ReadFirstNonEmptyLineOrThrow("Status code is missed");

            var resultCode = BatchSerializeHelper.GetResultCodeOrThrow(currentLine);

            

            var content = BatchSerializeHelper.ReadUntilBoundaryOrThrow(reader, boundary);

            return new HttpResponse<string>(
                new ResponseInfo((HttpStatusCode) resultCode),
                content);
        }

     
    }
}