using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Bjd.Threading
{
    public class SequentialTaskScheduler : System.Threading.Tasks.TaskScheduler
    {
        const int MaxLength = 262144;
        Task[] queue = new Task[MaxLength];
        int count = 0;
        int _cursorEnqueue = -1;
        int _cursorDequeue = -1;

        public SequentialTaskScheduler() : base()
        {
        }

        private void WaitCallback(object state)
        {
            bool isNotFound = false;
            while (true)
            {
                var idx = Increment(ref _cursorDequeue);
                try
                {
                    //var t = queue[idx];
                    var t= Interlocked.Exchange<Task>(ref queue[idx], null);

                    if (t == null)
                    {
                        //System.Threading.SpinWait.SpinUntil(() => queue[idx] != null, 100);
                        //t = queue[idx];
                        //var w = new SpinWait();
                        for(int i = 0; i < 100; i++) 
                        {
                            //w.SpinOnce();
                            System.Threading.Thread.Sleep(1);
                            t = Interlocked.Exchange<Task>(ref queue[idx], null);
                            if (t != null) break;
                        }
                    }
                    if (t != null)
                    {
                        //queue[idx] = null;
                        this.TryExecuteTask(t);
                    }
                    else
                    {
                        if (!isNotFound)
                        {
                            isNotFound = true;
                            Debug.WriteLine($"SequentialTaskScheduler: NotFound {idx}");
                        }
                        continue;
                    }
                    isNotFound = false;
                }
                catch { }
                if (Interlocked.Decrement(ref count) == 0) return;
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            var idx = _cursorDequeue;
            var cnt = count;
            for (var i = idx; i < (idx + cnt); i++)
            {
                var v = queue[i % MaxLength];
                if (v == null) continue;
                yield return v;
            }
        }

        protected override void QueueTask(Task task)
        {
            var idx = Increment(ref _cursorEnqueue);
            Interlocked.Exchange<Task>(ref queue[idx], task);
            if (Interlocked.Increment(ref count) == 1)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(this.WaitCallback);
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }
        protected override bool TryDequeue(Task task)
        {
            return false;
        }
        public override int MaximumConcurrencyLevel
        {
            get
            {
                return 1;
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Increment(ref int _cursor)
        {
            var idx = -1;
            while (idx == -1)
            {
                idx = Interlocked.Increment(ref _cursor);
            }
            if (idx >= MaxLength)
            {
                idx = idx % MaxLength;
                Interlocked.CompareExchange(ref _cursor, idx, MaxLength + idx);
            }
            return idx;
        }


    }
}
