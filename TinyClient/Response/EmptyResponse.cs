using System;
using System.Collections.Generic;
using System.Net;

namespace TinyClient.Response
{
    public class EmptyResponse: IHttpResponse
    {
        public EmptyResponse(ResponseInfo info)
        {
            Source = info.Source;
            Headers = info.Headers;
            StatusCode = info.StatusCode;
        }
        public Uri Source { get; }
        public KeyValuePair<string, string>[] Headers { get; }
        public HttpStatusCode StatusCode { get; }
    }
}