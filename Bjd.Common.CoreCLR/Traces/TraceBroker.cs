using Bjd.Logs;
using Bjd.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bjd.Traces
{
    public class TraceBroker
    {
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
            var fMessage = Format(message);
            foreach (var writer in _srvs)
            {
                writer.WriteLine(fMessage);
            }
        }

        [Conditional("TRACE")]
        public void TraceWarning(string message)
        {
            var fMessage = Format(message);
            foreach (var writer in _srvs)
            {
                writer.WriteLine(fMessage);
            }
        }

        [Conditional("TRACE")]
        public void TraceError(string message)
        {
            var fMessage = Format(message);
            foreach (var writer in _srvs)
            {
                writer.WriteLine(fMessage);
            }
        }

        [Conditional("TRACE")]
        public void Fail(string message)
        {
            var fMessage = Format(message);
            foreach (var writer in _srvs)
            {
                writer.WriteLine(fMessage);
            }
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

    }
}