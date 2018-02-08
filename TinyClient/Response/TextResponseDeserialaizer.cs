using System.IO;

namespace TinyClient.Response
{
    public class TextResponseDeserialaizer : IResponseDeserializer
    {
        public IHttpResponse Deserialize(ResponseInfo responseInfo, Stream dataStream)
        {
            using (var reader = new StreamReader(dataStream))
            {
                return new HttpResponse<string>(responseInfo, reader.ReadToEnd());
            }
        }
    }
}
