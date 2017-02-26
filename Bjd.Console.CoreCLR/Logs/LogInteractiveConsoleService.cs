using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Bjd.Utils;
using Bjd.Threading;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Logs
{
    public class LogInteractiveConsoleService : IDisposable, ILogService
    {
        public event EventHandler Appended;


        private long CurrentNo = 1;
        private long LastNo = 1;
        private int MaxRows = 100;
        private Dictionary<long, string> buffer = new Dictionary<long, string>();
        private SortedDictionary<long, string> bufferSorted = new SortedDictionary<long, string>();
        private bool isDisposed = false;
        private object lockKey = new object();

        public bool DetailEnabled { get; set; } = false;
        public bool InformationEnabled { get; set; } = false;
        public bool WarningEnabled { get; set; } = false;

        public long OutputCount { get { return CurrentNo; } }

        public LogInteractiveConsoleService()
        {
        }

        public void Dispose()
        {
            isDisposed = true;
        }

        protected void OnAppended()
        {
            if (Appended == null) return;
            Appended(this, EventArgs.Empty);
        }

        private void Write(string message)
        {
            lock (lockKey)
            {
                buffer.Add(CurrentNo++, message);
                while (buffer.Count > MaxRows)
                {
                    var key = LastNo++;
                    if (buffer.ContainsKey(key)) buffer.Remove(key);
                }
            }
            OnAppended();
        }

        public void Append(LogMessage oneLog)
        {
            if (!DetailEnabled && oneLog.LogKind == LogKind.Detail) return;
            Write(oneLog.ToTraceString());
        }


        public void WriteLine(string message)
        {
            Write(message);
        }


        public void TraceInformation(string message)
        {
            if (!InformationEnabled) return;
            Write(message);
        }

        public void TraceWarning(string message)
        {
            if (!WarningEnabled) return;
            Write(message);
        }

        public void TraceError(string message)
        {
            Write(message);
        }

        public string[] GetBuffer(int row)
        {
            KeyValuePair<long, string>[] buf;
            string[] result;
            lock (lockKey)
            {
                result = buffer.OrderByDescending(_ => _.Key).Take(row).Select(_ => _.Value).ToArray();
                //result = buf.Select(_ => _.Value).ToArray();
            }
            return result;
        }


    }
}

