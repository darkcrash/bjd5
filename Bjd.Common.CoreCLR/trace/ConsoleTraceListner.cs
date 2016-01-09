﻿using System;
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
            try
            {
                if (Console.WindowWidth < 200)
                    Console.WindowWidth = 200;
            }
            catch (Exception)
            {
                Console.WriteLine("Not allowed change Console.WindowWidth");
            }

            try
            {
                Console.WriteLine($"ConsoleTraceListner CodePage={Console.Out.Encoding.CodePage}");
                Define.ChangeOperationSystem += Define_ChangeOperationSystem;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error .ctor ConsoleTraceListner");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }

        private void Define_ChangeOperationSystem(object sender, EventArgs e)
        {
            // fix Windows ja-jp to codepage 932
            var lang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            var lang2 = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (Define.IsWindows && lang == "ja")
            {
                var enc = System.Text.CodePagesEncodingProvider.Instance;
                var sjis = enc.GetEncoding(932);
                var writer = new System.IO.StreamWriter(Console.OpenStandardOutput(), sjis);
                writer.AutoFlush = true;
                Console.SetOut(writer);
            }
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