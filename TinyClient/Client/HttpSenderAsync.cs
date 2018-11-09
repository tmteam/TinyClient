using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TinyClient.Helpers;
using TinyClient.Response;

namespace TinyClient.Client
{
    public class HttpSenderAsync : IHttpSender
    {
        private const int MaxUndefinedTimeout = 60 * 60 * 1000;//1 Hour
        private readonly string _host;
        private readonly Dictionary<string, IContentEncoder> _decoders;
        private readonly Dictionary<string, Action<string, HttpWebRequest>> _specialHeadersMap;
        /// <summary>
        /// For testing
        /// </summary>
        public Func<HttpWebRequest, byte[], ITinyAsyncRequest> AsyncRequestFactory { get; set; } = AsyncRequest.Create;

        public Action<Exception> UnderlyingAsyncExceptionHandler { get; set; } = null;
        public HttpSenderAsync(string host, IEnumerable<IContentEncoder> decoders)
        {
            _host = host;
            _decoders = new Dictionary<string, IContentEncoder>();
            foreach (var contentEncoder in decoders)
            {
                _decoders.Add(contentEncoder.EncodingType, contentEncoder);
            }

            _specialHeadersMap = new Dictionary<string, Action<string, HttpWebRequest>>
            {
                { HttpHelper.UserAgentHeader,(value, webRequest)=> webRequest.UserAgent = value}
            };
        }

        /// <exception cref="WebException">Connection troubles</exception>
        /// <exception cref="InvalidDataException"></exception>
        public IHttpResponse Send(HttpClientRequest request)
        {
            HttpWebResponse webResponse = null;

            try
            {
                var webRequest = CreateRequest(request, out byte[] data);
                ITinyAsyncRequest asyncRequest = AsyncRequestFactory(webRequest, data);
                var res = asyncRequest.Send();

                try
                {
                    if (request.Timeout.HasValue)
                    {
                        if (!res.Wait(request.Timeout.Value))
                        {
                            HandleTaskResults(res);

                            asyncRequest.Abort();
                            throw new TinyTimeoutException($"Request timeout of {request.Timeout.Value} is expired");
                        }
                    }
                    else
                        res.Wait();

                    webResponse = res.Result;
                }
                catch (AggregateException aggregateException)
                {
                    var originException = aggregateException.GetBaseException();

                    switch (originException)
                    {
                        case WebException webEx:
                            webResponse = webEx.Response as HttpWebResponse ?? throw originException;
                            break;
                        case TimeoutException _:
                            throw new TinyTimeoutException(originException.Message);
                        default:
                            throw originException;
                    }
                }

                var response = ToResponse(request, webResponse);
                return response;
            }
            finally
            {
                webResponse?.Close();
            }
        }

        private void HandleTaskResults(Task<HttpWebResponse> res)
        {
            res.ContinueWith(t =>
            {
                var e = t.Exception?.GetBaseException();
                if (e != null)
                    UnderlyingAsyncExceptionHandler?.Invoke(e);
            }, TaskContinuationOptions.OnlyOnFaulted);

            res.ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    try
                    {
                        task.Result?.Close();
                    }
                    catch (Exception e)
                    {
                        UnderlyingAsyncExceptionHandler?.Invoke(e);
                    }
                }
            }, TaskContinuationOptions.NotOnFaulted);
        }

        /// <exception cref="InvalidDataException">Request serialization error</exception>
        private HttpWebRequest CreateRequest(HttpClientRequest request, out byte[] data)
        {
            var uri = request.GetUriFor(_host);
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.Method = request.Method.Name;
            if (request.Timeout.HasValue)
                webRequest.Timeout = (int) request.Timeout.Value.TotalMilliseconds;
            else
                webRequest.Timeout = MaxUndefinedTimeout;

            if (request.Encoder != null)
                webRequest.Headers.Add(HttpHelper.ContentEncodingHeader, request.Encoder.EncodingType);

            if (request.Decoder != null)
                webRequest.Headers.Add(HttpHelper.AcceptEncodingHeader, request.Decoder.EncodingType);
            
            foreach (var header in request.CustomHeaders)
            {
                if (_specialHeadersMap.ContainsKey(header.Key))
                    _specialHeadersMap[header.Key](header.Value, webRequest);
                else
                    webRequest.Headers.Add(header.Key, header.Value);
            }

            data = null;
            if (request.Content != null)
            {
                data = request.GetData(uri);
                webRequest.ContentLength = data.Length;
                webRequest.ContentType = request.Content.ContentType;
            }

            if (request.KeepAlive != KeepAliveMode.UpToClient)
                webRequest.KeepAlive = request.KeepAlive == KeepAliveMode.True;

            webRequest.ServicePoint.Expect100Continue = false;
            return webRequest;
        }
        /// <exception cref="InvalidDataException"></exception>
        private IHttpResponse ToResponse(HttpClientRequest request, HttpWebResponse webResponse)
        {
            try
            {
                var responseHeaders = new Dictionary<string, string>();
                foreach (var key in webResponse.Headers.AllKeys)
                    responseHeaders.Add(key, webResponse.Headers.Get(key));
                
                var responseInfo = new ResponseInfo(
                    source:     webResponse.ResponseUri, 
                    requestUrl: request.QueryAbsolutePath,
                    headers:    responseHeaders.ToArray(), 
                    statusCode: webResponse.StatusCode);

                var stream = webResponse.GetResponseStream();

                if (responseHeaders.ContainsKey(HttpHelper.ContentEncodingHeader))
                {
                    var encodingType = responseHeaders[HttpHelper.ContentEncodingHeader];
                    if (_decoders.ContainsKey(encodingType))
                        stream = _decoders[encodingType].GetDecodingStream(stream);
                }

                IResponseDeserializer deserializer;

                if ((int)webResponse.StatusCode < 200 || (int)webResponse.StatusCode >= 300)
                    deserializer = new AutoResponseDeserializer();
                else
                    deserializer = request.Deserializer;

                var deserialized = deserializer.Deserialize(responseInfo, stream);
                return deserialized;
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Response deserialization error: " + e.Message, e);
            }

        }



    }
}
