using System.IO;

namespace TinyClient.Response
{
    public interface IResponseDeserializer
    {
        /// <exception cref="InvalidDataException"></exception>
        IHttpResponse Deserialize(ResponseInfo responseInfo, Stream dataStream);
    }

}
