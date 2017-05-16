using Bjd.Memory;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Bjd.Threading
{
    public class SimpleWoekerQueue
    {
        const int LOCKED = 1;
        const int UNLOCKED = 0;
        const int PoolMaxSize = 65535;
        private readonly static SimpleWoekerQueue Pool;

        static SimpleWoekerQueue()
        {
            Pool = new SimpleWoekerQueue();
        }

        public static bool QueueUserWorkItem(WaitCallback callBack, object state)
        {
            return Pool._QueueUserWorkItem(callBack, state);
        }

        private int ChangeZeroLock = UNLOCKED;
        private int workerCount = 0;
        private int queueCount = 0;
        private int enqueueIndex = -1;
        private int dequeueIndex = -1;
        private QueueStruct[] queue;
        private WaitCallback worker;
        private ParameterizedThreadStart workerThread;
        private int pCount = System.Environment.ProcessorCount;

        private SimpleWoekerQueue()
        {
            queue = new QueueStruct[PoolMaxSize];
            worker = WorkerAction;
            workerThread = WorkerAction;
        }

        ~SimpleWoekerQueue()
        {
        }

        private bool _QueueUserWorkItem(WaitCallback callBack, object state)
        {
            var idx = Interlocked.Increment(ref enqueueIndex);
            var idxMod = idx % PoolMaxSize;
            ExchangeZero(ref enqueueIndex, idx, idxMod);
            ref var q = ref queue[idxMod];
            var prev = Interlocked.Exchange(ref q.callBack, callBack);
            var prevState = Interlocked.Exchange(ref q.state, state);
            if (prev != null)
            {
                _QueueUserWorkItem(prev, prevState);
            }

            var qCnt = Interlocked.Increment(ref queueCount);

            var wk = Interlocked.CompareExchange(ref workerCount, 0, 0);
            if (wk < qCnt && wk < (pCount * 4))
            {
                NewWorker();
            }


            return true;
        }

        private void NewWorker()
        {
            var workerNo = Interlocked.Increment(ref workerCount);
            ThreadPool.QueueUserWorkItem(worker, workerNo);
            //var t =  new Thread(workerThread);
            //t.Start(null);
        }

        private void WorkerAction(object state)
        {
            try
            {
                //var workerNo = Interlocked.Increment(ref workerCount);
                var workerNo = (int)state;
                var wait = new SpinWait();

                while (true)
                {
                    //if (Interlocked.CompareExchange(ref DequeueLock, LOCKED, UNLOCKED) == LOCKED) continue;
                    var qCnt = Interlocked.CompareExchange(ref queueCount, 0, 0);
                    if (qCnt == 0)
                    {
                        //Interlocked.Exchange(ref DequeueLock, UNLOCKED);
                        return;
                    }
                    Interlocked.Decrement(ref queueCount);
                    //Interlocked.Exchange(ref DequeueLock, UNLOCKED);

                    var idx = Interlocked.Increment(ref dequeueIndex);
                    var idxMod = idx % PoolMaxSize;
                    ExchangeZero(ref dequeueIndex, idx, idxMod);
                    ref var q = ref queue[idxMod];
                    try
                    {
                        var work = Interlocked.Exchange(ref q.callBack, null);
                        if (work == null)
                        {
                            Interlocked.Increment(ref queueCount);
                            continue;
                        }
                        var workState = Interlocked.Exchange(ref q.state, null);
                        work(workState);
                    }
                    catch { }
                }
            }
            finally
            {
                Interlocked.Decrement(ref workerCount);
            }

        }


        private void ExchangeZero(ref int _cursor, int idx, int idxMod)
        {
            if (idx == idxMod) return;
            if (PoolMaxSize < _cursor)
            {
                if (Interlocked.CompareExchange(ref ChangeZeroLock, LOCKED, UNLOCKED) == UNLOCKED)
                {
                    if (PoolMaxSize < _cursor)
                    {
                        Interlocked.Add(ref _cursor, -PoolMaxSize);
                    }
                    Interlocked.Exchange(ref ChangeZeroLock, UNLOCKED);
                }
            }
        }

        private struct QueueStruct
        {
            public WaitCallback callBack;
            public object state;
        }
    }
}
