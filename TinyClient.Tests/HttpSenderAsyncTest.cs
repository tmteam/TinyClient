using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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

        [Test]
        public void AsyncRequestThrowsAfterTimeout_UnderlyingAsyncExceptionHandlerCalled()
        {
            var sender = new HttpSenderAsync("ya.ru", new IContentEncoder[0]);
            
            var requestStub = HttpClientRequest.CreateJsonPost("content").SetTimeout(TimeSpan.Zero);
            var resetEvent = new ManualResetEventSlim(false);
            Exception catchedException = null;
            sender.UnderlyingAsyncExceptionHandler += (e) => catchedException = e;

            Func<HttpWebResponse> waitThanThrowFunc = () =>
            {
                resetEvent.Wait();
                throw new InvalidOperationException();
            };

            sender.AsyncRequestFactory = (r, d) =>
            {
                var mock = new Mock<ITinyAsyncRequest>();
                mock.Setup(s => s.Send()).Returns(() => Task.Run(waitThanThrowFunc));
                return mock.Object;
            };
            //Send with zero timeout 
            Assert.Throws<TinyTimeoutException>(()=> sender.Send(requestStub));
            //unblock async request
            resetEvent.Set();

            //SpinWait for async request exception!
            Stopwatch se = Stopwatch.StartNew();
            while (se.ElapsedMilliseconds < 1000)
            {
                if (catchedException != null)
                {
                    //... and this exception has to be catched!
                    Assert.IsInstanceOf<InvalidOperationException>(catchedException);
                    return;
                }
            }
            Assert.Fail("UnderlyingAsyncExceptionHandler was not called");

        }


        [Test]
        public void AsyncRequestThrowsAfterTimeout_AsyncRequestAbortCalled()
        {
            var sender = new HttpSenderAsync("ya.ru", new IContentEncoder[0]);

            var requestStub = HttpClientRequest.CreateJsonPost("content").SetTimeout(TimeSpan.Zero);
            var resetEvent = new ManualResetEventSlim(false);
            Exception catchedException = null;
            sender.UnderlyingAsyncExceptionHandler += (e) => catchedException = e;

            Func<HttpWebResponse> waitForEvent = () =>
            {
                resetEvent.Wait();
                return null;
            };

            var mock = new Mock<ITinyAsyncRequest>();
            mock.Setup(s => s.Send()).Returns(() => Task.Run(waitForEvent));
            mock.Setup(s => s.Abort());    

            sender.AsyncRequestFactory = (r, d) => mock.Object;
            //Send with zero timeout 
            Assert.Throws<TinyTimeoutException>(() => sender.Send(requestStub));
            
            mock.Verify(m=>m.Abort(), Times.Once);
            
            //unblock async request
            resetEvent.Set();

        }





        [Test]
        public void AsyncRequestThrowsTimeoutWithTask_HttpSenderSendRethrowsTinyTimout()
        {
            var sender = new HttpSenderAsync("ya.ru", new IContentEncoder[0]);
            var requestStub = HttpClientRequest.CreateJsonPost("content");

            sender.AsyncRequestFactory = (r, d) =>
            {
                var mock = new Mock<ITinyAsyncRequest>();
                mock.Setup(s => s.Send()).Returns(() =>
                {
                    var source = new TaskCompletionSource<HttpWebResponse>();
                    source.SetException(new TimeoutException());
                    return source.Task;
                });
                return mock.Object;
            };
            Assert.Throws<TinyTimeoutException>(() => sender.Send(requestStub));
        }
    }
}
