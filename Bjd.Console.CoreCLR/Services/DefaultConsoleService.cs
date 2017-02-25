using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.Services
{
    public class DefaultConsoleService
    {
        Kernel _kernel;
        static System.Threading.ManualResetEvent signal = new System.Threading.ManualResetEvent(false);
        static DefaultConsoleService instance = new DefaultConsoleService();

        public static void Start()
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

            //Console.WriteLine($"ConsoleTraceListner CodePage={Console.Out.Encoding.CodePage}");
            Define.ChangeOperationSystem += Define_ChangeOperationSystem;
            Define_ChangeOperationSystem(instance, EventArgs.Empty);

            // service start
            DefaultConsoleService.instance.OnStart();
            System.Console.CancelKeyPress += Console_CancelKeyPress;
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

        private static void Define_ChangeOperationSystem(object sender, EventArgs e)
        {
            // fix Windows ja-jp to codepage 932
            var lang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            var lang2 = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (Define.IsWindows && lang == "ja")
            {
                var enc = System.Text.CodePagesEncodingProvider.Instance;
                var sjis = enc.GetEncoding(932);
                var writer = new System.IO.StreamWriter(System.Console.OpenStandardOutput(), sjis);
                writer.AutoFlush = true;
                System.Console.SetOut(writer);
            }
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
