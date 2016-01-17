using NUnitLite;
using System;
using System.Reflection;

namespace Bjd.CoreCLR.Test
{
    public class Program
    {
        private static IServiceProvider _serviceProvider;
        public Program(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Define.Initialize(_serviceProvider);
        }

        public static int Main(string[] args)
        {
            int result = 0;
            try
            {
                CleanUp();

#if DNXCORE50
                var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                if (lang.Equals("ja"))
                {
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    var enc = System.Text.Encoding.GetEncoding(932);
                    var writer = new System.IO.StreamWriter(Console.OpenStandardOutput(), enc);
                    writer.AutoFlush = true;
                    Console.SetOut(writer);
                }
                result = new AutoRun().Execute(typeof(Program).GetTypeInfo().Assembly, Console.Out, Console.In, args);
#else
            result = new AutoRun().Execute(args);
#endif
            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                CleanUp();
            }
            return result;
        }

        public static void CleanUp()
        {
            foreach (var f in System.IO.Directory.GetDirectories("LogFileTest"))
            {
                System.IO.Directory.Delete(f, true);
            }
        }
    }
}
