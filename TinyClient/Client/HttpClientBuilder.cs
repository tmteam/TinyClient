using System;

namespace TinyClient.Client
{
  
    public class HttpClientBuilder
    {
        public string Host { get; private set; }
        public bool KeepAlive { get; private set; } = true;
        public IHttpSender Sender { get; private set; }
        public Func<HttpClientRequest, HttpClientRequest> RequestMiddleware { get; private set; }= null;
        public Func<IHttpResponse, IHttpResponse> ResponseMiddleware { get; private set; } = null;
        public TimeSpan? Timeout { get; private set; } = TimeSpan.FromSeconds(10);

        public HttpClientBuilder(string host)
        {
            Host = host;
        }
        public HttpClientBuilder WithCustomSender(IHttpSender sender)
        {
            Sender = sender;
            return this;
        }

        public HttpClientBuilder WithRequestMiddleware(Func<HttpClientRequest, HttpClientRequest> requestMiddleware)
        {
            RequestMiddleware = requestMiddleware;
            return this;
        }

        public HttpClientBuilder WithResponseMiddleware(Func<IHttpResponse, IHttpResponse> responseMiddleware)
        {
            ResponseMiddleware = responseMiddleware;
            return this;

        }

        public HttpClientBuilder WithResponseMiddleware(Action<IHttpResponse> responseMiddleware)
        {
            return this.WithResponseMiddleware((r)=>
            {
                responseMiddleware(r);
                return r;
            });
        }

        public HttpClientBuilder WithKeepAlive(bool keepAlive)
        {
            KeepAlive = keepAlive;
            return this;

        }
        public HttpClientBuilder WithRequestTimeout(TimeSpan timeout)
        {
            Timeout = timeout;
            return this;
        }

        public HttpClient Build()
        {
            return new HttpClient(this);
        }
    }
}
