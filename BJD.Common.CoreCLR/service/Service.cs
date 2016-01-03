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
        static IServiceProvider serviceProvider;

        public static void ServiceMain(IServiceProvider sb)
        {
            // Add console trace
            System.Diagnostics.Trace.Listeners.Add(new trace.ConsoleTraceListner());
            Trace.WriteLine("Service.ServiceMain Start");

            // Define Initialize
            Define.Initialize(sb);

            // service start
            Service.instance.OnStart();
            Console.CancelKeyPress += Console_CancelKeyPress;
            signal.WaitOne();
            Trace.WriteLine("Service.ServiceMain End");
        }

        public static void Restart()
        {
            Trace.WriteLine("Service.Restart Start");
            instance.OnStop();
            instance.OnStart();
            Trace.WriteLine("Service.Restart End");
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Trace.WriteLine("Service.ConsoleCancel Start");
            e.Cancel = true;
            Service.instance.OnStop();
            signal.Set();
            Trace.WriteLine("Service.ConsoleCancel End");
        }

        protected void OnStart()
        {
            Trace.WriteLine("Service.OnStart Start");




            _kernel = new Kernel();
            _kernel.Start();
            //_kernel.MenuOnClick("StartStop_Start");
            Trace.WriteLine("Service.OnStart End");
        }
        protected void OnPause()
        {
            Trace.WriteLine("Service.OnPause Start");
            //_kernel.MenuOnClick("StartStop_Stop");
            _kernel.Stop();
            Trace.WriteLine("Service.OnPause End");
        }
        protected void OnContinue()
        {
            Trace.WriteLine("Service.OnContinue Start");
            //_kernel.MenuOnClick("StartStop_Start");
            _kernel.Start();
            Trace.WriteLine("Service.OnContinue End");
        }

        protected void OnStop()
        {
            Trace.WriteLine("Service.OnStop Start");
            //_kernel.MenuOnClick("StartStop_Stop");
            _kernel.Stop();
            _kernel.Dispose();
            _kernel = null;
            Trace.WriteLine("Service.OnStop End");
        }

    }



}
