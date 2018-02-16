using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using TinyClient.Helpers;

namespace TinyClient
{
    public static class HttpResponseExtensions
    {
        /// <exception cref="TinyHttpException"></exception>
        public static IHttpResponse ThrowIfFailed(this IHttpResponse response)
        {
            if(!response.IsSuccessStatusCode())
                throw new TinyHttpException(response);
            return response;
        }

        /// <exception cref="InvalidDataException"></exception>
        public static T GetJsonObject<T>(this IHttpResponse response)
        {
            var textResponse = response as HttpResponse<string>;
            if (textResponse == null)
                throw new InvalidDataException("Response contains no text");
            try
            {
                return JsonHelper.Deserialize<T>(textResponse.Content);
            }
            catch (JsonException e)
            {
                throw new InvalidDataException("Json deserialization failed: "+ e.Message, e);
            }
        }

        /// <exception cref="InvalidDataException"></exception>
        public static IHttpResponse[] GetMultipartResponse(this IHttpResponse response)
        {
            var multiResponse = response as HttpResponse<IHttpResponse[]>;
            if (multiResponse == null)
                throw new InvalidDataException("The returned content is not multipart");
            return multiResponse.Content;
        }

        public static string GetStringContentOrNull(this IHttpResponse response)
        {
            return (response as HttpResponse<string>)?.Content;
        }

        public static bool IsSuccessStatusCode(this IHttpResponse response)
        {
            int code = (int)response.StatusCode;
            return code >= 200 && code < 300;
        }
    }
}
