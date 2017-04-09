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
using Bjd.Logs;

namespace Bjd.RemoteServer.Logs
{
    public class RemoteLogService : IDisposable, ILogService
    {
        internal Server remoteServer;

        public RemoteLogService()
        {
        }

        public void Dispose()
        {
        }


        private void Write(string message)
        {
        }


        public void Append(CharsData message, LogMessage log)
        {
            remoteServer?.Append(log);
        }


        public void WriteLine(CharsData message)
        {
        }


        public void TraceInformation(CharsData message)
        {
        }

        public void TraceWarning(CharsData message)
        {
        }

        public void TraceError(CharsData message)
        {
        }


    }
}

