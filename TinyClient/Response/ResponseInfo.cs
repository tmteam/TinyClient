using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace TinyClient.Response
{
    public class ResponseInfo
    {
        [Obsolete("ctor is deprecated. It will be removed in future releases")]
        public ResponseInfo(HttpStatusCode statusCode)
        {
            Source = null ;
            RequestUrl = "";
            Headers = new KeyValuePair<string, string>[0];
            StatusCode = statusCode;
        }
        [Obsolete("ctor is deprecated. It will be removed in future releases")]
        public ResponseInfo(Uri source, KeyValuePair<string, string>[] headers, HttpStatusCode statusCode)
        {
            Source = source;
            RequestUrl = "";
            Headers = headers;
            StatusCode = statusCode;
        }
        public ResponseInfo(Uri source, string requestUrl, KeyValuePair<string, string>[] headers, HttpStatusCode statusCode)
        {
            Source = source;
            RequestUrl = requestUrl;
            Headers = headers;
            StatusCode = statusCode;
        }
        /// <summary>
        /// (optional) Response uri
        /// </summary>
        public Uri Source { get; }
        
        /// <summary>
        /// Request url
        /// </summary>
        public string RequestUrl { get; }
        
        /// <summary>
        /// Returns specified header value or null if it not exists
        /// Case-sensivityy
        /// </summary>
        public string GetHeaderValueOrNull(string headerName) 
            => Headers.FirstOrDefault(h => h.Key == headerName).Value;
        
        /// <summary>
        /// Headers collection
        /// </summary>
        public KeyValuePair<string, string>[] Headers { get; }
        
        /// <summary>
        /// Response status code
        /// </summary>
        public HttpStatusCode StatusCode { get; }
       
    }
}
