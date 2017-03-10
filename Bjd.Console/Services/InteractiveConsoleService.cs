using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using System.Collections.Generic;
using Bjd.Console.Controls;

namespace Bjd.Services
{
    public class InteractiveConsoleService
    {
        static ControlContext controlContext;
        static System.Threading.CancellationTokenSource sigContext = new System.Threading.CancellationTokenSource();
        static System.Threading.CancellationTokenSource signal = new System.Threading.CancellationTokenSource();
        internal static InteractiveConsoleService instance = new InteractiveConsoleService();

        public static void Start()
        {
            Trace.TraceInformation("InteractiveConsoleService.ServiceMain Start");

            controlContext = new ControlContext(sigContext.Token);

            //Console.WriteLine($"ConsoleTraceListner CodePage={Console.Out.Encoding.CodePage}");
            Define.ChangeOperationSystem += Define_ChangeOperationSystem;
            Define_ChangeOperationSystem(instance, EventArgs.Empty);

            // service start
            InteractiveConsoleService.instance.OnStart();
            System.Console.CancelKeyPress += Console_CancelKeyPress;
            signal.Token.WaitHandle.WaitOne();
            Trace.TraceInformation("InteractiveConsoleService.ServiceMain End");
        }

        public static void Stop()
        {
            Trace.TraceInformation("InteractiveConsoleService.Stop Start");
            InteractiveConsoleService.instance.OnStop();

            sigContext.Cancel();
            controlContext.Dispose();
            controlContext = null;
            sigContext.Dispose();
            signal.Cancel();

            Trace.TraceInformation("InteractiveConsoleService.Stop End");
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
            Trace.TraceInformation("InteractiveConsoleService.ConsoleCancel Start");
            e.Cancel = true;
            Stop();
            Trace.TraceInformation("InteractiveConsoleService.ConsoleCancel End");
        }


        Kernel _kernel;

        public InteractiveConsoleService()
        {
        }

        internal void OnStart()
        {
            Trace.TraceInformation("InteractiveConsoleService.OnStart Start");

            if (_kernel == null)
            {
                _kernel = new Kernel();
                _kernel.Events.RequestLogService += KernelEvents_RequestLogService;
                _kernel.ListInitialize();
                _kernel.Start();

                controlContext.Kernel = _kernel;

            }
            Trace.TraceInformation("InteractiveConsoleService.OnStart End");
        }

        private void KernelEvents_RequestLogService(object sender, EventArgs e)
        {
            var logService = new Logs.LogInteractiveConsoleService();
            controlContext.LogService = logService;
            _kernel.LogServices.Add(logService);
        }


        internal void OnStop()
        {
            Trace.TraceInformation("InteractiveConsoleService.OnStop Start");
            if (_kernel != null)
            {
                _kernel.Stop();
                _kernel.Events.RequestLogService -= KernelEvents_RequestLogService;
                _kernel.Dispose();
                _kernel = null;
                controlContext.Kernel = null;
            }
            Trace.TraceInformation("InteractiveConsoleService.OnStop End");
        }


    }



}
