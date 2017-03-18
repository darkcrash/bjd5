using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.Initialization
{
    public class DefaultConsole
    {
        Kernel _kernel;
        static System.Threading.ManualResetEvent signal = new System.Threading.ManualResetEvent(false);
        static DefaultConsole instance = new DefaultConsole();

        public static void Start()
        {
            // Add console trace
            //Kernel.Trace.Listeners.Add(new Traces.ConsoleTraceListner());
            Trace.TraceInformation("DefaultConsole Start");

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
            DefaultConsole.instance.OnStart();
            System.Console.CancelKeyPress += Console_CancelKeyPress;
            signal.WaitOne();
            Trace.TraceInformation("DefaultConsole. End");
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
            Trace.TraceInformation("DefaultConsole.ConsoleCancel Start");
            e.Cancel = true;
            DefaultConsole.instance.OnStop();
            signal.Set();
            Trace.TraceInformation("DefaultConsole.ConsoleCancel End");
        }

        protected void OnStart()
        {
            Trace.TraceInformation("DefaultConsole.OnStart Start");
            _kernel = new Kernel();
            _kernel.Events.RequestLogService += KernelEvents_RequestLogService;
            _kernel.ListInitialize();
            _kernel.Start();
            Trace.TraceInformation("DefaultConsole.OnStart End");
        }

        private void KernelEvents_RequestLogService(object sender, EventArgs e)
        {
            _kernel.LogServices.Add(new Logs.LogConsoleService());
        }

        protected void OnStop()
        {
            Trace.TraceInformation("DefaultConsole.OnStop Start");
            _kernel.Stop();
            _kernel.Events.RequestLogService -= KernelEvents_RequestLogService;
            _kernel.Dispose();
            _kernel = null;
            Trace.TraceInformation("DefaultConsole.OnStop End");
        }

    }



}
