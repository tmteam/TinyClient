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

        public static HttpClient CreateWithCustomSender(IHttpSender sender, string host, bool keepAlive) 
            => new HttpClient(sender,host, keepAlive);

        private HttpClient(IHttpSender sender, string host, bool keepAlive)
        {
            _host = host;
            _keepAlive = keepAlive;
            _sender = sender;
        }
        public HttpClient(string host, bool keepAlive = true)
        {
            _host = host;
            _keepAlive = keepAlive;
            _sender = new HttpSenderAsync(host);
        }

        public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(10);

        private Func<HttpClientRequest, HttpClientRequest> _requestPreprocessor = null;

        private Func<IHttpResponse, IHttpResponse> _responsePreprocessor = null;

        public HttpClient PreprocessRequestsWith(Func<HttpClientRequest, HttpClientRequest> preprocess) {
            _requestPreprocessor = preprocess;
            return this;
        }

        public HttpClient PreprocessResponseWith(Func<IHttpResponse, IHttpResponse> reaction) {
            _responsePreprocessor = reaction;
            return this;
        }


        /// <exception cref="WebException"></exception>
        /// <exception cref="InvalidDataException"></exception>
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
