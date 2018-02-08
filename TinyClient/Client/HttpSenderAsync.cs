using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TinyClient.Response;

namespace TinyClient.Client
{
    public class HttpSenderAsync: IHttpSender
    {
        private readonly string _host;

        public HttpSenderAsync(string host)
        {
            _host = host;
        }

        /// <exception cref="WebException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        public IHttpResponse Send(HttpClientRequest request)
        {
            var res = SendAndReceive(request);
            try
            {
                if (request.Timeout.HasValue)
                {
                    if (!res.Wait(request.Timeout.Value))
                        throw new TimeoutException($"Request timeout of {request.Timeout.Value} is expired");
                }
                else
                    res.Wait();
            }
            catch (AggregateException aggregateException)
            {
                throw aggregateException.GetBaseException();
            }

            var webResponse = res.Result;

            var response = ToResponse(request, webResponse);

            webResponse.Close();
            return response;
        }

    
        /// <exception cref="WebException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        private Task<HttpWebResponse> SendAndReceive(HttpClientRequest request)
        {
            byte[] data;
            var webRequest = CreateRequest(request, out data);

            var asyncRequest = new AsyncRequest(webRequest, data);
            return asyncRequest.Send();
         
        }
        private HttpWebRequest CreateRequest(HttpClientRequest request, out byte[] data)
        {
            var uri = request.GetUriFor(_host);
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.Method = request.Method.Name;

            foreach (var header in request.CustomHeaders)
            {
                webRequest.Headers.Add(header.Key, header.Value);
            }
            data = null;
            if (request.Content != null)
            {
                data = request.Content.GetDataFor(uri);
                webRequest.ContentLength = data.Length;
                webRequest.ContentType = request.Content.ContentType;
            }

            if (request.KeepAlive != KeepAliveMode.UpToClient)
                webRequest.KeepAlive = request.KeepAlive == KeepAliveMode.True;

            if (request.KeepAlive == KeepAliveMode.True)
                webRequest.KeepAlive = true;
            else if (request.KeepAlive == KeepAliveMode.False)
                webRequest.KeepAlive = false;

            return webRequest;
        }
        private static IHttpResponse ToResponse(HttpClientRequest request, HttpWebResponse webResponse)
        {
            var deserializer = request.Deserializer;

            var responseHeaders = new Dictionary<string, string>();
            foreach (var key in webResponse.Headers.AllKeys)
                responseHeaders.Add(key, webResponse.Headers.Get(key));
            var responseInfo = new ResponseInfo(webResponse.ResponseUri, responseHeaders.ToArray(), webResponse.StatusCode);

            var deserialized = deserializer.Deserialize(responseInfo, webResponse.GetResponseStream());
            return deserialized;
        }



    }
}
