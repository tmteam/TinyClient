using System;
using System.IO;

namespace TinyClient
{
    public interface IContent
    {
        string ContentType { get; }
        void WriteTo(Stream stream, Uri host);
    }
}
