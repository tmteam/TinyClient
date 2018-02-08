using System;
using System.Linq;
using TinyClient.Response;

namespace TinyClient
{
    public static class HttpClientRequestExtensions
    {
        public static HttpClientRequest SetMultipartContent(
            this HttpClientRequest request, 
            string boundary , 
            params HttpClientRequest[] requests)
        {
            var content = new MultipartContent(requests, boundary);
            request.SetContent(content);

            var serializers =requests.Select(r=> new TextResponseDeserialaizer()).ToArray();
            request.SetDeserializer(new MultipartRequestDeserializer(boundary, serializers));

            return request;
        }
        public static HttpClientRequest SetMultipartContent(
            this HttpClientRequest request,
            params HttpClientRequest[] requests)
            => SetMultipartContent(request, Guid.NewGuid().ToString(), requests);

        public static HttpClientRequest SetJsonContent(
            this HttpClientRequest request, 
            object serializeableObject) 
                => request.SetContent(new JsonContent(serializeableObject));
    }
}
