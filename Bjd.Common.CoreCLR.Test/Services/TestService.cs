using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using Bjd.Test;
using System.IO;

namespace Bjd.Services
{
    public class TestService
    {

        private TestService()
        {
        }

        static TestService()
        {
            foreach (TraceListener l in System.Diagnostics.Trace.Listeners)
            {
                var f = new EventTypeFilter(SourceLevels.All);
                l.Filter = f;
            }

            // Define Initialize
            Define.TestInitalize();

        }

        public Kernel Kernel { get; private set; }

        private TmpOption _op;

        private static TestService CreateTestServiceInternal()
        {
            var instance = new TestService();

            // set executable directory
            var dirName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            var env = new Enviroments();
            var dir = env.ExecutableDirectory;
            env.ExecutableDirectory = System.IO.Path.Combine(dir, dirName);
            Directory.CreateDirectory(env.ExecutableDirectory);
            CopyLangTxt(env);

            instance.Kernel = new Kernel(env);

            return instance;
        }

        public static TestService CreateTestService()
        {
            // Add console trace
            //System.Diagnostics.Trace.Listeners.Add(new trace.ConsoleTraceListner());
            Trace.TraceInformation("TestService.ServiceTest Start");

            var instance = CreateTestServiceInternal();
            instance._op = new TmpOption(".", "Option.ini");
            instance._op.Backup();

            Trace.TraceInformation("TestService.ServiceTest End");
            return instance;
        }

        public static TestService CreateTestService(TmpOption option)
        {
            // Add console trace
            //System.Diagnostics.Trace.Listeners.Add(new trace.ConsoleTraceListner());
            Trace.TraceInformation("TestService.ServiceTest Start");

            var instance = CreateTestServiceInternal();
            instance._op = option;
            instance._op.Backup();

            Trace.TraceInformation("TestService.ServiceTest End");
            return instance;
        }

        public static string ProjectDirectory
        {
            get
            {
                return System.IO.Directory.GetCurrentDirectory();
            }
        }

        //BJD.Lang.txtを作業ディレクトリにコピーする
        private static void CopyLangTxt(Enviroments env)
        {
            //var src = string.Format("{0}\\BJD.Lang.txt", ProjectDirectory() + "\\Bjd.CoreCLR");
            //var dst = string.Format("{0}\\BJD.Lang.txt", Directory.GetCurrentDirectory());
            var src = System.IO.Path.Combine(ProjectDirectory, "BJD.Lang.txt");
            var dst = System.IO.Path.Combine(env.ExecutableDirectory, "BJD.Lang.txt");
            if (File.Exists(src))
            {
                File.Copy(src, dst, true);
            }
        }


    }

}
