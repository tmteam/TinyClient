using TinyClient.Response;

namespace TinyClient.Client
{
    public interface IHttpSender
    {
        IHttpResponse Send(HttpClientRequest request);
    }
}