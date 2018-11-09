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
        private const string HttpNewLine = "\r\n";

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
                //Different operation systems have different default line endings
                //So we need to specify new line as "\r\n" according to 
                //Http specification
                var sb = new StringBuilder();
                sb.Append(BatchSerializeHelper.GetOpenBoundaryString(_boundary));
                sb.Append(HttpNewLine);
                sb.Append(HttpHelper.HttpRequestContentTypeHeaderString);
                sb.Append(HttpNewLine);
                sb.Append(HttpNewLine);
                sb.Append($"{request.Method.Name} {request.QueryAbsolutePath} {HttpHelper.Http11VersionCaption}");
                sb.Append(HttpNewLine);
                sb.Append($"Host: {host.Authority}");
                sb.Append(HttpNewLine);
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