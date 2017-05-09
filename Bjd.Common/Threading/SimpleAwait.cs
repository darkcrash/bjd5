using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Threading
{
    internal class SimpleAwait
    {
        static LazyCancelTimer timer = LazyCancelTimer.Instance;
        static Action<object> Registar => _ => { ((SimpleAwait)_).Cancel(); };
        private static WaitCallback queueWorker = (o) => { ((Action)o)(); };

        private static void Queue(Action continuation)
        {
            if (continuation == null) return;
            System.Threading.ThreadPool.QueueUserWorkItem(queueWorker, continuation);
        }

        private Action _continuation;
        private Action _continuationUnsafe;
        private bool IsCompleted { get => Interlocked.CompareExchange(ref _IsCompleted, 0, 0) == 1; }
        private int _IsCompleted = 0;
        private bool Result = false;
        private SimpleAwait _child;

        public SimpleAwait()
        {

        }

        private SimpleAwait(int millisecondsTimeout)
        {
            var limit = timer.Get(millisecondsTimeout);
            limit.Register(Registar, this);
        }

        public SimpleAwaiter GetAwaiter()
        {
            return new SimpleAwaiter(this);
        }

        public SimpleAwait GetLimited(int millisecondsTimeout)
        {
            var c = new SimpleAwait(millisecondsTimeout);
            SetChild(c);
            if (IsCompleted)
            {
                if (Result)
                {
                    c.Complete();
                }
                else
                {
                    c.Cancel();
                }
            }
            return c;
        }

        private void SetChild(SimpleAwait c)
        {
            var cc = Interlocked.CompareExchange(ref _child, c, null);
            cc?.SetChild(c);
        }

        private void Cancel()
        {
            var c = Interlocked.Exchange(ref _child, null);
            if (Interlocked.Exchange(ref _IsCompleted, 1) == 0)
            {
                Result = false;
                CompAndQueue();
            }
            c?.Cancel();
        }

        public void Complete()
        {
            var c = Interlocked.Exchange(ref _child, null);
            if (Interlocked.Exchange(ref _IsCompleted, 1) == 0)
            {
                Result = true;
                CompAndQueue();
            }
            c?.Complete();
        }

        private void CompAndQueue()
        {
            var c = Interlocked.Exchange(ref _continuation, null);
            Queue(c);
            var cu = Interlocked.Exchange(ref _continuationUnsafe, null);
            Queue(cu);
        }

        private void InvokeContinuation()
        {
            var c = Interlocked.Exchange(ref _continuation, null);
            if (c != null) c();
        }

        private void InvokeContinuationUnsafe()
        {
            var c = Interlocked.Exchange(ref _continuationUnsafe, null);
            if (c != null) c();
        }

        public void Reset()
        {
            Interlocked.Exchange(ref _IsCompleted, 0);
        }


        internal struct SimpleAwaiter : ICriticalNotifyCompletion, INotifyCompletion
        {
            private SimpleAwait _parent;
            private bool _cancelable;

            public bool IsCompleted { get => _parent.IsCompleted; }


            public SimpleAwaiter(SimpleAwait parent)
            {
                _parent = parent;
                _cancelable = false;
            }

            public SimpleAwaiter(SimpleAwait parent, bool cancelable)
            {
                _parent = parent;
                _cancelable = cancelable;
            }


            public bool GetResult()
            {
                return _parent.Result;
            }

            public void OnCompleted(Action continuation)
            {
                if (_parent.IsCompleted)
                {
                    continuation();
                    return;
                }

                Interlocked.Exchange(ref _parent._continuation, continuation);

                if (_parent.IsCompleted)
                {
                    //Interlocked.Exchange(ref _parent._continuation, null)?.Invoke();
                    _parent.InvokeContinuation();
                }

            }

            [SecurityCritical]
            public void UnsafeOnCompleted(Action continuation)
            {
                if (_parent.IsCompleted)
                {
                    continuation();
                    return;
                }

                Interlocked.Exchange(ref _parent._continuationUnsafe, continuation);

                if (_parent.IsCompleted)
                {
                    //Interlocked.Exchange(ref _parent._continuationUnsafe, null)?.Invoke();
                    _parent.InvokeContinuationUnsafe();
                }

                //Task.CompletedTask.GetAwaiter();
            }

        }

    }
}
