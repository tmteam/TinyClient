using System;
using System.Collections.Generic;
using System.Net;

namespace TinyClient
{
    public interface IHttpResponse
    {
        string RequestUrl { get; }
        Uri Source { get; }
        KeyValuePair<string, string>[] Headers { get; }
        HttpStatusCode StatusCode { get; }
    }
}
