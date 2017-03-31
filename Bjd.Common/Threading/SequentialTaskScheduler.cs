using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Bjd.Threading
{
    public class SequentialTaskScheduler : System.Threading.Tasks.TaskScheduler
    {
        Task[] queue = new Task[262144];
        int count = 0;
        int _cursorEnqueue = -1;
        int _cursorDequeue = -1;

        public SequentialTaskScheduler() : base()
        {
        }

        private void WaitCallback(object state)
        {
            while (true)
            {
                var idx = Increment(ref _cursorDequeue);
                try
                {
                    var t = queue[idx];
                    if(t != null)
                    {
                        queue[idx] = null;
                        this.TryExecuteTask(t);
                    }
                }
                catch { }
                if (Interlocked.Decrement(ref count) == 0) return;
            }

        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            var idx = _cursorDequeue;
            var length = queue.Length;
            for (var i =idx; i < (idx + count); i ++)
            {
                var v = queue[i % length];
                if (v == null) continue;
                yield return v;
            }
        }

        protected override void QueueTask(Task task)
        {
            var idx = Increment(ref _cursorEnqueue);
            queue[idx] = task;
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

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private int Increment(ref int _cursor)
        {
            var idx = Interlocked.Increment(ref _cursor);
            if (idx >= queue.Length)
            {
                idx = idx % queue.Length;
                Interlocked.CompareExchange(ref _cursor, idx, queue.Length + idx);
            }
            return idx;
        }


    }
}
