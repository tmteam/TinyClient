using System.IO;

namespace TinyClient.Response
{
    public class EmptyResponseDeserializer: IResponseDeserializer
    {
        public IHttpResponse Deserialize(ResponseInfo responseInfo, Stream dataStream)
        {
            return new EmptyResponse(responseInfo);
        }
    }
}
