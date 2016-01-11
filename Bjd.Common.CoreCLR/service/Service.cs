using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.service
{
    public class Service
    {
        Kernel _kernel;
        static Service instance = new Service();
        static System.Threading.ManualResetEvent signal = new System.Threading.ManualResetEvent(false);

        public static void ServiceMain(IServiceProvider sb)
        {
            foreach (TraceListener l in System.Diagnostics.Trace.Listeners)
            {
                var f = new EventTypeFilter(SourceLevels.Warning);
                l.Filter = f;
            }

            // Add console trace
            System.Diagnostics.Trace.Listeners.Add(new trace.ConsoleTraceListner());
            Trace.TraceInformation("Service.ServiceMain Start");

            // Define Initialize
            Define.Initialize(sb);

            // service start
            Service.instance.OnStart();
            Console.CancelKeyPress += Console_CancelKeyPress;
            signal.WaitOne();
            Trace.TraceInformation("Service.ServiceMain End");
        }

        public static void Restart()
        {
            Trace.TraceInformation("Service.Restart Start");
            instance.OnStop();
            instance.OnStart();
            Trace.TraceInformation("Service.Restart End");
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Trace.TraceInformation("Service.ConsoleCancel Start");
            e.Cancel = true;
            Service.instance.OnStop();
            signal.Set();
            Trace.TraceInformation("Service.ConsoleCancel End");
        }

        protected void OnStart()
        {
            Trace.TraceInformation("Service.OnStart Start");
            _kernel = new Kernel();
            _kernel.Start();
            //_kernel.MenuOnClick("StartStop_Start");
            Trace.TraceInformation("Service.OnStart End");
        }
        protected void OnPause()
        {
            Trace.TraceInformation("Service.OnPause Start");
            //_kernel.MenuOnClick("StartStop_Stop");
            _kernel.Stop();
            Trace.TraceInformation("Service.OnPause End");
        }
        protected void OnContinue()
        {
            Trace.TraceInformation("Service.OnContinue Start");
            //_kernel.MenuOnClick("StartStop_Start");
            _kernel.Start();
            Trace.TraceInformation("Service.OnContinue End");
        }

        protected void OnStop()
        {
            Trace.TraceInformation("Service.OnStop Start");
            //_kernel.MenuOnClick("StartStop_Stop");
            _kernel.Stop();
            _kernel.Dispose();
            _kernel = null;
            Trace.TraceInformation("Service.OnStop End");
        }

    }



}
