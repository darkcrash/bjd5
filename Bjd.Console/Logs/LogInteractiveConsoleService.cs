using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Bjd.Utils;
using Bjd.Threading;
using System.Threading;
using System.Threading.Tasks;
using Bjd.Memory;

namespace Bjd.Logs
{
    public class LogInteractiveConsoleService : IDisposable, ILogService
    {
        public event EventHandler Appended;


        private const int MaxRows = 100;
        private string[] bufferArray = new string[MaxRows];
        private int currentIndex = -1;
        private bool isDisposed = false;

        public bool DetailEnabled { get; set; } = false;
        public bool InformationEnabled { get; set; } = false;
        public bool WarningEnabled { get; set; } = false;


        public LogInteractiveConsoleService()
        {
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
        }

        protected void OnAppended()
        {
            if (Appended == null) return;
            Appended(this, EventArgs.Empty);
        }

        private void Write(string message)
        {
            var idx = Interlocked.Increment(ref currentIndex);
            idx = idx % MaxRows;
            Interlocked.CompareExchange(ref currentIndex, idx, MaxRows + idx);
            bufferArray[idx] = message;
            OnAppended();
        }


        public void Append(CharsData message, LogMessage log)
        {
            if (!DetailEnabled && log.LogKind == LogKind.Detail) return;
            Write(log.ToTraceString());
        }


        public void WriteLine(CharsData message)
        {
            Write(message.ToString());
        }


        public void TraceInformation(CharsData message)
        {
            if (!InformationEnabled) return;
            Write(message.ToString());
        }

        public void TraceWarning(CharsData message)
        {
            if (!WarningEnabled) return;
            Write(message.ToString());
        }

        public void TraceError(CharsData message)
        {
            Write(message.ToString());
        }

        public string[] GetBuffer(int row)
        {
            var cIndex = currentIndex;
            string[] result = new string[row];
            for (var i = 0; i < row; i++ )
            {
                var idx = (cIndex + MaxRows - i) % MaxRows;
                result[i] = bufferArray[idx];
            }
            return result;
        }


    }
}

