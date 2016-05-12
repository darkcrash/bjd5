using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Bjd.Test.Services
{
    public class TestService
    {
        public static void ServiceTest()
        {
            foreach (TraceListener l in System.Diagnostics.Trace.Listeners)
            {
                var f = new EventTypeFilter(SourceLevels.All);
                l.Filter = f;
            }

            // Add console trace
            //System.Diagnostics.Trace.Listeners.Add(new trace.ConsoleTraceListner());
            Trace.TraceInformation("TestService.ServiceTest Start");

            // Define Initialize
            Define.TestInitalize();

            // service start
            //Service.instance.OnStart();
            //Console.CancelKeyPress += Console_CancelKeyPress;
            //signal.WaitOne();

            Trace.TraceInformation("TestService.ServiceTest End");
        }

    }

}
