using System;
using System.Net;

namespace TinyClient
{
    public class TinyHttpException : Exception
    {
        public HttpStatusCode Code { get; }
        public IHttpResponse Response { get; }


        public TinyHttpException(HttpStatusCode code, IHttpResponse response, string message)
            : base(message)
        {
            this.Code = code;
            this.Response = response;
        }
        public TinyHttpException(HttpStatusCode code, IHttpResponse response):base("Http request failed. Response status: "+ code)
        {
            this.Code = code;
            this.Response = response;
        }
    }
}