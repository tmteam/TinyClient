using System;
using System.IO;
using System.Net;
using TinyClient.Client;

namespace TinyClient
{
    public class HttpClient
    {
        private readonly string _host;
        private readonly bool _keepAlive;
        private readonly IHttpSender _sender;

        public static HttpClientBuilder Create(string host) => new HttpClientBuilder(host);

        public HttpClient(HttpClientBuilder builder)
        {
            _host = builder.Host;
            _keepAlive = builder.KeepAlive;
            _sender = builder.Sender?? new HttpSenderAsync(_host);
            Timeout = builder.Timeout;
            _requestPreprocessor = builder.RequestMiddleware;
            _responsePreprocessor = builder.ResponseMiddleware;

        }
        public HttpClient(string host, bool keepAlive = true)
        {
            _host = host;
            _keepAlive = keepAlive;
            _sender = new HttpSenderAsync(host);
        }

        public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(10);

        private readonly Func<HttpClientRequest, HttpClientRequest> _requestPreprocessor = null;

        private readonly Func<IHttpResponse, IHttpResponse> _responsePreprocessor = null;



        /// <exception cref="WebException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public IHttpResponse Send(HttpClientRequest request)
        {
            if (!request.Timeout.HasValue && Timeout.HasValue)
                request.SetTimeout(Timeout.Value);

            if (request.KeepAlive== KeepAliveMode.UpToClient)
                request.SetKeepAlive(_keepAlive);

            if (_requestPreprocessor != null)
                request = _requestPreprocessor(request);



            var response = _sender.Send(request );

            if (_responsePreprocessor != null)
                response = _responsePreprocessor(response);

            return response;
        }
    }
}
