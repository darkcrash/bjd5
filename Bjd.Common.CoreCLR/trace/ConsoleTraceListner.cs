using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.trace
{
    public class ConsoleTraceListner : System.Diagnostics.TraceListener
    {
        SequentialTaskScheduler sts = new SequentialTaskScheduler();
        public ConsoleTraceListner()
        {

            this.TraceOutputOptions = System.Diagnostics.TraceOptions.DateTime | System.Diagnostics.TraceOptions.ThreadId;
            this.IndentSize = 1;
            try
            {
                if (Console.WindowWidth < 200)
                    Console.WindowWidth = 200;
            }
            catch (Exception)
            {
                Console.WriteLine("Not allowed change Console.WindowWidth");
            }

            try
            {
                //Console.WriteLine($"ConsoleTraceListner CodePage={Console.Out.Encoding.CodePage}");
                Define.ChangeOperationSystem += Define_ChangeOperationSystem;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error .ctor ConsoleTraceListner");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }

        private void Define_ChangeOperationSystem(object sender, EventArgs e)
        {
            // fix Windows ja-jp to codepage 932
            var lang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            var lang2 = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (Define.IsWindows && lang == "ja")
            {
                var enc = System.Text.CodePagesEncodingProvider.Instance;
                var sjis = enc.GetEncoding(932);
                var writer = new System.IO.StreamWriter(Console.OpenStandardOutput(), sjis);
                writer.AutoFlush = true;
                Console.SetOut(writer);
            }
        }

        public override void Write(string message)
        {
            //var t = new Task(
            //    () =>
            //    {
            //        if (this.NeedIndent)
            //            this.WriteIndent();
            //        Console.Write(message);
            //    }, TaskCreationOptions.PreferFairness);
            //t.Start();
            if (this.NeedIndent)
                this.WriteIndent();
            Console.Write(message);


        }
        protected override void WriteIndent()
        {
            //var t = new Task(
            //    () =>
            //    {
            //        Console.Write(new string(' ', this.IndentLevel * this.IndentSize));
            //    },  TaskCreationOptions.PreferFairness);
            //t.Start();
            Console.Write(new string(' ', this.IndentLevel * this.IndentSize));

        }

        public override void WriteLine(string message)
        {
            //var t = new Task(
            //    () =>
            //    {
            //        if (this.NeedIndent)
            //            this.WriteIndent();
            //        Console.WriteLine(message);
            //    }, TaskCreationOptions.PreferFairness);
            //t.Start();
            if (this.NeedIndent)
                this.WriteIndent();
            Console.WriteLine(message);
        }


        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            Action tAct = () =>
            {
                var sb = new System.Text.StringBuilder();

                if (this.TraceOutputOptions.HasFlag(TraceOptions.DateTime))
                {
                    var date = eventCache.DateTime.ToLocalTime().ToString("HH\\:mm\\:ss\\.fff");
                    sb.Append($"[{date}]");
                }
                if (this.TraceOutputOptions.HasFlag(TraceOptions.Timestamp))
                {
                    var time = TimeSpan.FromTicks(eventCache.Timestamp).ToString("hh\\:mm\\:ss\\.fffff");
                    sb.Append($"[{time}]");
                }
                if (this.TraceOutputOptions.HasFlag(TraceOptions.ProcessId))
                {
                    sb.Append($"[PID:{eventCache.ProcessId}]");
                }
                if (this.TraceOutputOptions.HasFlag(TraceOptions.ThreadId))
                {
                    sb.Append($"[{eventCache.ThreadId.PadLeft(3)}]");
                }

                var ind = this.NeedIndent;
                //sb.Append($"[{eventType.ToString().Remove(4)}][{id}] ");
                sb.Append($"[{id}] ");
                sb.Append(new string(' ', this.IndentLevel * this.IndentSize));
                sb.Append(message);
                Console.WriteLine(sb.ToString());

                //base.TraceEvent(eventCache, source, eventType, id, message);
            };
            var t = new Task(tAct, TaskCreationOptions.PreferFairness);
            t.Start(this.sts);

        }

    }

    class SequentialTaskScheduler : System.Threading.Tasks.TaskScheduler
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
            return _q.ToArray();
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
