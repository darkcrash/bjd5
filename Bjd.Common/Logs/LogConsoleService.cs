﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Bjd.Utils;
using Bjd.Threading;
using System.Threading.Tasks;
using Bjd.Memory;

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


        public void Append(CharsData message, LogMessage log)
        {
            //Console.WriteLine(message.Data, 0, message.DataSize);
        }

        public void TraceAppend(CharsData message, LogMessage log)
        {
            Console.WriteLine(message.Data, 0, message.DataSize);
        }


        public void WriteLine(CharsData message)
        {
            Console.WriteLine(message.Data, 0, message.DataSize);
        }


        public void TraceInformation(CharsData message)
        {
            Console.WriteLine(message.Data, 0, message.DataSize);
        }

        public void TraceWarning(CharsData message)
        {
            Console.WriteLine(message.Data, 0, message.DataSize);
        }

        public void TraceError(CharsData message)
        {
            Console.WriteLine(message.Data, 0, message.DataSize);
        }

    }
}

