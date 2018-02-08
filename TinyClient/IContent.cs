using System;

namespace TinyClient
{
    public interface IContent
    {
        string ContentType { get; }
        byte[] GetDataFor(Uri host);
    }
}
