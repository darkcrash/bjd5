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
        private EventWaitHandle handle = new EventWaitHandle(true, EventResetMode.ManualReset);
        private WaitHandle[] handles = new WaitHandle[2];
        private SimpleResetPool _pool;

        public SimpleResetEvent(SimpleResetPool pool)
        {
            _pool = pool;
            handles[0] = handle;
            Reset();
        }

        public void Set()
        {
            if (Interlocked.Exchange(ref lockState, UNLOCKED) == LOCKED)
            {
                handle.Set();
            }
        }

        public void Reset()
        {
            if (Interlocked.Exchange(ref lockState, LOCKED) == UNLOCKED)
            {
                handle.Reset();
            }
        }

        public bool IsLocked()
        {
            return (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED);
        }

        public void Wait()
        {
            if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
            {
                handle.WaitOne();
            }

            //while (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
            //{
            //    Thread.Sleep(1);
            //}


        }

        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
            {
                handles[1] = cancellationToken.WaitHandle;
                var result = WaitHandle.WaitAny(handles, millisecondsTimeout);
                handles[1] = null;
                if (result == 1) cancellationToken.ThrowIfCancellationRequested();
                return (System.Threading.WaitHandle.WaitTimeout != result);
            }
            return true;

            //var timeout = DateTime.Now.AddMilliseconds(millisecondsTimeout);
            //while (Interlocked.CompareExchange(ref lockState, UNLOCKED, UNLOCKED) == LOCKED)
            //{
            //    Thread.Sleep(1);
            //    if (timeout < DateTime.Now) return false;
            //    if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
            //}
            //return true;

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

                if (handle != null)
                {
                    handle.Set();
                    handle.Dispose();
                    handle = null;
                }

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
