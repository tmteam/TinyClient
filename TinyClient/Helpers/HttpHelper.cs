using System.IO;

namespace TinyClient.Helpers
{
    public static class HttpHelper
    {
        public const string ContentTypeHeader = "Content-Type";
        public static string HttpRequestContentTypeHeaderString => $"{HttpHelper.ContentTypeHeader}: {HttpMediaTypes.Http}; msgtype=request";
        public static string Http11VersionCaption = "HTTP/1.1";
    

        public static void Write(this Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }
    }
}
