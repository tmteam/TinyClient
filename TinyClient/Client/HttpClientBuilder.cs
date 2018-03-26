using System;
using System.Collections.Generic;

namespace TinyClient.Client
{
  
    public class HttpClientBuilder
    {
        private readonly List<IContentEncoder> _decoders = new List<IContentEncoder>();

        public string Host { get; }
        public bool KeepAlive { get; private set; } = true;
        public IHttpSender Sender { get; private set; }
        public Func<HttpClientRequest, HttpClientRequest> RequestMiddleware { get; private set; }= null;
        public Func<IHttpResponse, IHttpResponse> ResponseMiddleware { get; private set; } = null;
        public TimeSpan? Timeout { get; private set; } = TimeSpan.FromSeconds(10);
        public IEnumerable<IContentEncoder> Decoders => _decoders;

        public Action<Exception> AsyncExceptionsHandler => _asyncExceptionsHandler;

        private Action<Exception> _asyncExceptionsHandler;
        public HttpClientBuilder(string host)
        {
            Host = host;
        }


        public HttpClientBuilder WithCustomDecoder(IContentEncoder decoder)
        {
            _decoders.Add(decoder);
            return this;
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


        public HttpClientBuilder WithUnderlyingAsyncExceptionHandler(Action<Exception> asyncExceptionsHandler)
        {
            _asyncExceptionsHandler = asyncExceptionsHandler;
            return this;
        }

        public HttpClient Build()
        {
            return new HttpClient(this);
        }
    }
}
