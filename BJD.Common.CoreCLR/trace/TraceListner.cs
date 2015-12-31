using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.trace
{
    public class ConsoleTraceListner : System.Diagnostics.TraceListener
    {
        public ConsoleTraceListner()
        {
            this.TraceOutputOptions = System.Diagnostics.TraceOptions.DateTime | System.Diagnostics.TraceOptions.ThreadId;
            this.IndentSize = 2;
            if (Console.WindowWidth < 200)
                Console.WindowWidth = 200;
        }

        public override void Write(string message)
        {
            if (this.NeedIndent)
                this.WriteIndent();
            Console.Write(message);
        }
        protected override void WriteIndent()
        {
            Console.Write(new string(' ', this.IndentLevel * this.IndentSize));
        }

        public override void WriteLine(string message)
        {
            if (this.NeedIndent)
                this.WriteIndent();
            Console.WriteLine(message);
        }
    }


}
