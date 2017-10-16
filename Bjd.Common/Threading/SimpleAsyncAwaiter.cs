using Bjd.Memory;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Threading
{
    public class SimpleAsyncAwaiter : IDisposable, IPoolBuffer
    {
        const int USE = 1;
        const int NOTUSE = 0;
        const int LOCKED = 1;
        const int UNLOCKED = 0;

        static LazyCancelTimer timer = LazyCancelTimer.Instance;
        private static Action<Task> _ActionEmpty = _ => { };
        private static Func<Task, bool> _FuncCancelFalse = _ => (_.IsCanceled ? false : true);
        private static Action<object> CancelRegister = _ =>
        {
            try { ((CancellationTokenSource)_).Cancel(); } catch { }
        };
        private static Action<Task, object> CancelDispose = (t, o) =>
        {
            try { ((CancellationTokenSource)o).Dispose(); }
            catch { }
        };

        private int lockState = 0;
        private SimpleAsyncAwaiterPool _pool;
        private Task waitTask;
        private SimpleAwait waitAwaiterAsync;
        private static Action nullAction = () => { };
        private int usewaitTask = NOTUSE;
        private int usewaitAwait = NOTUSE;



        public SimpleAsyncAwaiter(SimpleAsyncAwaiterPool pool)
        {
            _pool = pool;
            waitTask = new Task(nullAction, TaskCreationOptions.RunContinuationsAsynchronously | TaskCreationOptions.LongRunning);
            waitTask.ConfigureAwait(false);
            waitAwaiterAsync = new SimpleAwait();
            Reset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set()
        {
            if (Interlocked.Exchange(ref lockState, UNLOCKED) == LOCKED)
            {
                if (Interlocked.CompareExchange(ref usewaitTask, NOTUSE, USE) == USE)
                {
                    var t = new Task(nullAction, TaskCreationOptions.RunContinuationsAsynchronously | TaskCreationOptions.LongRunning);
                    t.ConfigureAwait(false);
                    var nowT = Interlocked.Exchange(ref waitTask, t);
                    nowT.RunSynchronously(TaskScheduler.Default);
                }
                if (Interlocked.CompareExchange(ref usewaitAwait, NOTUSE, USE) == USE)
                {
                    //var a = new SimpleAwait();
                    //var nowA = Interlocked.Exchange(ref waitAwaiter, a);
                    //nowA.Complete();
                    waitAwaiterAsync.Complete();
                }

            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            if (Interlocked.Exchange(ref lockState, LOCKED) == UNLOCKED)
            {
                waitAwaiterAsync.Reset();
            }
        }

        public bool IsLocked
        {
            get
            {
                return (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WaitAsync()
        {
            var t = waitTask;
            Interlocked.CompareExchange(ref usewaitTask, USE, NOTUSE);
            if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
            {
                return t;
            }
            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<bool> WaitAsyncValueTask()
        {
            //var t = waitTask;
            var a = waitAwaiterAsync;
            Interlocked.CompareExchange(ref usewaitAwait, USE, NOTUSE);
            if (Interlocked.CompareExchange(ref lockState, LOCKED, LOCKED) == UNLOCKED)
            {
                return true;
            }
            return await a;
        }

        public async ValueTask<bool> WaitAsyncValueTask(int millisecondsTimeout)
        {
            var a = waitAwaiterAsync;

            Interlocked.CompareExchange(ref usewaitAwait, USE, NOTUSE);
            if (Interlocked.CompareExchange(ref lockState, LOCKED, LOCKED) == UNLOCKED)
            {
                return true;
            }

            return await a.GetLimited(millisecondsTimeout);

        }

        public async ValueTask<bool> WaitAsyncValueTask(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var t1 = waitTask;
            Interlocked.CompareExchange(ref usewaitTask, USE, NOTUSE);
            if (Interlocked.CompareExchange(ref lockState, LOCKED, LOCKED) == UNLOCKED)
            {
                return true;
            }

            var timerToken = timer.Get(millisecondsTimeout);
            var cancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timerToken.Register(CancelRegister, cancel);
            var t2 = t1.ContinueWith(_ActionEmpty, cancel.Token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            var t3_1 = t2.ContinueWith(CancelDispose, cancel, TaskContinuationOptions.ExecuteSynchronously);
            var t3_2 = t2.ContinueWith(_FuncCancelFalse, TaskContinuationOptions.ExecuteSynchronously);
            return await t3_2;
        }

        public async ValueTask<bool> WaitAsyncValueTask(CancellationToken cancellationToken)
        {
            var t1 = waitTask;
            Interlocked.CompareExchange(ref usewaitTask, USE, NOTUSE);
            if (Interlocked.CompareExchange(ref lockState, LOCKED, LOCKED) == UNLOCKED)
            {
                return true;
            }
            var t2 = t1.ContinueWith(_ActionEmpty, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            var t3 = t2.ContinueWith(_FuncCancelFalse, TaskContinuationOptions.ExecuteSynchronously);
            return await t3;
        }

        #region IDisposable Support

        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                Set();


                _pool = null;

                disposedValue = true;
            }
        }

        ~SimpleAsyncAwaiter()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            if (disposedValue) return;
            Set();
            _pool.PoolInternal(this);
        }

        public void Initialize()
        {
            //Interlocked.Exchange(ref usewaitTask, NOTUSE);
            Reset();
        }

        public void DisposeInternal()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }


}
