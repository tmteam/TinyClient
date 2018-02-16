using System;
using System.Net;

namespace TinyClient
{
    public class TinyHttpException : WebException
    {
        public HttpStatusCode Code => HttpResponse.StatusCode;
        public IHttpResponse HttpResponse { get; }

        public TinyHttpException(IHttpResponse response, Exception innerException) : base(
            "Http request failed. Response status: " + response.StatusCode, innerException) {
            this.HttpResponse = response;
        }

        public TinyHttpException(IHttpResponse response, string message, Exception innerException, WebResponse webResponse) :base(message, innerException, WebExceptionStatus.ProtocolError, webResponse)
        {
            this.HttpResponse = response;
        } 
        public TinyHttpException(IHttpResponse response, string message, Exception innerException) : base(
            message, innerException)
        {
            this.HttpResponse = response;
        }

        public TinyHttpException(IHttpResponse response, string message)
            : base(message)
        {
            this.HttpResponse = response;
        }
        public TinyHttpException(IHttpResponse response):base("Http request failed. Response status: "+ response.StatusCode)
        {
            this.HttpResponse = response;
        }
    }
}