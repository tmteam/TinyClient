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
        [Test]
        public void CreateWithSingleEmptyGetRequest_WriteTo_ResultIsAsExpected()
        {
            string boundary = "theBoundary";
            string host = "http://www.ya.ru:9090/";
            string query = "myquery";
            var getRequest = HttpClientRequest.CreateGet(query);
            var multipartContent = new MultipartContent(new[] { getRequest }, boundary);
            string parsed = GetContentString(host, multipartContent);

            string expected = "--" + boundary + "\r\n" +
                              "Content-Type: application/http; msgtype=request\r\n\r\n" + //todo: why it is application/http?
                              $"GET /{query} HTTP/1.1\r\n" +
                              "Host: www.ya.ru:9090\r\n" +
                              "\r\n\r\n" + 
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

            string expected = "--" + boundary + "\r\n" +
                              "Content-Type: application/http; msgtype=request\r\n\r\n" + //todo: why it is application/http?
                              $"GET /{query1} HTTP/1.1\r\n" +
                              "Host: www.ya.ru\r\n" +
                              "\r\n" +
                              "\r\n" +
                              "--" + boundary+"\r\n"+
                              "Content-Type: application/http; msgtype=request\r\n\r\n" + //todo: why it is application/http?
                              $"GET /{query2} HTTP/1.1\r\n" +
                              "Host: www.ya.ru\r\n" +
                              "\r\n" +
                              "\r\n" +
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

            string expected = "--" + boundary + "\r\n" +
                              "Content-Type: application/http; msgtype=request\r\n\r\n" +
                              $"POST /{query} HTTP/1.1\r\n" +
                              "Host: www.ya.ru:9090\r\n" +
                              "Content-Type: application/json; charset=utf-8\r\n\r\n" +
                              vmSerialized+"\r\n"+
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


            string expected = "--" + boundary + "\r\n" +
                              "Content-Type: application/http; msgtype=request\r\n\r\n" +
                              $"PUT /{query1} HTTP/1.1\r\n" +
                              "Host: localhost:9090\r\n" +
                              "Content-Type: application/json; charset=utf-8\r\n\r\n" +
                              vm1Serialized + "\r\n" +
                              "--" + boundary + "\r\n"+
                              "Content-Type: application/http; msgtype=request\r\n\r\n" +
                              $"PUT /{query2} HTTP/1.1\r\n" +
                              "Host: localhost:9090\r\n" +
                              "Content-Type: application/json; charset=utf-8\r\n\r\n" +
                              vm2Serialized + "\r\n" +
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
