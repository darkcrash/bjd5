using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Threading
{
    public class SequentialTaskScheduler : System.Threading.Tasks.TaskScheduler
    {
        ConcurrentQueue<Task> _q = new ConcurrentQueue<Task>();
        int count = 0;

        public SequentialTaskScheduler() : base()
        {
        }

        private void WaitCallback(object state)
        {
            Task t = null;
            while (true)
            {
                try
                {
                    while (_q.TryDequeue(out t))
                    {
                        try
                        {
                            this.TryExecuteTask(t);
                        }
                        catch { }
                        if (Interlocked.Decrement(ref count) == 0) return;
                    }
                }
                catch { }
            }

        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _q.ToArray();
        }

        protected override void QueueTask(Task task)
        {
            _q.Enqueue(task);
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

    }
}
