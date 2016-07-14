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
        //object Lock = new object();
        bool isRunning = false;
        EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        Thread worker;

        public SequentialTaskScheduler() : base()
        {
            worker = new Thread(WaitCallback);
            worker.IsBackground = true;
            worker.Name = this.GetType().Name;
            worker.Start();
        }

        private void WaitCallback(object state)
        {
            Task t = null;
            while (true)
            {
                if (_q.IsEmpty)
                {
                    isRunning = false;
                    waitHandle.Reset();
                    waitHandle.WaitOne(1000);
                    isRunning = true;
                    continue;
                }
                while (_q.TryDequeue(out t))
                {
                    this.TryExecuteTask(t);
                }
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _q.ToArray();
        }

        protected override void QueueTask(Task task)
        {
            _q.Enqueue(task);
            if (isRunning) return;
            waitHandle.Set();
            //lock (Lock)
            //{
            //    if (isRunning)
            //        return;
            //    isRunning = true;
            //}
            //System.Threading.ThreadPool.QueueUserWorkItem(this.WaitCallback);
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
