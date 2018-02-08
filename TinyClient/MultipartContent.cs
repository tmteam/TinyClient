using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TinyClient.Helpers;
using TinyClient.Response;

namespace TinyClient
{
    public class MultipartContent : IContent
    {
        public HttpClientRequest[] SubRequests { get; }
        private readonly string _boundary;

        public MultipartContent(HttpClientRequest[] subRequests, string boundary)
        {
            SubRequests = subRequests;
            _boundary = boundary;
        }

        public string ContentType => HttpMediaTypes.Mixed;
        public byte[] GetDataFor(Uri host)
        {
            var stream = new MemoryStream();
            var sb = new StringBuilder();
            foreach (var request in SubRequests)
            {

                sb.AppendLine(BatchParseHelper.GetOpenBoundaryString(_boundary));
                sb.AppendLine(HttpHelper.HttpRequestContentTypeHeaderString);
                sb.AppendLine();
                sb.AppendLine($"{request.Method.Name} {request.QueryAbsolutePath} {HttpHelper.Http11VersionCaption}");
                sb.AppendLine($"Host: {host.Host}");
                sb.AppendLine();

                stream.Write(Encoding.UTF8.GetBytes(sb.ToString()));

                stream.Write(request.Content.GetDataFor(host));
            }
            stream.Write(Encoding.UTF8.GetBytes("\r\n--" + _boundary));
            return stream.ToArray();
        }
    }
}