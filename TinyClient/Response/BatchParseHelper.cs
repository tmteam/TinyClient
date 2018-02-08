using System;
using System.IO;
using System.Text;
using TinyClient.Helpers;

namespace TinyClient.Response
{
    public static class BatchParseHelper
    {
        public static string GetOpenBoundaryString(string boundary) => "--" + boundary;
        public static string GetCloseBoundaryString(string boundary) => "--" + boundary + "--";

        public static int GetResultCodeOrThrow(string str)
        {
            str = str.Trim();

            if (!str.StartsWith(HttpHelper.Http11VersionCaption))
                throw new InvalidDataException();

            var parsed = str.Remove(0, HttpHelper.Http11VersionCaption.Length)
                .Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            var resultCode = 0;
            if (!Int32.TryParse(parsed[0], out resultCode))
                throw new InvalidDataException();
            return resultCode;
        }

       

     

        public static string ReadUntilBoundaryOrThrow(PeekableStreamReader reader, string boundary)
        {
            //Вычитываем все пустые строки перед контентом
            if(!reader.ReadUntilFirstNonEmptyLine())
                throw new InvalidDataException("Close boundary or content not found");

            //Теперь читаем строки пока не получим боундари
            var stringBuilder = new StringBuilder();
            bool hasOneLine = false;
            while (true)
            {
                if (reader.EndOfStream)
                    throw new InvalidDataException("Close boundary not found");

                if (reader.PeekLine().StartsWith(BatchParseHelper.GetOpenBoundaryString(boundary)))
                    return stringBuilder.ToString();
                else
                {
                    stringBuilder.Append((hasOneLine ? "\r\n" : String.Empty) + reader.ReadLine());
                    hasOneLine = true;
                }
            }
        }
    }
}
