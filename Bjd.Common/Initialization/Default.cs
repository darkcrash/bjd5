using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.Initialization
{
    public class Default
    {
        Kernel _kernel;
        static System.Threading.ManualResetEvent signal = new System.Threading.ManualResetEvent(false);
        static Default instance = new Default();

        public static void Start()
        {
            // Add console trace
            Trace.TraceInformation("DefaultService.ServiceMain Start");

            // service start
            Default.instance.OnStart();
            Console.CancelKeyPress += Console_CancelKeyPress;
            signal.WaitOne();
            Trace.TraceInformation("DefaultService.ServiceMain End");
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Trace.TraceInformation("DefaultService.ConsoleCancel Start");
            e.Cancel = true;
            Default.instance.OnStop();
            signal.Set();
            Trace.TraceInformation("DefaultService.ConsoleCancel End");
        }

        protected void OnStart()
        {
            Trace.TraceInformation("DefaultService.OnStart Start");
            _kernel = new Kernel();
            _kernel.ListInitialize();
            _kernel.Start();
            Trace.TraceInformation("DefaultService.OnStart End");
        }

        protected void OnStop()
        {
            Trace.TraceInformation("DefaultService.OnStop Start");
            _kernel.Stop();
            _kernel.Dispose();
            _kernel = null;
            Trace.TraceInformation("DefaultService.OnStop End");
        }

    }



}
