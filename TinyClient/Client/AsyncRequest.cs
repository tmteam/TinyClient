using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TinyClient.Helpers;

namespace TinyClient.Client
{
    public class AsyncRequest
    {
        private readonly WebRequest _request;
        private readonly byte[] _dataOrNull;
        private TaskCompletionSource<HttpWebResponse> _completionSource;
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
            getRequestTask.ContinueWith(c => _completionSource.TrySetException(c.Exception.GetBaseException()),
                TaskContinuationOptions.OnlyOnFaulted);
            getRequestTask.ContinueWith(HandleRequestStream, 
                TaskContinuationOptions.NotOnFaulted);
        }


        private void ReceiveAsync()
        {
            var getResponseTask = Task.Factory
                .FromAsync(_request.BeginGetResponse, _request.EndGetResponse, null);
            getResponseTask.ContinueWith(c => _completionSource.TrySetException(c.Exception.GetBaseException())
                , TaskContinuationOptions.OnlyOnFaulted);
            getResponseTask.ContinueWith(HandleResponse, 
                TaskContinuationOptions.NotOnFaulted);
        }

        private void HandleRequestStream(Task<Stream> task) {
            if (task.IsCanceled) {
                _completionSource.TrySetCanceled();
                return;
            }

            if (task.IsFaulted) {
                _completionSource.TrySetException(task.Exception.InnerException);
                return;
            }
            var stream = task.Result;
            stream.Write(_dataOrNull);
            ReceiveAsync();
        }

        private void HandleResponse(Task<WebResponse> task)
        {
            if (task.IsCanceled) {
                _completionSource.TrySetCanceled();
                return;
            }

            if (task.IsFaulted) {
                _completionSource.TrySetException(task.Exception.InnerException);
                return;
            }
            _completionSource.TrySetResult((HttpWebResponse)task.Result);
        }

    }
}
