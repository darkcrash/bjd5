using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Bjd.Test.Logs
{
    public class TestOutputService : System.Diagnostics.TraceListener
    {
        StringBuilder sb = new StringBuilder();
        ITestOutputHelper helper;
        public TestOutputService(ITestOutputHelper helper)
        {
            this.helper = helper;
        }

        public static TestOutputService CreateListener(ITestOutputHelper helper)
        {
            var item =  new TestOutputService(helper);
            System.Diagnostics.Trace.Listeners.Add(item);
            return item;
        }

        public override void Write(string message)
        {
            sb.Append(message);
        }

        public override void WriteLine(string message)
        {
            this.helper.WriteLine(sb.ToString() +  message);
            sb.Length = 0;
        }
    }
}
