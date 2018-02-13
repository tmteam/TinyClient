using System;
using System.Linq;
using NUnit.Framework;

namespace TinyClient.Tests
{
    [TestFixture]
    public class HttpClientRequestTest
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

        [TestCase("http://www.ya.ru", "search")]
        [TestCase("https://www.ya.ru", "search")]
        [TestCase("http://www.ya.ru:9405", "search")]
        [TestCase("https://www.ya.ru:9405", "search")]
        [TestCase("http://localhost:9010/", "/search")]
        [TestCase("http://www.ya.ru", "/search")]
        [TestCase("https://www.ya.ru", "/search")]
        [TestCase("http://www.ya.ru:9405", "/search")]
        [TestCase("https://www.ya.ru:9405", "/search")]
        [TestCase("http://localhost:9010/", "/search")]
        [TestCase("http://www.ya.ru", "search/")]
        [TestCase("https://www.ya.ru", "search/")]
        [TestCase("http://www.ya.ru:9405", "search/")]
        [TestCase("https://www.ya.ru:9405", "search/")]
        [TestCase("http://localhost:9010/", "search/")]

        public void SetMultipleUrlParamAndSubQuery_GetUriFor_UriIsCorrect(string host, string subquery)
        {
            string[] paramNames = { "var1", "var2", "var3" };
            object[] paramValues = { "foo", 0, false };
            object[] paramValuesInQuery = { "foo", "0", "False"};

            var request = HttpClientRequest.CreateGet(subquery);

            for (int i = 0; i < paramNames.Length; i++)
            {
                request = request.AddUriParam(paramNames[i], paramValues[i]);
            }

            var uri = request.GetUriFor(host);
            var expected =
                $"{host.TrimEnd('/')}/{subquery.TrimStart('/')}?{paramNames[0]}={paramValuesInQuery[0]}&{paramNames[1]}={paramValuesInQuery[1]}&{paramNames[2]}={paramValuesInQuery[2]}";
            Assert.AreEqual(
                expected: expected,
                actual: uri.AbsoluteUri);
        }

        [TestCase("http://www.ya.ru", "api/session?userId=6567&userSecret=65676567")]
        [TestCase("https://www.ya.ru", "api/session?userId=6567&userSecret=65676567")]
        [TestCase("http://www.ya.ru:9405", "api/session?userId=6567&userSecret=65676567")]
        [TestCase("https://www.ya.ru:9405", "api/session?userId=6567&userSecret=65676567")]
        [TestCase("http://localhost:9010/", "/api/session?userId=6567&userSecret=65676567")]
        [TestCase("http://www.ya.ru", "/api/session?userId=6567&userSecret=65676567")]
        [TestCase("https://www.ya.ru", "/api/session?userId=6567&userSecret=65676567")]
        [TestCase("http://www.ya.ru:9405", "/api/session?userId=6567&userSecret=65676567")]
        [TestCase("https://www.ya.ru:9405", "/api/session?userId=6567&userSecret=65676567")]
        [TestCase("http://localhost:9010/", "/api/session?userId=6567&userSecret=65676567")]
        [TestCase("http://www.ya.ru", "api/session?userId=6567&userSecret=65676567/")]
        [TestCase("https://www.ya.ru", "api/session?userId=6567&userSecret=65676567/")]
        [TestCase("http://www.ya.ru:9405", "api/session?userId=6567&userSecret=65676567/")]
        [TestCase("https://www.ya.ru:9405", "api/session?userId=6567&userSecret=65676567/")]
        [TestCase("http://localhost:9010/", "api/session?userId=6567&userSecret=65676567/")]
        public void SetSubQuery_GetUriFor_UriIsCorrect(string host, string subquery)
        {
            var request = HttpClientRequest.CreateGet(subquery);

         
            var uri = request.GetUriFor(host);
            var expected =
                $"{host.TrimEnd('/')}/{subquery.TrimStart('/')}";
            Assert.AreEqual(
                expected: expected,
                actual: uri.AbsoluteUri);
        }


        [TestCase("ya.ru", "", "http://ya.ru/")]
        [TestCase("http://ya.ru", "", "http://ya.ru/")]
        [TestCase("https://ya.ru", "", "https://ya.ru/")]
        [TestCase("https://ya.ru/", "", "https://ya.ru/")]
        [TestCase("localhost", "", "http://localhost/")]
        [TestCase("localhost:9090", "", "http://localhost:9090/")]
        [TestCase("http://localhost:9090", "", "http://localhost:9090/")]
        [TestCase("http://localhost:9090/", "", "http://localhost:9090/")]
        [TestCase("localhost:9090/", "", "http://localhost:9090/")]

        [TestCase("ya.ru", "search", "http://ya.ru/search")]
        [TestCase("http://ya.ru", "search", "http://ya.ru/search")]
        [TestCase("https://ya.ru", "search", "https://ya.ru/search")]
        [TestCase("https://ya.ru/", "search", "https://ya.ru/search")]
        [TestCase("localhost", "search", "http://localhost/search")]
        [TestCase("localhost:9090", "search", "http://localhost:9090/search")]
        [TestCase("http://localhost:9090", "search", "http://localhost:9090/search")]
        [TestCase("http://localhost:9090/", "search", "http://localhost:9090/search")]
        [TestCase("localhost:9090/", "search", "http://localhost:9090/search")]

        [TestCase("ya.ru", "search/", "http://ya.ru/search/")]
        [TestCase("http://ya.ru", "search/", "http://ya.ru/search/")]
        [TestCase("https://ya.ru", "search/", "https://ya.ru/search/")]
        [TestCase("https://ya.ru/", "search/", "https://ya.ru/search/")]
        [TestCase("localhost", "search/", "http://localhost/search/")]
        [TestCase("localhost:9090", "search/", "http://localhost:9090/search/")]
        [TestCase("http://localhost:9090", "search/", "http://localhost:9090/search/")]
        [TestCase("http://localhost:9090/", "search/", "http://localhost:9090/search/")]
        [TestCase("localhost:9090/", "search/", "http://localhost:9090/search/")]

        [TestCase("ya.ru", "search?a=b%20c&d=e", "http://ya.ru/search?a=b%20c&d=e")]
        [TestCase("http://ya.ru", "search?a=b%20c&d=e", "http://ya.ru/search?a=b%20c&d=e")]
        [TestCase("https://ya.ru", "search?a=b%20c&d=e", "https://ya.ru/search?a=b%20c&d=e")]
        [TestCase("https://ya.ru/", "search?a=b%20c&d=e", "https://ya.ru/search?a=b%20c&d=e")]
        [TestCase("localhost", "search?a=b%20c&d=e", "http://localhost/search?a=b%20c&d=e")]
        [TestCase("localhost:9090", "search?a=b%20c&d=e", "http://localhost:9090/search?a=b%20c&d=e")]
        [TestCase("http://localhost:9090", "search?a=b%20c&d=e", "http://localhost:9090/search?a=b%20c&d=e")]
        [TestCase("http://localhost:9090/", "search?a=b%20c&d=e", "http://localhost:9090/search?a=b%20c&d=e")]
        [TestCase("localhost:9090/", "search?a=b%20c&d=e", "http://localhost:9090/search?a=b%20c&d=e")]
        public void GetUriFor_noQueryParams(string host, string queryParams, string expected)
        {
            var uri = HttpClientRequest
                .CreateGet(queryParams)
                .GetUriFor(host);
            Assert.AreEqual(expected, uri.AbsoluteUri);
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
    }
}
