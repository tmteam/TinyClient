using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework.Internal;
using NUnit.Framework;
using TinyClient.Helpers;

namespace TinyClient.Tests
{
    [TestFixture]
    public class MultipartContentTest
    {
        private const string CRLF = "\r\n";

        [Test]
        public void CreateWithSingleEmptyGetRequest_WriteTo_ResultIsAsExpected()
        {
            string boundary = "theBoundary";
            string host = "http://www.ya.ru:9090/";
            string query = "myquery";
            var getRequest = HttpClientRequest.CreateGet(query);
            var multipartContent = new MultipartContent(new[] { getRequest }, boundary);
            string parsed = GetContentString(host, multipartContent);

            string expected = "--" + boundary + CRLF +
                              $"Content-Type: application/http; msgtype=request{CRLF}{CRLF}" + 
                              $"GET /{query} HTTP/1.1{CRLF}" +
                              $"Host: www.ya.ru:9090{CRLF}" +
                              $"{CRLF}{CRLF}" + 
                              "--" + boundary + "--";
            Assert.AreEqual(expected, parsed);
        }


        [Test]
        public void CreateWithTwoEmptyGetRequests_WriteTo_ResultIsAsExpected()
        {
            string boundary = "theBoundary";
            string host = "http://www.ya.ru/";
            string query1 = "myquery1";
            string query2 = "myquery2";

            var multipartContent = new MultipartContent(
                new[]
                {
                    HttpClientRequest.CreateGet(query1),
                    HttpClientRequest.CreateGet(query2)
                }, boundary);

            string parsed = GetContentString(host, multipartContent);

            string expected = "--" + boundary + CRLF +
                              $"Content-Type: application/http; msgtype=request{CRLF}{CRLF}" + 
                              $"GET /{query1} HTTP/1.1{CRLF}" +
                              $"Host: www.ya.ru{CRLF}{CRLF}{CRLF}" +
                              $"--" + boundary+CRLF+
                              $"Content-Type: application/http; msgtype=request{CRLF}{CRLF}" + 
                              $"GET /{query2} HTTP/1.1{CRLF}" +
                              $"Host: www.ya.ru{CRLF}{CRLF}{CRLF}" +
                              
                              "--" + boundary + "--";
            Assert.AreEqual(expected, parsed);
        }

        [Test]
        public void CreateWithSingleJsonPostRequest_WriteTo_ResultIsAsExpected()
        {
            string boundary = "theBoundary";
            string host = "http://www.ya.ru:9090/";
            string query = "myquery";

            var vm = new VmMock
            {
                Age = 42,
                Name = "Foo"
            };

            var getRequest = HttpClientRequest.CreatePost(query).SetJsonContent(vm);
            var multipartContent = new MultipartContent(new[] { getRequest }, boundary);
            string parsed = GetContentString(host, multipartContent);


            string vmSerialized = JsonHelper.Serialize(vm);

            string expected = "--" + boundary + CRLF +
                              $"Content-Type: application/http; msgtype=request{CRLF}{CRLF}" +
                              $"POST /{query} HTTP/1.1{CRLF}" +
                              $"Host: www.ya.ru:9090{CRLF}" +
                              $"Content-Type: application/json; charset=utf-8{CRLF}{CRLF}" +
                              vmSerialized+CRLF+
                              "--" + boundary + "--";
            Assert.AreEqual(expected, parsed);
        }

        [Test]
        public void CreateWithTwoJsonPutRequests_WriteTo_ResultIsAsExpected()
        {
            string boundary = "theBoundary";
            string host = "http://localhost:9090/";
            string query1 = "myquery1";
            string query2 = "myquery2";


            var vm1 = new VmMock
            {
                Age = 42,
                Name = "Foo"
            };

            var vm2 = new VmMock
            {
                Age = 99,
                Name = "Bzingo"
            };

            var request1 = HttpClientRequest.CreatePut(query1).SetJsonContent(vm1);
            var request2 = HttpClientRequest.CreatePut(query2).SetJsonContent(vm2);

            var multipartContent = new MultipartContent(new[] { request1, request2 }, boundary);
            string parsed = GetContentString(host, multipartContent);
            

            string vm1Serialized = JsonHelper.Serialize(vm1);
            string vm2Serialized = JsonHelper.Serialize(vm2);


            string expected = "--" + boundary + CRLF +
                              $"Content-Type: application/http; msgtype=request{CRLF}{CRLF}" +
                              $"PUT /{query1} HTTP/1.1{CRLF}" +
                              $"Host: localhost:9090{CRLF}" +
                              $"Content-Type: application/json; charset=utf-8{CRLF}{CRLF}" +
                              vm1Serialized + CRLF +
                              "--" + boundary + CRLF+
                              "Content-Type: application/http; msgtype=request"+ CRLF+ CRLF +
                              $"PUT /{query2} HTTP/1.1{CRLF}" +
                              $"Host: localhost:9090{CRLF}" +
                              "Content-Type: application/json; charset=utf-8" + CRLF+ CRLF+
                              vm2Serialized + CRLF +
                              "--" + boundary + "--";
            Assert.AreEqual(expected, parsed);
        }


        [Test]
        public void CreateWithBoundary_CountentTypeIsCorrect()
        {
            string boundary = "theBoundary";
            var getRequest = HttpClientRequest.CreateGet();
            var multipartContent = new MultipartContent(new[] { getRequest }, boundary);

            Assert.AreEqual($"multipart/mixed; boundary={boundary}", multipartContent.ContentType);
        }

        private static string GetContentString(string host, MultipartContent multipartContent)
        {
            var stream = new MemoryStream();
            multipartContent.WriteTo(stream, new Uri(host));
            var parsed = Encoding.UTF8.GetString(stream.ToArray());
            return parsed;
        }
    }

    public class VmMock
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
