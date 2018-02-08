using System.Collections.Generic;
using System.IO;
using System.Net;
using TinyClient.Helpers;

namespace TinyClient.Response
{
    

    public class MultipartRequestDeserializer : IResponseDeserializer
    {
        private readonly string _boundary;
        private readonly IResponseDeserializer[] _subresponseDeserializers;

        public MultipartRequestDeserializer(string boundary, IResponseDeserializer[] subresponseDeserializers)
        {
            _boundary = boundary;
            _subresponseDeserializers = subresponseDeserializers;
        }

        /// <exception cref="InvalidDataException"></exception>
        public IHttpResponse Deserialize(ResponseInfo responseInfo, Stream dataStream)
        {
            var subresponses = new List<IHttpResponse>(_subresponseDeserializers.Length);
            using (var reader = new PeekableStreamReader(dataStream))
            {
                if (reader.ReadUntilFirstNonEmptyLine())
                {
                    while (true)
                    {
                        var subrequest = ParseNextSubresponseFrom(reader);
                        subresponses.Add(subrequest);
                        if (reader.PeekLine() == BatchParseHelper.GetCloseBoundaryString(_boundary))
                            break;
                    }
                }
            }
            if(subresponses.Count!= _subresponseDeserializers.Length)
                throw new InvalidDataException($"Actual and expected items count are not equal. Actual: {subresponses.Count}, expected: {_subresponseDeserializers.Length}");

            return new HttpChannelResponse<IHttpResponse[]>(responseInfo, subresponses.ToArray());
        }

        /// <exception cref="InvalidDataException"></exception>
        private HttpChannelResponse<string> ParseNextSubresponseFrom(PeekableStreamReader reader)
        {
            var currentLine = reader.ReadFirstNonEmptyLine();
            if (currentLine != BatchParseHelper.GetOpenBoundaryString(_boundary))
                throw new InvalidDataException("Boundary missed");

            currentLine = reader.ReadFirstNonEmptyLineOrThrow("Content type header not found");
            //  if (currentLine?.Trim() != HttpHelper.HttpRequestContentTypeHeaderString)
            if (!currentLine.Trim().StartsWith(HttpHelper.ContentTypeHeader))
                throw new InvalidDataException("Content type has invalid format");

            currentLine = reader.ReadFirstNonEmptyLineOrThrow("Status code is missed");

            var resultCode = BatchParseHelper.GetResultCodeOrThrow(currentLine);

            

            var content = BatchParseHelper.ReadUntilBoundaryOrThrow(reader, _boundary);

            return new HttpChannelResponse<string>(
                new ResponseInfo((HttpStatusCode) resultCode),
                content);
        }
    }
}