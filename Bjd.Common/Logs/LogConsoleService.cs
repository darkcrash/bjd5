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
    public class LogConsoleService : IDisposable, ILogService
    {

        private bool isDisposed = false;

        public LogConsoleService()
        {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error .ctor ConsoleTraceListner");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
        }


        public void Append(LogMessage oneLog)
        {
            Console.WriteLine(oneLog.ToTraceString());
        }


        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }


        public void TraceInformation(string message)
        {
            Console.WriteLine(message);
        }

        public void TraceWarning(string message)
        {
            Console.WriteLine(message);
        }

        public void TraceError(string message)
        {
            Console.WriteLine(message);
        }

    }
}

