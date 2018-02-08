using System;
using System.Linq;
using NUnit.Framework;

namespace TinyClient.Tests
{
    [TestFixture]
    public class HttpChannelRequestTest
    {
        [Test]
        public void GetUriFor_querySpecified_UrlIsCorrect()
        {
            string subPath = "/search";
            var host = "http://www.ya.ru";
            var uri = HttpClientRequest.CreateGet(subPath).GetUriFor(host);

            Assert.AreEqual(host + subPath, uri.AbsoluteUri);
        }
        [Test]
        public void GetUriFor_queryIsEmpty_UriAbsoluteUrlEqualsHost()
        {
            var host = "http://www.ya.ru";
            var uri = HttpClientRequest.CreateGet().GetUriFor(host);
            Assert.AreEqual(host+"/", uri.AbsoluteUri);
        }
        [TestCase("foo","foo")]
        [TestCase(0, "0")]
        [TestCase(true, "True")]
        [TestCase(false, "False")]
        [TestCase("chuk i gek", "chuk+i+gek")]
        public void GetUriFor_AddUriParam_UrlIsCorrect(object paramValue, string paramValueInPath)
        {
            var host = "http://www.ya.ru";
            var paramName = "var";
            var uri = HttpClientRequest.CreateGet()
                .AddUriParam(paramName, paramValue).GetUriFor(host);


            Assert.AreEqual($"{host}/?{paramName}={paramValueInPath}", uri.AbsoluteUri);
        }
        [Test]
        public void SetMultipleUrlParam_GetUriFor_UriIsCorrect()
        {
            var host = "http://www.ya.ru";
            string[] paramNames = { "var1","var2","var3"};
            object[] paramValues = {"foo",0,false};

            var request = HttpClientRequest.CreateGet();

            for (int i = 0; i < paramNames.Length; i++)
                request = request.AddUriParam(paramNames[i], paramValues[i]);


            var uri = request.GetUriFor(host);

            Assert.AreEqual(
                expected: $"{host}/?{paramNames[0]}={paramValues[0]}&{paramNames[1]}={paramValues[1]}&{paramNames[2]}={paramValues[2]}",
                actual: uri.AbsoluteUri);
        }

        [Test]
        public void SetMultipleUrlParamAndSubQuery_GetUriFor_UriIsCorrect()
        {
            var host = "http://www.ya.ru";
            var subquery = "search";
            string[] paramNames = { "var1", "var2", "var3" };
            object[] paramValues = { "foo", 0, false };
            object[] paramValuesInQuery = { "foo", "0", "False"};

            var request = HttpClientRequest.CreateGet(subquery);

            for (int i = 0; i < paramNames.Length; i++)
            {
                request = request.AddUriParam(paramNames[i], paramValues[i]);
            }

            var uri = request.GetUriFor(host);
            Assert.AreEqual(
                expected: $"{host}/{subquery}?{paramNames[0]}={paramValuesInQuery[0]}&{paramNames[1]}={paramValuesInQuery[1]}&{paramNames[2]}={paramValuesInQuery[2]}",
                actual: uri.AbsoluteUri);
        }
        [Test]
        public void SetMethodName_BuildFor_MethodNameEquals()
        {
            var method = HttpMethod.Get;
            var request =  HttpClientRequest.Create(method);
            Assert.AreEqual(method.Name, request.Method.Name);
        }

        [Test]
        public void AddCustomHeader_BuildFor_CustomHeaderExists()
        {
            var name = "myHeader";
            var value = "myValue";
            var request = HttpClientRequest
                .CreateGet()
                .AddCustomHeader(name, value);

            var header = request.CustomHeaders.SingleOrDefault(c => c.Key == name);
            Assert.IsNotNull(header);
            Assert.AreEqual(value, header.Value);
        }

        private Uri FakeUri => new UriBuilder { Query = "lalal" }.Uri;
    }
}
