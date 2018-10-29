using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TinyClient.Helpers;

namespace TinyClient.Client
{
    public class AsyncRequest : ITinyAsyncRequest
    {
        private readonly WebRequest _request;
        private readonly byte[] _dataOrNull;
        private TaskCompletionSource<HttpWebResponse> _completionSource;


        public static AsyncRequest Create(WebRequest request, byte[] dataOrNull) => new AsyncRequest(request,dataOrNull);

        public AsyncRequest(WebRequest request, byte[] dataOrNull)
        {
            _request = request;
            _dataOrNull = dataOrNull;
        }

        /// <exception cref="InvalidOperationException"></exception>
        public void Abort()
        {
            if(_completionSource==null)
                throw new InvalidOperationException("Request is not sent yet");
            _request.Abort();
        }
        /// <exception cref="InvalidOperationException"></exception>
        public Task<HttpWebResponse> Send()
        {
            if (_completionSource != null)
                throw new InvalidOperationException("Request is already sent");
            _completionSource = new TaskCompletionSource<HttpWebResponse>();
            if (_dataOrNull != null)
                SendAsync();
            else
                ReceiveAsync();

            return _completionSource.Task;
        }

        private void SendAsync()
        {
            var getRequestTask = Task.Factory
                .FromAsync(_request.BeginGetRequestStream, _request.EndGetRequestStream, null);

            getRequestTask.ContinueWith(c => _completionSource.TrySetException(
                    GetExceptionFrom(c, new InvalidOperationException("SendAsync error with no base exception"))),
                TaskContinuationOptions.OnlyOnFaulted);

            getRequestTask.ContinueWith(HandleRequestStream, 
                TaskContinuationOptions.NotOnFaulted);
        }


        private void ReceiveAsync()
        {
            var getResponseTask = Task.Factory
                .FromAsync(_request.BeginGetResponse, EndGetResponseWrapping, null);

            getResponseTask.ContinueWith(
                c => _completionSource.TrySetException(
                    GetExceptionFrom(c, 
                        defaultException:new InvalidOperationException("ReceiveAsync error with no base exception"))), 
                TaskContinuationOptions.OnlyOnFaulted);

            getResponseTask.ContinueWith(HandleResponse, 
                TaskContinuationOptions.NotOnFaulted);
        }

        private void HandleRequestStream(Task<Stream> task) {
            if (task.IsFaulted)
            {
                _completionSource.TrySetException(task.Exception.InnerException);
                return;
            }
            if (task.IsCanceled) {
                _completionSource.TrySetCanceled();
                return;
            }

            var stream = task.Result;
            var sendStreamTask = Task.Factory
                .FromAsync((callback, state) => stream.BeginWrite(_dataOrNull, 0, _dataOrNull.Length, callback, state), stream.EndWrite, null);

            sendStreamTask.ContinueWith(
                c => _completionSource.TrySetException(
                    GetExceptionFrom(c,
                        defaultException: new InvalidOperationException("Write Stream Async error with no base exception"))),
                TaskContinuationOptions.OnlyOnFaulted);

            sendStreamTask.ContinueWith((t) => ReceiveAsync(),
                TaskContinuationOptions.NotOnFaulted);
        }

        private void HandleResponse(Task<WebResponse> task)
        {
            if (task.IsFaulted)
            {
                _completionSource.TrySetException(task.Exception.InnerException);
                return;
            }
            if (task.IsCanceled) {
                _completionSource.TrySetCanceled();
                return;
            }

            
            _completionSource.TrySetResult((HttpWebResponse)task.Result);
        }


        private WebResponse EndGetResponseWrapping(IAsyncResult result)
        {
            return _request.EndGetResponse(result);
        }

        private Exception GetExceptionFrom(Task t, Exception defaultException)
        {
            var e = t.Exception;
            if (e == null)
                return defaultException;
            return e.InnerException 
                   ?? e.GetBaseException() 
                   ?? e;

        }
    }
}
