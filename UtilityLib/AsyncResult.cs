using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    internal abstract class AsyncResult : IAsyncResult, IDisposable
    {
        #region Fields

        private readonly object _asyncState;
        private bool _isCompleted;
        private readonly AsyncCallback _userCallback;
        private ManualResetEvent _asyncWaitEvent;
        private Exception _exception;

        #endregion

        #region IAsyncResult Members

        public object AsyncState
        {
            get { return _asyncState; }
        }

        public AsyncCallback Callback
        {
            get { return _userCallback; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (_asyncWaitEvent != null)
                    return _asyncWaitEvent;

                _asyncWaitEvent = new ManualResetEvent(false);

                if (IsCompleted)
                    _asyncWaitEvent.Set();

                return _asyncWaitEvent;
            }
        }

        public bool CompletedSynchronously { get; protected set; }

        [DebuggerNonUserCode]
        public bool IsCompleted
        {
            get { return _isCompleted; }
        }

        #endregion

        protected AsyncResult(AsyncCallback callback, object state)
        {
            _userCallback = callback;
            _asyncState = state;
        }

        public void Complete(Exception ex)
        {
            _exception = ex;
            NotifyCompletion();
        }

        protected void WaitForCompletion()
        {
            if (!IsCompleted)
                AsyncWaitHandle.WaitOne();

            if (_exception != null)
                throw _exception;
        }

        protected void NotifyCompletion()
        {
            _isCompleted = true;
            if (_asyncWaitEvent != null)
                _asyncWaitEvent.Set();

            if (_userCallback != null)
            {
                //var httpAsyncResult = this as ServiceClientImpl.HttpAsyncResult;
                //Debug.Assert(httpAsyncResult != null);
                //_userCallback(httpAsyncResult.AsyncState as RetryableAsyncResult);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _asyncWaitEvent != null)
            {
                _asyncWaitEvent.Close();
                _asyncWaitEvent = null;
            }
        }

        #endregion
    }

    internal class AsyncResult<T> : AsyncResult
    {
        private T _result;

        public AsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        { }

        public T GetResult()
        {
            base.WaitForCompletion();
            return _result;
        }

        public void Complete(T result)
        {
            // Complete should not throw if disposed.
            _result = result;
            base.NotifyCompletion();
        }
    }
}
