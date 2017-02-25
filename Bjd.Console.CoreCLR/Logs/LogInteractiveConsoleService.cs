using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Bjd.Utils;
using Bjd.Threading;
using System.Threading.Tasks;

namespace Bjd.Logs
{
    public class LogInteractiveConsoleService : IDisposable, ILogService
    {
        public event EventHandler Appended;

        private long CurrentNo = 1;
        private long LastNo = 1;
        private int MaxRows = 50;
        private Dictionary<long, string> buffer = new Dictionary<long, string>();
        private bool isDisposed = false;
        private object lockKey = new object();

        public LogInteractiveConsoleService()
        {

        }

        public void Dispose()
        {
            isDisposed = true;
        }

        protected void OnAppended()
        {
            lock (lockKey)
            {
                while (buffer.Count > MaxRows)
                {
                    var key = LastNo++;
                    if (buffer.ContainsKey(key)) buffer.Remove(key);
                }
            }

            if (Appended == null) return;
            Appended(this, EventArgs.Empty);
        }

        private void Write(string message)
        {
            lock (lockKey)
            {
                buffer.Add(CurrentNo++, message);
            }
            OnAppended();
        }

        public void Append(LogMessage oneLog)
        {
            Write(oneLog.ToTraceString());
        }


        public void WriteLine(string message)
        {
            Write(message);
        }


        public void TraceInformation(string message)
        {
            Write(message);
        }

        public void TraceWarning(string message)
        {
            Write(message);
        }

        public void TraceError(string message)
        {
            Write(message);
        }

        public string[] GetBuffer()
        {
            lock (lockKey)
            {
                return buffer.OrderBy(_ => _.Key).Select(_ => _.Value).ToArray();
            }
        }


    }
}

