using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using TinyClient.Helpers;

namespace TinyClient
{
    public static class HttpClientExtensions
    {
        #region send
        /// <exception cref="WebException">Channel error</exception>
        /// <exception cref="TinyTimeoutException">Tiny client timeout</exception>
        /// <exception cref="InvalidDataException">Data format error</exception>
        public static IHttpResponse SendGet(this HttpClient client, string query) 
            => client.Send(HttpClientRequest.CreateGet(query));

        /// <exception cref="WebException">Channel error</exception>
        /// <exception cref="InvalidDataException">Data format error</exception>
        /// <exception cref="TinyTimeoutException">Tiny client timeout</exception>
        public static IHttpResponse SendJsonPost(this HttpClient client, string query,
            object jsonSerializeableObject)
            => client.Send(HttpClientRequest.CreateJsonPost(query, jsonSerializeableObject));

        /// <exception cref="WebException">Channel error</exception>
        /// <exception cref="InvalidDataException">Data format error</exception>
        /// <exception cref="TinyTimeoutException">Tiny client timeout</exception>
        public static IHttpResponse SendJsonPut(this HttpClient client, string query,
            object jsonSerializeableObject)
            => client.Send(HttpClientRequest.CreateJsonPut(query, jsonSerializeableObject));
        #endregion

        #region receive json
        /// <exception cref="WebException">Channel error</exception>
        /// <exception cref="TinyHttpException">Server side error</exception>
        /// <exception cref="InvalidDataException">Data format error</exception>
        /// <exception cref="TinyTimeoutException">Tiny client timeout</exception>
        public static TResponseJsonObject GetAndReceiveJson<TResponseJsonObject>(this HttpClient client, string query, bool throwIfNot2xx = true) 
            => client.SendAndReceiveJsonObject<TResponseJsonObject>(HttpClientRequest.CreateGet(query), throwIfNot2xx);

        /// <exception cref="WebException">Channel error</exception>
        /// <exception cref="TinyHttpException">Server side error</exception>
        /// <exception cref="InvalidDataException">Data format error</exception>
        /// <exception cref="TinyTimeoutException">Tiny client timeout</exception>
        public static TResponseJsonObject PutAndReceiveJson<TResponseJsonObject>(this HttpClient client, string query, object jsonSerializeableContent, bool throwIfNot2xx = true) 
            => client.SendAndReceiveJsonObject<TResponseJsonObject>(HttpClientRequest.CreateJsonPut(query, jsonSerializeableContent), throwIfNot2xx);

        /// <exception cref="WebException">Channel error</exception>
        /// <exception cref="TinyHttpException">Server side error</exception>
        /// <exception cref="InvalidDataException">Data format error</exception>
        /// <exception cref="TinyTimeoutException">Tiny client timeout</exception>
        public static TResponseJsonObject PostAndReceiveJson<TResponseJsonObject>(this HttpClient client, string query, object jsonSerializeableContent, bool throwIfNot2xx = true) 
            => client.SendAndReceiveJsonObject<TResponseJsonObject>(HttpClientRequest.CreateJsonPost(query, jsonSerializeableContent), throwIfNot2xx);

        /// <exception cref="WebException">Channel error</exception>
        /// <exception cref="TinyHttpException">Server side error</exception>
        /// <exception cref="InvalidDataException">Data format error</exception>
        /// <exception cref="TinyTimeoutException">Tiny client timeout</exception>
        public static TJsonObject SendAndReceiveJsonObject<TJsonObject>(this HttpClient client,  HttpClientRequest request, bool throwIfNot2xx = true)
        {
            var response = client.Send(request);
            if (throwIfNot2xx)
                response.ThrowIfNot2xx();

            var stringResponse = response as HttpResponse<string>;
            if (stringResponse == null)
                throw new InvalidDataException("Unexpected response format");
            try
            {
                return JsonHelper.Deserialize<TJsonObject>(stringResponse.Content);
            }
            catch (JsonException e)
            {
                throw new InvalidDataException("Json deserialization exception: "+e.Message, e);
            }
            
        }
        #endregion

        #region receive text

        /// <exception cref="WebException">Channel error</exception>
        /// <exception cref="TinyHttpException">Server side error</exception>
        /// <exception cref="InvalidDataException">Data format error</exception>
        /// <exception cref="TinyTimeoutException">Tiny client timeout</exception>
        public static HttpResponse<string> GetAndReceiveText(this HttpClient client, string query, bool throwIfNot2xx = true) 
            => client.SendAndReceiveText(HttpClientRequest.CreateGet(query), throwIfNot2xx);

        /// <exception cref="WebException">Channel error</exception>
        /// <exception cref="TinyHttpException">Server side error</exception>
        /// <exception cref="InvalidDataException">Data format error</exception>
        /// <exception cref="TinyTimeoutException">Tiny client timeout</exception>
        public static HttpResponse<string> PutAndReceiveText(this HttpClient client, string query,
            object jsonSerializeableContent, bool throwIfNot2xx = true) => client.SendAndReceiveText(HttpClientRequest.CreateJsonPut(query, jsonSerializeableContent), throwIfNot2xx);

        /// <exception cref="WebException">Channel error</exception>
        /// <exception cref="TinyHttpException">Server side error</exception>
        /// <exception cref="InvalidDataException">Data format error</exception>
        /// <exception cref="TinyTimeoutException">Tiny client timeout</exception>
        public static HttpResponse<string> PostAndReceiveText(this HttpClient client, string query,
            object jsonSerializeableContent, bool throwIfNot2xx = true) => client.SendAndReceiveText(HttpClientRequest.CreateJsonPost(query, jsonSerializeableContent), throwIfNot2xx);


        /// <exception cref="WebException">Channel error</exception>
        /// <exception cref="TinyHttpException">Server side error</exception>
        /// <exception cref="InvalidDataException">Data format error</exception>
        /// <exception cref="TinyTimeoutException">Tiny client timeout</exception>
        public static HttpResponse<string> SendAndReceiveText(this HttpClient client, HttpClientRequest request, bool throwIfNot2xx = true)
        {
            var response = client.Send(request);
            if (throwIfNot2xx)
                response.ThrowIfNot2xx();

            var stringResponse = response as HttpResponse<String>;
            if (stringResponse == null)
                throw new InvalidDataException("Unexpected response format");
            return stringResponse;
        }
        
        #endregion

    }
}
