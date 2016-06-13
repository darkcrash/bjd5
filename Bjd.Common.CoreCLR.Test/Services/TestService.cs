using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using Bjd.Test;
using System.IO;

namespace Bjd.Services
{
    public class TestService : IDisposable
    {
        private static Random rd = new Random();

        private TestService()
        {
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    if (this._kernel != null)
                    {
                        this._kernel.Dispose();
                    }
                    this._kernel = null;
                    Directory.Delete(this.env.ExecutableDirectory, true);
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~TestService() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion


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

        private Enviroments env;

        private Kernel _kernel;
        public Kernel Kernel
        {
            get
            {
                if (_kernel == null)
                    _kernel = new Kernel(env);
                return _kernel;
            }
        }
        public string MailboxPath { get; private set; }
        public string MailQueuePath { get; private set; }

        private static TestService CreateTestServiceInternal()
        {
            var instance = new TestService();

            // set executable directory
            var dirName = $"{DateTime.Now.ToString("yyyyMMddHHmmssffff")}_{System.Threading.Thread.CurrentThread.ManagedThreadId}";

            instance.env = new Enviroments();
            var dir = instance.env.ExecutableDirectory;
            instance.env.ExecutableDirectory = System.IO.Path.Combine(dir, dirName);
            Directory.CreateDirectory(instance.env.ExecutableDirectory);

            //BJD.Lang.txtを作業ディレクトリにコピーする
            Copy(instance.env, "BJD.Lang.txt", "BJD.Lang.txt");

            // メールボックスの生成
            instance.MailboxPath = System.IO.Path.Combine(instance.env.ExecutableDirectory, "mailbox");

            // メールキューの生成
            instance.MailQueuePath = System.IO.Path.Combine(instance.env.ExecutableDirectory, "MailQueue");



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

        public void ContentFile(params string[] paths)
        {
            var src = Path.Combine(paths);
            var filename = Path.GetFileName(src);
            Copy(this.env, src, filename);
        }

        public void ContentDirectory(params string[] paths)
        {
            var src = Path.Combine(paths);
            CreateDirectory(this.env, src);
            var srcPath = System.IO.Path.Combine(ProjectDirectory, src);
            foreach (var f in Directory.GetFiles(srcPath))
            {
                var fPath = f.Substring(ProjectDirectory.Length + 1);
                Copy(this.env, fPath, fPath);
            }
            foreach (var d in Directory.GetDirectories(src))
            {
                ContentDirectory(d);
            }
        }

        public void SetOption(params string[] paths)
        {
            var src = Path.Combine(paths);
            //var filename = Path.GetFileName(src);
            Copy(this.env, src, "Option.ini");
        }

        public void AddMail(string srcFile, string user)
        {
            var filename = Path.GetFileName(srcFile);
            var destFilepath = Path.Combine(this.MailboxPath, user, filename);
            Copy(this.env, srcFile, destFilepath);
        }

        public void AddMailQueue(string srcFile)
        {
            var filename = Path.GetFileName(srcFile);
            var destFilepath = Path.Combine(this.MailQueuePath, filename);
            Copy(this.env, srcFile, destFilepath);
        }

        public void CreateMailbox(string username)
        {
            var boxPath = Path.Combine(this.MailboxPath, username);
            Directory.CreateDirectory(boxPath);
        }

        public void CleanMailbox(string username)
        {
            var boxPath = Path.Combine(this.MailboxPath, username);
            Directory.Delete(boxPath, true);
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

        private static string CreateDirectory(Enviroments env,  string directoryPath)
        {
            var dst = System.IO.Path.Combine(env.ExecutableDirectory, directoryPath);
            if (!Directory.Exists(dst))
                Directory.CreateDirectory(dst);
            return dst;
        }


    }

}
