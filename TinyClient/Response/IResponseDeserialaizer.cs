using System.IO;

namespace TinyClient.Response
{
    public interface IResponseDeserializer
    {
        IHttpResponse Deserialize(ResponseInfo responseInfo, Stream dataStream);
    }

}
