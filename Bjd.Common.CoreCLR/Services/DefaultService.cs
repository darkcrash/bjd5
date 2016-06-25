using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.Services
{
    public class DefaultService
    {
        Kernel _kernel;
        static DefaultService instance = new DefaultService();
        static System.Threading.ManualResetEvent signal = new System.Threading.ManualResetEvent(false);

        public static void ServiceMain()
        {
            foreach (TraceListener l in System.Diagnostics.Trace.Listeners)
            {
                var f = new EventTypeFilter(SourceLevels.Warning);
                l.Filter = f;
            }

            // Add console trace
            System.Diagnostics.Trace.Listeners.Add(new Traces.ConsoleTraceListner());
            Trace.TraceInformation("DefaultService.ServiceMain Start");

            // Define Initialize
            Define.Initialize();

            // service start
            DefaultService.instance.OnStart();
            Console.CancelKeyPress += Console_CancelKeyPress;
            signal.WaitOne();
            Trace.TraceInformation("DefaultService.ServiceMain End");
        }

        public static void Restart()
        {
            Trace.TraceInformation("DefaultService.Restart Start");
            instance.OnStop();
            instance.OnStart();
            Trace.TraceInformation("DefaultService.Restart End");
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Trace.TraceInformation("DefaultService.ConsoleCancel Start");
            e.Cancel = true;
            DefaultService.instance.OnStop();
            signal.Set();
            Trace.TraceInformation("DefaultService.ConsoleCancel End");
        }

        protected void OnStart()
        {
            Trace.TraceInformation("DefaultService.OnStart Start");
            _kernel = new Kernel(new Enviroments());
            _kernel.Start();
            Trace.TraceInformation("DefaultService.OnStart End");
        }
        protected void OnPause()
        {
            Trace.TraceInformation("DefaultService.OnPause Start");
            _kernel.Stop();
            Trace.TraceInformation("DefaultService.OnPause End");
        }
        protected void OnContinue()
        {
            Trace.TraceInformation("DefaultService.OnContinue Start");
            _kernel.Start();
            Trace.TraceInformation("DefaultService.OnContinue End");
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
