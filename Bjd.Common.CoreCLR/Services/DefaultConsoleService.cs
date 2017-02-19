using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.Services
{
    public class DefaultConsoleService
    {
        Kernel _kernel;
        static DefaultConsoleService instance = new DefaultConsoleService();
        static System.Threading.ManualResetEvent signal = new System.Threading.ManualResetEvent(false);

        public static void ServiceMain()
        {
            // Add console trace
            //Kernel.Trace.Listeners.Add(new Traces.ConsoleTraceListner());
            Trace.TraceInformation("DefaultConsoleService.ServiceMain Start");

//#if DEBUG
//            var f = new EventTypeFilter(SourceLevels.All);
//#else
//            var f = new EventTypeFilter(SourceLevels.Warning);
//#endif
//            // filter Trace
//            foreach (TraceListener l in System.Diagnostics.Trace.Listeners)
//            {
//                l.Filter = f;
//            }
//            Trace.UseGlobalLock = false;

            //// Define Initialize
            //Define.Initialize();

            // service start
            DefaultConsoleService.instance.OnStart();
            Console.CancelKeyPress += Console_CancelKeyPress;
            signal.WaitOne();
            Trace.TraceInformation("DefaultConsoleService.ServiceMain End");
        }

        public static void Restart()
        {
            Trace.TraceInformation("DefaultConsoleService.Restart Start");
            instance.OnStop();
            instance.OnStart();
            Trace.TraceInformation("DefaultConsoleService.Restart End");
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Trace.TraceInformation("DefaultService.ConsoleCancel Start");
            e.Cancel = true;
            DefaultConsoleService.instance.OnStop();
            signal.Set();
            Trace.TraceInformation("DefaultService.ConsoleCancel End");
        }

        protected void OnStart()
        {
            Trace.TraceInformation("DefaultConsoleService.OnStart Start");
            _kernel = new Kernel();
            _kernel.Events.RequestLogService += KernelEvents_RequestLogService;
            _kernel.ListInitialize();
            _kernel.Start();
            Trace.TraceInformation("DefaultConsoleService.OnStart End");
        }

        private void KernelEvents_RequestLogService(object sender, EventArgs e)
        {
            _kernel.LogServices.Add(new Logs.LogConsoleService());
        }

        protected void OnPause()
        {
            Trace.TraceInformation("DefaultConsoleService.OnPause Start");
            _kernel.Stop();
            Trace.TraceInformation("DefaultConsoleService.OnPause End");
        }
        protected void OnContinue()
        {
            Trace.TraceInformation("DefaultConsoleService.OnContinue Start");
            _kernel.Start();
            Trace.TraceInformation("DefaultConsoleService.OnContinue End");
        }

        protected void OnStop()
        {
            Trace.TraceInformation("DefaultConsoleService.OnStop Start");
            _kernel.Stop();
            _kernel.Events.RequestLogService -= KernelEvents_RequestLogService;
            _kernel.Dispose();
            _kernel = null;
            Trace.TraceInformation("DefaultConsoleService.OnStop End");
        }

    }



}
