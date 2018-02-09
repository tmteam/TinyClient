using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyClient
{
    public static class ClientEncoders
    {

        public static IContentEncoder Deflate { get; } = new DeflateEncoder();
    }
}
