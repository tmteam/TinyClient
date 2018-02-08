using System;
using System.Collections.Generic;
using System.Net;
using TinyClient.Response;

namespace TinyClient
{
    public class HttpResponse<TContent> : IHttpResponse
    {
        private readonly ResponseInfo _responseInfo;

        public HttpResponse(ResponseInfo responseInfo, TContent content)
        {
            _responseInfo = responseInfo;
            Content = content;
        }

        public Uri Source => _responseInfo.Source;
        public KeyValuePair<string, string>[] Headers => _responseInfo.Headers;
        public HttpStatusCode StatusCode => _responseInfo.StatusCode;
        public TContent Content { get; }
    }
}