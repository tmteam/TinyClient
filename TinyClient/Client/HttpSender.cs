namespace TinyClient.Client
{
    /*
    internal class HttpOldWebSender : IHttpSender
    {
        private readonly string _host;

        public HttpOldWebSender(string host)
        {
            _host = host;
        }

        /// <exception cref="WebException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        public IHttpChannelResponse Send(HttpClientRequest request)
        {
            var webResponse = SendAndReceive(request);
            var deserializer = request.Deserializer;

            var responseHeaders = new Dictionary<string, string>();
            foreach (var key in webResponse.Headers.AllKeys)
                responseHeaders.Add(key, webResponse.Headers.Get(key));
            var responseInfo = new ResponseInfo(webResponse.ResponseUri, responseHeaders.ToArray(), webResponse.StatusCode);

            var deserialized =  deserializer.Deserialize(responseInfo, webResponse.GetResponseStream());
            webResponse.Close();
            return deserialized;
        }


        /// <exception cref="WebException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        private HttpWebResponse SendAndReceive(HttpClientRequest request)
        {
            byte[] data;
            var webRequest = CreateRequest(request, out data);
            if (request.KeepAlive != KeepAliveMode.UpToClient)
                webRequest.KeepAlive = request.KeepAlive == KeepAliveMode.True;

            if (request.KeepAlive == KeepAliveMode.True)
                webRequest.KeepAlive = true;
            else if (request.KeepAlive == KeepAliveMode.False)
                webRequest.KeepAlive = false;

            if (data != null)
            {
                using (var stream = webRequest.GetRequestStream())
                    stream.Write(data);
            }

            return (HttpWebResponse)webRequest.GetResponse();
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

            return webRequest;
        }
    }*/
}
