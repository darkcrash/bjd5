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
        private static Random rd = new Random();

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
        public string MailboxPath { get; private set; }
        public string MailQueuePath { get; private set; }

        private TestOption _op;


        private static TestService CreateTestServiceInternal()
        {
            var instance = new TestService();

            // set executable directory
            var dirName = $"{DateTime.Now.ToString("yyyyMMddHHmmssffff")}_{System.Threading.Thread.CurrentThread.ManagedThreadId}";

            var env = new Enviroments();
            var dir = env.ExecutableDirectory;
            env.ExecutableDirectory = System.IO.Path.Combine(dir, dirName);
            Directory.CreateDirectory(env.ExecutableDirectory);

            //BJD.Lang.txtを作業ディレクトリにコピーする
            Copy(env, "BJD.Lang.txt", "BJD.Lang.txt");

            // メールボックスの生成
            instance.MailboxPath = System.IO.Path.Combine(env.ExecutableDirectory, "mailbox");

            // メールキューの生成
            instance.MailQueuePath = System.IO.Path.Combine(env.ExecutableDirectory, "MailQueue");


            instance.Kernel = new Kernel(env);

            return instance;
        }

        public static TestService CreateTestService()
        {
            // Add console trace
            //System.Diagnostics.Trace.Listeners.Add(new trace.ConsoleTraceListner());
            Trace.TraceInformation("TestService.ServiceTest Start");

            var instance = CreateTestServiceInternal();

            Trace.TraceInformation("TestService.ServiceTest End");
            return instance;
        }

        public static TestService CreateTestService(TestOption option)
        {
            // Add console trace
            //System.Diagnostics.Trace.Listeners.Add(new trace.ConsoleTraceListner());
            Trace.TraceInformation("TestService.ServiceTest Start");

            var instance = CreateTestServiceInternal();
            instance._op = option;
            instance._op.Copy(instance);

            Trace.TraceInformation("TestService.ServiceTest End");
            return instance;
        }

        public void ContentFile(params string[] paths)
        {
            var src = Path.Combine(paths);
            var filename = Path.GetFileName(src);
            Copy(this.Kernel.Enviroment, src, filename);
        }

        public void AddMail(string srcFile, string user)
        {
            var filename = Path.GetFileName(srcFile);
            var destFilepath = Path.Combine(this.MailboxPath, user, filename);
            Copy(this.Kernel.Enviroment, srcFile, destFilepath);
        }

        public void AddMailQueue(string srcFile)
        {
            var filename = Path.GetFileName(srcFile);
            var destFilepath = Path.Combine(this.MailQueuePath, filename);
            Copy(this.Kernel.Enviroment, srcFile, destFilepath);
        }

        public void CreateMailbox(string username)
        {
            var boxPath = Path.Combine(this.MailboxPath, username);
            Directory.CreateDirectory(boxPath);
        }


        public static string ProjectDirectory
        {
            get
            {
                var parent = System.IO.Path.GetDirectoryName(AppContext.BaseDirectory);
                parent = System.IO.Path.GetDirectoryName(parent);
                parent = System.IO.Path.GetDirectoryName(parent);
                return parent;
            }
        }

        private static string Copy(Enviroments env, string contentFile, string destnationFilename)
        {
            var src = System.IO.Path.Combine(ProjectDirectory, contentFile);
            var dst = System.IO.Path.Combine(env.ExecutableDirectory, destnationFilename);
            var dir = Path.GetDirectoryName(dst);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (File.Exists(src))
            {
                File.Copy(src, dst, true);
            }

            return dst;
        }

    }

}
