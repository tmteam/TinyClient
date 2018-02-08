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
        /// <exception cref="WebException">Ошибка при запросе</exception>
        /// <exception cref="JsonException">Ошибки сериализации или десериализации Json-объектов</exception>
        /// <exception cref="TimeoutException"></exception>
        public static IHttpResponse SendGet(this HttpClient client, string query) 
            => client.Send(HttpClientRequest.CreateGet(query));

        /// <exception cref="WebException">Ошибка при запросе</exception>
        /// <exception cref="JsonException">Ошибки сериализации или десериализации Json-объектов</exception>
        /// <exception cref="TimeoutException"></exception>
        public static IHttpResponse SendJsonPost(this HttpClient client, string query,
            object jsonSerializeableObject)
            => client.Send(HttpClientRequest.CreateJsonPost(query, jsonSerializeableObject));

        /// <exception cref="WebException">Ошибка при запросе</exception>
        /// <exception cref="JsonException">Ошибки сериализации или десериализации Json-объектов</exception>
        /// <exception cref="TimeoutException"></exception>
        public static IHttpResponse SendJsonPut(this HttpClient client, string query,
            object jsonSerializeableObject)
            => client.Send(HttpClientRequest.CreateJsonPut(query, jsonSerializeableObject));
        #endregion

        #region receive json
        /// <exception cref="WebException">Ошибка при запросе</exception>
        /// <exception cref="JsonException">Ошибки сериализации или десериализации Json-объектов</exception>
        /// <exception cref="InvalidDataException">Ошибка формата принятых данных </exception>
        /// <exception cref="TimeoutException"></exception>
        public static TResponseJsonObject GetAndReceiveJson<TResponseJsonObject>(this HttpClient client, string query) => client.SendAndReceiveJsonObject<TResponseJsonObject>(HttpClientRequest
            .CreateGet(query));

        /// <exception cref="WebException">Ошибка при запросе</exception>
        /// <exception cref="JsonException">Ошибки сериализации или десериализации Json-объектов</exception>
        /// <exception cref="InvalidDataException">Ошибка формата принятых данных </exception>
        /// <exception cref="TimeoutException"></exception>
        public static TResponseJsonObject PutAndReceiveJson<TResponseJsonObject>(this HttpClient client, string query,
            object jsonSerializeableContent) => client.SendAndReceiveJsonObject<TResponseJsonObject>(HttpClientRequest.CreateJsonPut(query, jsonSerializeableContent));

        /// <exception cref="WebException">Ошибка при запросе</exception>
        /// <exception cref="JsonException">Ошибки сериализации или десериализации Json-объектов</exception>
        /// <exception cref="InvalidDataException">Ошибка формата принятых данных </exception>
        /// <exception cref="TimeoutException"></exception>
        public static TResponseJsonObject PostAndReceiveJson<TResponseJsonObject>(this HttpClient client, string query,
            object jsonSerializeableContent) => client.SendAndReceiveJsonObject<TResponseJsonObject>(HttpClientRequest.CreateJsonPost(query, jsonSerializeableContent));

        /// <exception cref="WebException">Ошибка при запросе</exception>
        /// <exception cref="JsonException">Ошибки сериализации или десериализации Json-объектов</exception>
        /// <exception cref="InvalidDataException">Ошибка формата принятых данных </exception>
        /// <exception cref="TimeoutException"></exception>
        public static TJsonObject SendAndReceiveJsonObject<TJsonObject>(this HttpClient client,  HttpClientRequest request)
        {
            var response = client.Send(request);

            var stringResponse = response as HttpChannelResponse<string>;
            if (stringResponse == null)
                throw new InvalidDataException("Unexpected response format");
            return JsonHelper.Deserialize<TJsonObject>(stringResponse.Content);
        }
        #endregion

        #region receive text

        /// <exception cref="WebException">Ошибка при запросе</exception>
        /// <exception cref="JsonException">Ошибки сериализации или десериализации Json-объектов</exception>
        /// <exception cref="InvalidDataException">Ошибка формата принятых данных </exception>
        /// <exception cref="TimeoutException"></exception>
        public static HttpChannelResponse<string> GetAndReceiveText(this HttpClient client, string query) 
            => client.SendAndReceiveText(HttpClientRequest.CreateGet(query));

        /// <exception cref="WebException">Ошибка при запросе</exception>
        /// <exception cref="JsonException">Ошибки сериализации или десериализации Json-объектов</exception>
        /// <exception cref="InvalidDataException">Ошибка формата принятых данных </exception>
        /// <exception cref="TimeoutException"></exception>
        public static HttpChannelResponse<string> PutAndReceiveText<TResponseJsonObject>(this HttpClient client, string query,
            object jsonSerializeableContent) => client.SendAndReceiveText(HttpClientRequest.CreateJsonPut(query, jsonSerializeableContent));

        /// <exception cref="WebException">Ошибка при запросе</exception>
        /// <exception cref="JsonException">Ошибки сериализации или десериализации Json-объектов</exception>
        /// <exception cref="InvalidDataException">Ошибка формата принятых данных </exception>
        /// <exception cref="TimeoutException"></exception>
        public static HttpChannelResponse<string> PostAndReceiveText<TResponseJsonObject>(this HttpClient client, string query,
            object jsonSerializeableContent) => client.SendAndReceiveText(HttpClientRequest.CreateJsonPost(query, jsonSerializeableContent));


        /// <exception cref="WebException">Ошибка при запросе</exception>
        /// <exception cref="JsonException">Ошибки сериализации или десериализации Json-объектов</exception>
        /// <exception cref="InvalidDataException">Ошибка формата принятых данных </exception>
        /// <exception cref="TimeoutException"></exception>
        public static HttpChannelResponse<string> SendAndReceiveText(this HttpClient client, HttpClientRequest request)
        {
            var response = client.Send(request);

            var stringResponse = response as HttpChannelResponse<String>;
            if (stringResponse == null)
                throw new InvalidDataException("Unexpected response format");
            return stringResponse;
        }
        
        #endregion

    }
}
