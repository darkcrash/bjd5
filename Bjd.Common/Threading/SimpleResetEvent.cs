using Bjd.Memory;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
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
        private int lockState = 0;
        private int signalState = 0;
        private int lockWaiter = 0;
        private int callbackCount = 0;
        private EventWaitHandle handle = new EventWaitHandle(true, EventResetMode.ManualReset);
        private WaitHandle[] handles = new WaitHandle[2];
        private Task[] callbacks = new Task[16];
        private SimpleResetPool _pool;
        private Task waitTask;
        private static Action nullAction = () => { };
        private int usewaitTask = NOTUSE;

        public SimpleResetEvent(SimpleResetPool pool)
        {
            _pool = pool;
            handles[0] = handle;
            Reset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set()
        {
            if (Interlocked.Exchange(ref lockState, UNLOCKED) == LOCKED)
            {
                if (Interlocked.CompareExchange(ref usewaitTask, USE, USE) == USE && waitTask != null)
                {
                    waitTask.RunSynchronously();
                    waitTask = null;
                }
                if (Interlocked.CompareExchange(ref lockWaiter, 0, 0) != 0)
                {
                    Interlocked.Exchange(ref signalState, UNLOCKED);
                    handle.Set();
                }
                for (var i = 0; i < callbackCount; i++)
                {
                    Task a = Interlocked.Exchange(ref callbacks[i], null);
                    a.Start();
                }
                callbackCount = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            if (Interlocked.Exchange(ref lockState, LOCKED) == UNLOCKED)
            {
                if (waitTask == null) waitTask = new Task(nullAction, TaskCreationOptions.RunContinuationsAsynchronously | TaskCreationOptions.LongRunning);
                if (Interlocked.Exchange(ref signalState, LOCKED) == UNLOCKED)
                {
                    handle.Reset();
                }
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
            Interlocked.Exchange(ref usewaitTask, USE);
            if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
            {
                return waitTask;
            }
            return Task.CompletedTask;
        }

        public bool WaitCallback(Task callback)
        {
            var cb = Interlocked.Increment(ref callbackCount);

            if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
            {
                callbacks[cb] = callback;
                return false;
            }

            try
            {
                callback.RunSynchronously();
            }
            catch { }

            Interlocked.Decrement(ref callbackCount);
            return true;

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

                if (handle != null)
                {
                    handle.Dispose();
                    handle = null;
                }

                handles = null;
                callbacks = null;

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
            _pool.PoolInternal(this);
        }

        public void Initialize()
        {
            Interlocked.Exchange(ref usewaitTask, NOTUSE);
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
