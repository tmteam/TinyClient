using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TinyClient.Client;

namespace TinyClient.Tests
{
    [TestFixture]
    public class HttpSenderAsyncTest
    {
        [Test]
        public void AsyncRequestThrowsSyncroniously_HttpSenderSendRethrows()
        {
            string host = "ya.ru";
            var sender = new HttpSenderAsync(host, new IContentEncoder[0]);
            var requestStub = HttpClientRequest.CreateJsonPost("content");

            sender.AsyncRequestFactory = (r, d) =>
            {
                var mock = new Mock<ITinyAsyncRequest>();
                mock.Setup(s => s.Send()).Throws(new InvalidOperationException());
                return mock.Object;
            };
            Assert.Throws<InvalidOperationException>(()=>sender.Send(requestStub));
        }

        [Test]
        public void AsyncRequestThrowsWithTask_HttpSenderSendRethrows()
        {
            string host = "ya.ru";
            var sender = new HttpSenderAsync(host, new IContentEncoder[0]);
            var requestStub = HttpClientRequest.CreateJsonPost("content");

            sender.AsyncRequestFactory = (r, d) =>
            {
                var mock = new Mock<ITinyAsyncRequest>();
                mock.Setup(s => s.Send()).Returns(() =>
                {
                    var source = new TaskCompletionSource<HttpWebResponse>();
                    source.SetException(new InvalidOperationException());
                    return source.Task;
                });
                return mock.Object;
            };
            Assert.Throws<InvalidOperationException>(() => sender.Send(requestStub));
        }
    }
}
