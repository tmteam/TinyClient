using System.Net;
using System.Threading.Tasks;

namespace TinyClient.Client
{
    public interface ITinyAsyncRequest
    {
        void Abort();
        Task<HttpWebResponse> Send();
    }
}