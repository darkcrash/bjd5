using NUnitLite;
using NUnit.Framework;
using System;
using System.Reflection;

namespace Bjd.CoreCLR.TestRunner
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

                var asm = typeof(BjdTest.ThreadBaseTest).GetTypeInfo().Assembly;

                var enc = System.Text.Encoding.UTF8;
                var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                if (lang.Equals("ja"))
                {
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    enc = System.Text.Encoding.GetEncoding(932);
                }

                var writer = new System.IO.StreamWriter(Console.OpenStandardOutput(), enc);
                writer.AutoFlush = true;
                var nunitwriter = new NUnit.Common.ExtendedTextWrapper(writer);


                result = new AutoRun(asm).Execute(args, nunitwriter, Console.In);
#else
                result = new AutoRun().Execute(args);
#endif
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                CleanUp();
                Console.ReadLine();
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
