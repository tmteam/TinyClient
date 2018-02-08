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
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="JsonException"></exception>
        public static T AsJsonObject<T>(this IHttpResponse response)
        {
            var textResponse = response as HttpResponse<string>;
            if (textResponse == null)
                throw new ArgumentException("Response contains not text");
            return JsonHelper.Deserialize<T>(textResponse.Content);
        }

        /// <exception cref="InvalidDataException"></exception>
        public static IHttpResponse[] AsMultipartResponse(this IHttpResponse response)
        {
            var multiResponse = response as HttpResponse<IHttpResponse[]>;
            if (multiResponse == null)
                throw new InvalidDataException("The returned content is not multipart");
            return multiResponse.Content;
        }
    }
}
