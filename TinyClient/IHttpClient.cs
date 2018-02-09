namespace TinyClient
{
    public interface IHttpClient
    {
        IHttpResponse Send(HttpClientRequest request);
    }
}