using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Bjd.Test.Logs
{
    public class TestOutputService : System.Diagnostics.TraceListener, IDisposable
    {
        StringBuilder sb = new StringBuilder();
        ITestOutputHelper helper;
        public TestOutputService(ITestOutputHelper helper)
        {
            this.helper = helper;
            System.Diagnostics.Trace.Listeners.Add(this);
        }

        bool DisposedValue = false;
        protected override void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    System.Diagnostics.Trace.Listeners.Remove(this);
                }
            }
            base.Dispose(disposing);
        }

        public override void Write(string message)
        {
            sb.Append(message);
        }

        public override void WriteLine(string message)
        {
            this.helper.WriteLine(sb.ToString() + message);
            sb.Length = 0;
        }
    }
}
