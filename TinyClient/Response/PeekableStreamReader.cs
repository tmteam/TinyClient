using System;
using System.IO;

namespace TinyClient.Response
{
    public class PeekableStreamReader: IDisposable
    {
        private readonly StreamReader _underlying;
        private string bufferOrNull = null;

        public PeekableStreamReader(Stream stream): this(new  StreamReader(stream))
        {

        }

        public PeekableStreamReader(StreamReader underlying)
        {
            _underlying = underlying;
        }

        public bool EndOfStream => _underlying.EndOfStream && bufferOrNull==null;
        public string PeekLine()
        {
            if (bufferOrNull == null)
                bufferOrNull = _underlying.ReadLine();
            return bufferOrNull;
        }


        public string ReadLine()
        {
            if (bufferOrNull != null)
            {
                var ans = bufferOrNull;
                bufferOrNull = null;
                return ans;
            }
            return _underlying.ReadLine();
        }

        public string ReadFirstNonEmptyLine()
        {
            while (true)
            {
                if (EndOfStream)
                    return null;
                var str = ReadLine();
                if (!string.IsNullOrWhiteSpace(str))
                    return str;
            }
        }


        public bool ReadUntilFirstNonEmptyLine()
        {
            while (true)
            {
                if (EndOfStream)
                    return false;

                var str = PeekLine();

                if (!string.IsNullOrWhiteSpace(str))
                    return true;
                else
                    ReadLine();
            } 
        }

        public string ReadFirstNonEmptyLineOrThrow(string exceptionMessage)
        {
            var res = ReadFirstNonEmptyLine();
            if (string.IsNullOrWhiteSpace(res))
                throw new InvalidDataException(exceptionMessage);
            return res;
        }

        public void Dispose()
        {
            _underlying?.Dispose();
        }
    }
}
