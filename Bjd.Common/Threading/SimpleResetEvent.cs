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
        private EventWaitHandle handle = new EventWaitHandle(true, EventResetMode.ManualReset);
        private WaitHandle[] handles = new WaitHandle[2];
        private SimpleResetPool _pool;
        private Task waitTask;
        private static Action nullAction = () => { };
        private int usewaitTask = NOTUSE;

        public SimpleResetEvent(SimpleResetPool pool)
        {
            _pool = pool;
            handles[0] = handle;
            Reset();
            waitTask = new Task(nullAction, TaskCreationOptions.RunContinuationsAsynchronously | TaskCreationOptions.LongRunning);
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
                    var t = new Task(nullAction, TaskCreationOptions.AttachedToParent);
                    var nowT = Interlocked.Exchange(ref waitTask, t);
                    nowT.RunSynchronously();
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
            //Task t = waitTask;
            //if (Interlocked.CompareExchange(ref usewaitTask, USE, NOTUSE) == NOTUSE)
            //{
            //    t = new Task(nullAction, TaskCreationOptions.RunContinuationsAsynchronously | TaskCreationOptions.LongRunning);
            //    waitTask = t;
            //}
            var t = waitTask;
            Interlocked.CompareExchange(ref usewaitTask, USE, NOTUSE);
            if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
            {
                return t;
            }
            return Task.CompletedTask;
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
