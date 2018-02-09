using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace TinyClient.Helpers
{
    public static class DeflateHelper
    {
        public static byte[] Compress(byte[] source)
        {
            var res = new MemoryStream();
            Compress(source, destination: res);
            return res.ToArray();
        }

        public static void Compress(byte[] source, Stream destination)
        {
                
            using (var compressed = new DeflateStream(destination, CompressionMode.Compress, true))
            {
                compressed.Write(source);
            }
            destination.Position = 0;
        }

        public static void Decompress(Stream source, Stream destination)
        {
            using (var decompressed = new DeflateStream(source, CompressionMode.Decompress, true))
            {
                decompressed.CopyTo(destination);
            }
            destination.Position = 0;
        }

        public const string EncodingType = "deflate";
    }
}
