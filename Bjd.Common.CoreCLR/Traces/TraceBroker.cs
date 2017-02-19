using Bjd.Logs;
using Bjd.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bjd.Traces
{
    public class TraceBroker
    {
        private Threading.SequentialTaskScheduler sts = new Threading.SequentialTaskScheduler();
        private List<ILogService> _srvs;
        private string _pid;

        public TraceBroker(Kernel context)
        {
            _srvs = context.LogServices;
            _pid = Process.GetCurrentProcess().Id.ToString();
        }

        [Conditional("TRACE")]
        public void TraceInformation(string message)
        {
            FormatWriteLine(message);
        }

        [Conditional("TRACE")]
        public void TraceWarning(string message)
        {
            FormatWriteLine(message);
        }

        [Conditional("TRACE")]
        public void TraceError(string message)
        {
            FormatWriteLine(message);
        }

        [Conditional("TRACE")]
        public void Fail(string message)
        {
            FormatWriteLine(message);
        }

        private int indentCount = 0;
        private string indent = string.Empty;
        private string indentText = "  ";

        [Conditional("TRACE")]
        public void Indent()
        {
            indentCount++;
            indent = string.Empty;
            for (var i = 0; i < indentCount; i++)
            {
                indent += indentText;
            }
        }

        [Conditional("TRACE")]
        public void Unindent()
        {
            indentCount--;
            indent = string.Empty;
            for (var i = 0; i < indentCount; i++)
            {
                indent += indentText;
            }
        }

        private string Format(string message)
        {
            var date = DateTime.Now.ToString("HH\\:mm\\:ss\\.fff");
            var tid = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(3);
            return $"[{date}][{_pid}][{tid}] {indent}{message}";
        }

        private void FormatWriteLine(string message)
        {
            var date = DateTime.Now;
            var tid = System.Threading.Thread.CurrentThread.ManagedThreadId;
            var t = new System.Threading.Tasks.Task(() => WriteLineAll(date, tid, indent, message));
            t.Start(sts);
        }

        private void WriteLineAll(DateTime date, int tid, string ind, string message)
        {
            var dateText = date.ToString("HH\\:mm\\:ss\\.fff");
            var tidtext = tid.ToString().PadLeft(3);
            var msg = $"[{dateText}][{_pid}][{tidtext}] {ind}{message}";
            foreach (var writer in _srvs)
            {
                writer.WriteLine(msg);
            }
        }
    }
}