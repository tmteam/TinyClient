using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using TinyClient.Helpers;
using TinyClient.Response;

namespace TinyClient
{
    public class HttpClientRequest
    {
        #region statics
        public static HttpClientRequest Create(HttpMethod method, string subQuery = "") 
            => new HttpClientRequest(method, subQuery);

        public static HttpClientRequest CreateGet(string subQuery) 
            => Create(HttpMethod.Get, subQuery);
        public static HttpClientRequest CreateGet()
            => CreateGet( "");

        public static HttpClientRequest CreatePost(string subQuery)
            => Create(HttpMethod.Post, subQuery);
        public static HttpClientRequest CreateJsonPost(string subQuery, object content) 
            => CreatePost(subQuery).SetContent(new JsonContent(content));
        public static HttpClientRequest CreateJsonPost(object content) => CreateJsonPost("", content);


        public static HttpClientRequest CreatePut(string subQuery)
            => Create(HttpMethod.Put, subQuery);
        public static HttpClientRequest CreateJsonPut(string subQuery, object content)
            => CreatePut(subQuery).SetContent(new JsonContent(content));
        public static HttpClientRequest CreateJsonPut(object content) => CreateJsonPut("", content);

        public static HttpClientRequest CreateDelete(string subQuery)
            => Create(HttpMethod.Delete, subQuery);
        public static HttpClientRequest CreateJsonDelete(string subQuery, object content)
            => CreateDelete(subQuery).SetContent(new JsonContent(content));
        public static HttpClientRequest CreateJsonDelete(object content) => CreateJsonDelete("", content);

        #endregion
        
        private readonly Dictionary<string, string> headers = new Dictionary<string, string>();
        private readonly Dictionary<string, string> queryParams = new Dictionary<string, string>();

        public HttpClientRequest(HttpMethod method, string subQuery)
        {
            Method = method;
            this.Query = subQuery;
            Content = null;
            Deserializer = new AutoResponseDeserializer();
        }
        public HttpMethod Method { get; }
        public IContent Content { get; private set; }
        public IResponseDeserializer Deserializer { get; private set; }
        public KeepAliveMode KeepAlive { get; private set; } = KeepAliveMode.UpToClient;
        public string Query { get; private set; }
        public KeyValuePair<string, string>[] CustomHeaders => headers.ToArray();
        public TimeSpan? Timeout { get; private set; }


        /// <summary>
        /// Adds or replace custom request header.
        /// </summary>
        public HttpClientRequest AddCustomHeader(string key, string value)
        {
            headers[key] = value;
            return this;
        }

        /// <summary>
        /// Adds custom request header.
        /// </summary>
        /// <exception cref="ArgumentException">header is already exist</exception>
        public HttpClientRequest AddCustomHeaderOrThrow(string key, string value)
        {
            headers.Add(key,value);
            return this;
        }

        public HttpClientRequest SetKeepAlive(bool keepAlive)
        {
            KeepAlive = keepAlive ? KeepAliveMode.True : KeepAliveMode.False;
            return this;
        }

        public HttpClientRequest SetDeserializer(IResponseDeserializer specificDeserializer) {
            this.Deserializer = specificDeserializer;
            return this;
        }


        public HttpClientRequest SetContent(IContent content) {
            this.Content = content;
            return this;
        }

        public HttpClientRequest SetTimeout(TimeSpan timeout) {
            this.Timeout = timeout;
            return this;
        }

        public HttpClientRequest AddUriParam(string paramName, object paramValue)
        {
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentNullException(nameof(paramName));
            if (paramValue == null)
                throw new ArgumentNullException(nameof(paramValue));

            queryParams[paramName] = SerializeUriParam(paramValue);
            return this;
        }

        private static string SerializeUriParam(object paramValue)
        {
            var type = paramValue.GetType();
            // Для енумов отдельная обработка, т.к. стандартный метод форматирования не делает camelCase.
            // Форматируем средствами Json.Net.
            if (type.IsEnum)
                return JsonHelper.Serialize(paramValue);

            // Для даты/времени также отдельная обработка, т.к. стандартный метод форматирования двет некорректный
            // результат (не сериализуется часовой пояс).

            if (type == typeof(DateTime))
            {
                var dt = (DateTime)paramValue;
                return dt.ToString(JsonHelper.DateTimeFormatString);
            }

            var convertible = paramValue as IConvertible;
            return convertible?.ToString(CultureInfo.InvariantCulture) ?? paramValue.ToString();
        }
        public string QueryAbsolutePath 
        {
            get { throw new InvalidOperationException(); }
        }

        public Uri GetUriFor(string host) {
          
            return UriHelper.BuildUri(host, Query, queryParams);
        }
        /*
        public Uri GetUriFor(Uri host)
        {
            var uriBuilder = new UriBuilder(host);

            if (!string.IsNullOrEmpty(subQuery))
                uriBuilder.Path = VirtualPathUtility.Combine(uriBuilder.Path, subQuery);

            if (queryParams.Any())
            {
                var collection = HttpUtility.ParseQueryString(string.Empty);
                foreach (var kvp in queryParams)
                    collection.Add(kvp.Key, kvp.Value);


                uriBuilder.Query = collection.ToString();
            }

            return uriBuilder.Uri;
        }*/
    }

    public enum KeepAliveMode
    {
        True,
        False,
        UpToClient
    }
}
