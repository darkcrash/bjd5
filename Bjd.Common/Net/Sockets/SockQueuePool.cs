using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Bjd.Net.Sockets
{
    public class SockQueuePool : IDisposable
    {
        public readonly static SockQueuePool Instance = new SockQueuePool();
        const int poolSize = 512;

        private ConcurrentBag<SockQueue> _queue = new ConcurrentBag<SockQueue>();
        //private CancellationTokenSource cancel = new CancellationTokenSource();
        private int _leaseCount = 0;
        private int _Count = 0;

        private SockQueuePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                _Count++;
                _queue.Add(new SockQueue());
            }
            //Cleanup();
        }

        ~SockQueuePool()
        {
            Dispose();
        }

        public void Dispose()
        {
            //cancel.Cancel();
            while (!_queue.IsEmpty)
            {
                SockQueue outQ;
                if (_queue.TryTake(out outQ)) outQ.Dispose();
            }
        }


        //private void Cleanup()
        //{
        //    if (cancel.IsCancellationRequested) return;
        //    try
        //    {
        //        if (_leaseCount > 0) return;

        //        var q = _queue.Count;

        //        int delete = q - poolSize;
        //        if (delete > 0)
        //        {
        //            SockQueue outQ;
        //            while (!_queue.TryTake(out outQ)) ;
        //            outQ.Dispose();
        //        }

        //    }
        //    finally
        //    {
        //        var t = Task.Delay(1000);
        //        t.ContinueWith(_ => Cleanup(), cancel.Token);
        //    }

        //}

        public SockQueue Get()
        {
            Interlocked.Increment(ref _leaseCount);
            SockQueue outQ;
            if (_queue.TryTake(out outQ))
            {
                return outQ;
            }
            Interlocked.Increment(ref _Count);
            return new SockQueue();
        }

        public void Pool(ref SockQueue q)
        {
            var l =Interlocked.Decrement(ref _leaseCount);
            if (poolSize < (_Count - l))
            {
                Interlocked.Decrement(ref _Count);
                q.Dispose();
                return;
            }
            q.Initialize();
            _queue.Add(q);
            q = null;

        }


    }
}

