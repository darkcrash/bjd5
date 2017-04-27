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
        const int LOCKED = 1;
        const int UNLOCKED = 0;
        private int lockState = 0;
        private int signalState = 0;
        private int lockWaiter = 0;
        private EventWaitHandle handle = new EventWaitHandle(true, EventResetMode.ManualReset);
        private WaitHandle[] handles = new WaitHandle[2];
        private SimpleResetPool _pool;

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
                if (Interlocked.CompareExchange(ref lockWaiter, 0, 0) != 0)
                {
                    Interlocked.Exchange(ref signalState, UNLOCKED);
                    handle.Set();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLocked()
        {
            return (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED);
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

                //var spin = new SpinWait();
                //while (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
                //{
                //    spin.SpinOnce();
                //}
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

                //var spin = new SpinWait();
                //while (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
                //{
                //    spin.SpinOnce();
                //}
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

                //var spin = new SpinWait();
                //var timeout = DateTime.Now.AddMilliseconds(millisecondsTimeout);
                //while (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
                //{
                //    spin.SpinOnce();
                //    if (timeout < DateTime.Now) return false;
                //    if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
                //}
                //return true;
            }
            finally
            {
                Interlocked.Decrement(ref lockWaiter);
            }

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
