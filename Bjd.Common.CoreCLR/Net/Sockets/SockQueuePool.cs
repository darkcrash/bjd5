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
        const int poolSize = 20;

        private ConcurrentQueue<SockQueue> _queue = new ConcurrentQueue<SockQueue>();
        private CancellationTokenSource cancel = new CancellationTokenSource();

        private SockQueuePool()
        {
            Task.Factory.StartNew(() => Cleanup());
        }

        private void Cleanup()
        {
            try
            {
                var q = _queue.Count;

                int create = poolSize - q;
                if (create > 0)
                {
                    for (int i = 0; i < create; i++)
                    {
                        _queue.Enqueue(new SockQueue());
                    }
                    return;
                }

                int delete = q - poolSize;
                if (delete > 0)
                {
                    SockQueue outQ;
                    while (!_queue.TryDequeue(out outQ)) ;
                    outQ.Dispose();
                }

            }
            finally
            {
                var t = Task.Delay(10000);
                t.ContinueWith(_ => Cleanup(), cancel.Token);
            }

        }

        public SockQueue Get()
        {
            if (_queue.Count > 0)
            {
                SockQueue outQ;
                while (!_queue.TryDequeue(out outQ)) ;
                return outQ;
            }
            return new SockQueue();

        }

        public void Pool(ref SockQueue q)
        {
            q.Initialize();
            _queue.Enqueue(q);
            q = null;

        }


        public void Dispose()
        {
            cancel.Cancel();
        }
    }
}

