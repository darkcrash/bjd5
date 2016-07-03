using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Threading
{
    public class SequentialTaskScheduler : System.Threading.Tasks.TaskScheduler
    {
        List<Task> _q = new List<Task>();
        object Lock = new object();
        bool isRunning = false;

        public SequentialTaskScheduler() : base()
        {
        }

        private void WaitCallback(object state)
        {
            Task t;
            while (true)
            {
                lock (Lock)
                {
                    if (_q.Count == 0)
                    {
                        isRunning = false;
                        return;
                    }
                    t = _q.First();
                    _q.Remove(t);
                }
                this.TryExecuteTask(t);
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            lock (Lock)
            {
                return _q.ToArray();
            }
        }

        protected override void QueueTask(Task task)
        {
            lock (Lock)
            {
                _q.Add(task);
                if (isRunning)
                    return;
                isRunning = true;
                System.Threading.ThreadPool.QueueUserWorkItem(this.WaitCallback);
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }
        protected override bool TryDequeue(Task task)
        {
            lock (Lock)
            {
                return _q.Remove(task);
            }
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
