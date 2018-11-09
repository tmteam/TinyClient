using System;
using System.Net;

namespace TinyClient
{
    public class TinyHttpException : WebException
    {
        public HttpStatusCode Code => HttpResponse.StatusCode;
        public IHttpResponse HttpResponse { get; }

        public TinyHttpException(IHttpResponse response, Exception innerException) : base(
            CreateErrorMessage(response.StatusCode), innerException) {
            HttpResponse = response;
        }
        
        public TinyHttpException(
            IHttpResponse response, 
            string message, 
            Exception innerException, 
            WebResponse webResponse) 
            :base(message, innerException, WebExceptionStatus.ProtocolError, webResponse) 
            => HttpResponse = response;

        public TinyHttpException(IHttpResponse response, string message, Exception innerException) 
            : base(message, innerException) 
            => HttpResponse = response;

        public TinyHttpException(IHttpResponse response, string message)
            : base(message) 
            => HttpResponse = response;

        public TinyHttpException(IHttpResponse response)
            :base(CreateErrorMessage(response.StatusCode)) 
            => HttpResponse = response;

        private static string CreateErrorMessage(HttpStatusCode statusCode) 
            => $"Http request failed. Response status: {(int)statusCode} {statusCode}";

    }
}