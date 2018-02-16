using System;
using System.Net;

namespace TinyClient
{
    public class TinyTimeoutException : WebException
    {
        public TinyTimeoutException(string message) : base(message)
        {

        }
        public TinyTimeoutException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}