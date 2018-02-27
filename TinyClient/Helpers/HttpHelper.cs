using System.Net;

namespace TinyClient.Helpers
{
    public static class HttpHelper
    {
        public const string ContentTypeHeader = "Content-Type";
        public const string ContentEncodingHeader = "Content-Encoding";
        public const string AcceptEncodingHeader = "Accept-Encoding";
        public static string UserAgentHeader = "User-Agent";

        public static string HttpRequestContentTypeHeaderString => $"{HttpHelper.ContentTypeHeader}: {HttpMediaTypes.Http}; msgtype=request";
        public static string Http11VersionCaption = "HTTP/1.1";


    }
}
