using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bjd.Net.Sockets
{
    public class SockQueuePool : IDisposable
    {
        public readonly static SockQueuePool Instance = new SockQueuePool();

        private Queue<SockQueue> _queue = new Queue<SockQueue>();
        private object _lock = new object();
        private int _countGet = 0;
        private int _countPool = 0;
        private int _countPeek = 0;
        private CancellationTokenSource cancel = new CancellationTokenSource();

        private SockQueuePool()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            const int poolSize = 4;
            int get, pool, peek, q;
            lock (_lock)
            {
                get = _countGet;
                pool = _countPool;
                peek = _countPeek;
                q = _queue.Count;
                _countGet = 0;
                _countPool = 0;
            }

            try
            {
                int create = poolSize - q;
                if (create > 0)
                {
                    lock (_lock)
                    {
                        for (int i = 0; i < create; i++)
                            _queue.Enqueue(new SockQueue());
                    }
                    return;
                }

                // アクセス上昇中なら処理しない
                if (_countGet < _countPool) return;

                int delete = q - peek - poolSize;
                if (delete > 0)
                {
                    lock (_lock)
                    {
                        for (int i = 0; i < delete; i++)
                            _queue.Dequeue().Dispose();
                    }
                    return;
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
            lock (_lock)
            {
                _countGet++;
                var releaseCount = _countGet - _countPool;
                if (_countPeek < releaseCount) _countPeek = releaseCount;
                if (_queue.Count > 0) return _queue.Dequeue();
            }

            return new SockQueue();

        }

        public void Pool(ref SockQueue q)
        {
            q.Initialize();
            lock (_lock)
            {
                _countPool++;
                _queue.Enqueue(q);
            }
            q = null;

        }


        public void Dispose()
        {
            cancel.Cancel();
        }
    }
}

