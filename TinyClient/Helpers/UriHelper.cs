using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TinyClient.Helpers
{
    public static class UriHelper
    {
        public static string Combine(params string[] paths)
        {
            if (!paths.Any())
                return string.Empty;
            if (paths.Length == 1)
                return paths.First();

            var ans = new StringBuilder();

            ans.Append(paths.First().TrimEnd('/') + "/");

            for (int i = 1; i < paths.Length-1; i++)
            {
                ans.Append(paths[i].Trim('/')+"/");
            }
            if(paths.Length>1)
                ans.Append(paths.Last().TrimStart('/'));
            return ans.ToString();
        }
        
        public static string SerializeUriParam(object paramValue)
        {
            var type = paramValue.GetType();
            // Для енумов отдельная обработка, т.к. стандартный метод форматирования не делает camelCase.
            // Форматируем средствами Json.Net.
            if (type.IsEnum)
                return JsonHelper.Serialize(paramValue);

            // Для даты/времени также отдельная обработка, т.к. стандартный метод форматирования двет некорректный
            // результат (не сериализуется часовой пояс).

            if (type == typeof(DateTime))
            {
                var dt = (DateTime)paramValue;
                return dt.ToString(JsonHelper.DateTimeFormatString);
            }

            var convertible = paramValue as IConvertible;
            return convertible?.ToString(CultureInfo.InvariantCulture) ?? paramValue.ToString();
        }
        
        public static string CreateQuery(IEnumerable<KeyValuePair<string, string>> uriParams)
        {
            return String.Join("&",
                uriParams.Select(kv => kv.Key + "=" + UrlEncode(kv.Value)));
        }

        public static string GetQuery(string subQuery, IEnumerable<KeyValuePair<string, string>> uriParams)
        {
            var path = subQuery;
            var queryParams = CreateQuery(uriParams);
            if (!string.IsNullOrWhiteSpace(queryParams))
            {
                if (!path.Contains('?'))
                    path += '?' + queryParams;
                else
                    path += '&' + queryParams;
            }

            return '/'+path.TrimStart('/');
        }
        
        public static Uri BuildUri(string host, string subQuery, IEnumerable<KeyValuePair<string, string>> uriParams)
        {
            var query = GetQuery(subQuery, uriParams);

            string path;
            if(!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(query))
                path = host.TrimEnd('/') + query;
            else
                path = host + subQuery;

            if (!path.Contains("://"))
                path = "http://" + path;
            try
            {
                return new Uri(path);
            }
            catch
            {
                throw new ArgumentException($"invalid uri. Host: {host}, query: {query}");
            }
        }
        
        public static void Write(this Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        public static string UrlEncode(string value)
        {
            if (value == null)
                return null;

            if (string.IsNullOrEmpty(value))
                return "";

            var bytes = Encoding.UTF8.GetBytes(value);
            return Encoding.UTF8.GetString(UrlEncode(bytes, 0, bytes.Length));
        }

        private static byte[] UrlEncode(byte[] bytes, int offset, int count)
        {
            int cSpaces = 0;
            int cUnsafe = 0;

            // count them first
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];

                if (ch == ' ')
                    cSpaces++;
                else if (!IsUrlSafeChar(ch))
                    cUnsafe++;
            }

            // nothing to expand?
            if (cSpaces == 0 && cUnsafe == 0)
            {
                // DevDiv 912606: respect "offset" and "count"
                if (offset == 0 && bytes.Length == count)
                    return bytes;

                var subarray = new byte[count];
                Buffer.BlockCopy(bytes, offset, subarray, 0, count);
                return subarray;
            }

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + cUnsafe * 2];
            int pos = 0;

            for (int i = 0; i < count; i++)
            {
                byte b = bytes[offset + i];
                char ch = (char)b;

                if (IsUrlSafeChar(ch))
                    expandedBytes[pos++] = b;
                else if (ch == ' ')
                    expandedBytes[pos++] = (byte)'+';
                else
                {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)IntToHex(b & 0x0f);
                }
            }

            return expandedBytes;
        }
        
        // Set of safe chars, from RFC 1738.4 minus '+'
        private static bool IsUrlSafeChar(char ch)
        {
            if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9')
                return true;

            switch (ch)
            {
                case '-':
                case '_':
                case '.':
                case '!':
                case '*':
                case '(':
                case ')':
                    return true;
            }

            return false;
        }
        
        private static char IntToHex(int n)
        {
            if (n <= 9)
                return (char)(n + (int)'0');
            else
                return (char)(n - 10 + (int)'A');
        }
    }
}