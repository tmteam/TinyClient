using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TinyClient
{
    public interface IContentEncoder
    {
        string EncodingType { get; }

        Stream GetEncodingStream(Stream destination);

        Stream GetDecodingStream(Stream source);
    }
}
