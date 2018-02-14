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

        public string ContentType => HttpMediaTypes.Mixed+"; boundary="+_boundary;
        public void WriteTo(Stream stream, Uri host)
        {
            
            foreach (var request in SubRequests)
            {
                var sb = new StringBuilder();
                sb.AppendLine(BatchSerializeHelper.GetOpenBoundaryString(_boundary));
                sb.AppendLine(HttpHelper.HttpRequestContentTypeHeaderString);
                sb.AppendLine();
                sb.AppendLine($"{request.Method.Name} {request.QueryAbsolutePath} {HttpHelper.Http11VersionCaption}");
                sb.AppendLine($"Host: {host.Authority}");
                stream.WriteUtf8(sb.ToString());

                if (request.Content != null)
                {
                    stream.WriteUtf8($"{HttpHelper.ContentTypeHeader}: {request.Content.ContentType}\r\n\r\n");
                    request.Content.WriteTo(stream, host);
                }
                else
                {
                    stream.WriteUtf8("\r\n");
                }
                stream.WriteUtf8("\r\n");
            }
            stream.WriteUtf8(BatchSerializeHelper.GetCloseBoundaryString(_boundary));
        }
    }
}