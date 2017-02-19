using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Bjd.Utils;
using Bjd.Threading;
using System.Threading.Tasks;

namespace Bjd.Logs
{
    public class LogConsoleService : IDisposable, ILogService
    {

        private bool isDisposed = false;

        public LogConsoleService()
        {
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
                //Console.WriteLine($"ConsoleTraceListner CodePage={Console.Out.Encoding.CodePage}");
                Define.ChangeOperationSystem += Define_ChangeOperationSystem;
                Define_ChangeOperationSystem(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error .ctor ConsoleTraceListner");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }

        public void Dispose()
        {
            isDisposed = true;
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

        public void Append(LogMessage oneLog)
        {
            Console.WriteLine(oneLog.ToTraceString());
        }


        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }


        public void TraceInformation(string message)
        {
            Console.WriteLine(message);
        }

        public void TraceWarning(string message)
        {
            Console.WriteLine(message);
        }

        public void TraceError(string message)
        {
            Console.WriteLine(message);
        }

    }
}

