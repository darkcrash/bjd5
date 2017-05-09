using Bjd.Memory;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Threading
{
    public class SimpleResetEvent : IDisposable, IPoolBuffer
    {
        const int USE = 1;
        const int NOTUSE = 0;
        const int LOCKED = 1;
        const int UNLOCKED = 0;

        static LazyCancelTimer timer =  LazyCancelTimer.Instance;
        private static Action<Task> _ActionEmpty = _ => { };
        private static Func<Task, bool> _FuncCancelFalse = _ => (_.IsCanceled ? false : true);
        private static Action<object> CancelRegister = _ => ((CancellationTokenSource)_).Cancel();
        private static Action<Task, object> CancelDispose = (t, o) => ((CancellationTokenSource)o).Dispose();

        private int lockState = 0;
        private int signalState = 0;
        private int lockWaiter = 0;
        private EventWaitHandle handle = new EventWaitHandle(true, EventResetMode.ManualReset);
        private WaitHandle[] handles = new WaitHandle[2];
        private SimpleResetPool _pool;
        private Task waitTask;
        private SimpleAwait waitAwaiterAsync;
        private static Action nullAction = () => { };
        private int usewaitTask = NOTUSE;
        private int usewaitAwait = NOTUSE;



        public SimpleResetEvent(SimpleResetPool pool)
        {
            _pool = pool;
            handles[0] = handle;
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
                if (Interlocked.CompareExchange(ref lockWaiter, 0, 0) != 0)
                {
                    Interlocked.Exchange(ref signalState, UNLOCKED);
                    handle.Set();
                }
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
                if (Interlocked.Exchange(ref signalState, LOCKED) == UNLOCKED)
                {
                    handle.Reset();
                }
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
        public void Wait()
        {
            var waiterNo = Interlocked.Increment(ref lockWaiter);
            try
            {

                if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
                {
                    handle.WaitOne();
                }

            }
            finally
            {
                Interlocked.Decrement(ref lockWaiter);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Wait(int millisecondsTimeout)
        {
            var waiterNo = Interlocked.Increment(ref lockWaiter);
            try
            {

                if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
                {
                    return handle.WaitOne(millisecondsTimeout);
                }
                else
                {
                    return true;
                }

            }
            finally
            {
                Interlocked.Decrement(ref lockWaiter);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var waiterNo = Interlocked.Increment(ref lockWaiter);
            try
            {
                if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
                {
                    if (cancellationToken.IsCancellationRequested) return false;
                    handles[1] = cancellationToken.WaitHandle;
                    var result = WaitHandle.WaitAny(handles, millisecondsTimeout);
                    handles[1] = null;
                    if (result == 1) cancellationToken.ThrowIfCancellationRequested();
                    return (System.Threading.WaitHandle.WaitTimeout != result);
                }
                return true;

            }
            finally
            {
                Interlocked.Decrement(ref lockWaiter);
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
            //Interlocked.CompareExchange(ref usewaitTask, USE, NOTUSE);
            Interlocked.CompareExchange(ref usewaitAwait, USE, NOTUSE);
            if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
            {
                await a;
                //await t;
            }
            return true;
        }


        public async ValueTask<bool> WaitAsyncValueTask(int millisecondsTimeout)
        {
            //var t1 = waitTask;

            //Interlocked.CompareExchange(ref usewaitTask, USE, NOTUSE);
            //if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
            //{
            //    var token = timer.Get(millisecondsTimeout);
            //    var t2 = t1.ContinueWith(_ActionEmpty, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            //    var t3 = t2.ContinueWith(_FuncCancelFalse, TaskContinuationOptions.ExecuteSynchronously);
            //    return await t3;
            //}
            //return true;

            var a = waitAwaiterAsync;

            Interlocked.CompareExchange(ref usewaitAwait, USE, NOTUSE);
            if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
            {
                var limited = a.GetLimited(millisecondsTimeout);
                return await limited;
            }
            return true;

        }

        public async ValueTask<bool> WaitAsyncValueTask(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var t1 = waitTask;
            Interlocked.CompareExchange(ref usewaitTask, USE, NOTUSE);
            if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
            {
                var token = timer.Get(millisecondsTimeout);
                var cancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                token.Register(CancelRegister, cancel);
                var t2 = t1.ContinueWith(_ActionEmpty, cancel.Token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                var t2_2 = t2.ContinueWith(CancelDispose, cancel, TaskContinuationOptions.ExecuteSynchronously);
                var t3 = t2.ContinueWith(_FuncCancelFalse, TaskContinuationOptions.ExecuteSynchronously);
                return await t3;
            }
            return true;
        }

        public async ValueTask<bool> WaitAsyncValueTask(CancellationToken cancellationToken)
        {
            var t1 = waitTask;
            Interlocked.CompareExchange(ref usewaitTask, USE, NOTUSE);
            if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
            {
                var t2 = t1.ContinueWith(_ActionEmpty, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                var t3 = t2.ContinueWith(_FuncCancelFalse, TaskContinuationOptions.ExecuteSynchronously);
                return await t3;
            }
            return true;
        }

        //public async Task WaitAsync()
        //{
        //    while (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
        //    {
        //        await Task.Yield();
        //    }
        //}


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

                if (handle != null)
                {
                    handle.Dispose();
                    handle = null;
                }

                handles = null;

                _pool = null;

                disposedValue = true;
            }
        }

        ~SimpleResetEvent()
        {
            Dispose(true);
        }

        public void Dispose()
        {
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
