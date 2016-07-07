using Bjd.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Traces
{
    /// <summary>
    /// Write Console Feature
    /// </summary>
    internal class ConsoleTraceListner : System.Diagnostics.TraceListener
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
            //Console.Write(message);
            var ind = this.NeedIndent;

            var t = new Task(
                () =>
                {
                    if (ind)
                        Console.Write(new string(' ', this.IndentLevel * this.IndentSize));
                    Console.Write(message);
                }, TaskCreationOptions.PreferFairness);
            t.Start(this.sts);

        }
        protected override void WriteIndent()
        {
            //Console.Write(new string(' ', this.IndentLevel * this.IndentSize));
            var t = new Task(
                () =>
                    Console.Write(new string(' ', this.IndentLevel * this.IndentSize)), TaskCreationOptions.PreferFairness);
            t.Start(this.sts);

        }

        public override void WriteLine(string message)
        {

            //Console.WriteLine(message);
            var ind = this.NeedIndent;

            var t = new Task(
                () =>
                {
                    if (ind)
                        Console.Write(new string(' ', this.IndentLevel * this.IndentSize));
                    Console.WriteLine(message);
                }, TaskCreationOptions.PreferFairness);
            t.Start(this.sts);

        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            var ind = this.NeedIndent;
            Action tAct = () =>
            {
                //var sb = new System.Text.StringBuilder();

                if (this.TraceOutputOptions.HasFlag(TraceOptions.DateTime))
                {
                    var date = eventCache.DateTime.ToLocalTime().ToString("HH\\:mm\\:ss\\.fff");
                    //sb.Append($"[{date}]");
                    Console.Write($"[{date}]");
                }
                if (this.TraceOutputOptions.HasFlag(TraceOptions.Timestamp))
                {
                    //var time = TimeSpan.FromTicks(eventCache.Timestamp).ToString("hh\\:mm\\:ss\\.fffff");
                    ////sb.Append($"[{time}]");
                    //Console.Write($"[{time}]");
                    Console.Write($"[{eventCache.Timestamp}]");
                }
                if (this.TraceOutputOptions.HasFlag(TraceOptions.ProcessId))
                {
                    //sb.Append($"[PID:{eventCache.ProcessId}]");
                    Console.Write($"[PID:{eventCache.ProcessId}]");
                }
                if (this.TraceOutputOptions.HasFlag(TraceOptions.ThreadId))
                {
                    //sb.Append($"[{eventCache.ThreadId.PadLeft(3)}]");
                    Console.Write($"[{eventCache.ThreadId.PadLeft(3)}]");
                }

                //sb.Append($"[{eventType.ToString().Remove(4)}][{id}] ");
                //sb.Append($"[{id}] ");
                Console.Write($"[{id}] ");
                if (NeedIndent)
                {
                    //sb.Append(new string(' ', this.IndentLevel * this.IndentSize));
                    Console.Write(new string(' ', this.IndentLevel * this.IndentSize));
                }
                //sb.Append(message);
                Console.WriteLine(message);
                //Console.WriteLine(sb.ToString());
                //base.TraceEvent(eventCache, source, eventType, id, message);
            };
            var t = new Task(tAct, TaskCreationOptions.PreferFairness);
            t.Start(this.sts);

        }

    }

}
