using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace TinyClient
{
    public class DeflateEncoder: IContentEncoder
    {
        public string EncodingType => "deflate";
        public Stream GetEncodingStream(Stream destination) {
            return new DeflateStream(destination, CompressionMode.Compress, true);
        }

        public Stream GetDecodingStream(Stream source) {
            return new DeflateStream(source, CompressionMode.Decompress, true);
        }
    }
}
