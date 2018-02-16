using System.IO;
using System.Net;
using TinyClient.Response;

namespace TinyClient.Client
{
    public interface IHttpSender
    {
        /// <exception cref="WebException">Connection troubles</exception>
        /// <exception cref="InvalidDataException"></exception>
        IHttpResponse Send(HttpClientRequest request);
    }
}