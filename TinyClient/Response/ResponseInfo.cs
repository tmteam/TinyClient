using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace TinyClient.Response
{
    public class ResponseInfo
    {
        public ResponseInfo(HttpStatusCode statusCode): this(null, new KeyValuePair<string, string>[0], statusCode )
        {

        }
        public ResponseInfo(Uri source, KeyValuePair<string, string>[] headers, HttpStatusCode statusCode)
        {
            Source = source;
            Headers = headers;
            StatusCode = statusCode;
        }

        public Uri Source { get; }

        public string GetHeaderValueOrNull(string headerName)
        {
            return Headers.FirstOrDefault(h => h.Key == headerName).Value;
        }
        public KeyValuePair<string, string>[] Headers { get; }
        public HttpStatusCode StatusCode { get; }
       
    }
}
