using System;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;
using Bjd.Test;
using System.IO;
using Bjd.Net.Sockets;
using Bjd.Net;
using Bjd.Configurations;
using System.Collections.Generic;
using Xunit.Abstractions;
using Bjd.Test.Logs;
using Bjd.Logs;

namespace Bjd.Services
{
    public class TestService : IDisposable
    {
        private static Random rd = new Random();
        private static List<int> useTcpPortList = new List<int>();
        private static List<int> useUdpPortList = new List<int>();
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
                    Enviroments env = null;
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    if (_kernel != null)
                    {
                        env = _kernel.Enviroment;
                        _kernel.Events.RequestLogService -= KernelEvents_RequestLogService;
                        _kernel.Dispose();
                        _kernel = null;
                    }
                    if (lsrvList != null)
                    {
                        foreach (var item in lsrvList)
                        {
                            item.Dispose();
                        }
                        lsrvList.Clear();
                    }
                    if (env != null)
                    {
                        try { Directory.Delete(env.ExecutableDirectory, true); } catch { };
                        try { Directory.Delete(env.ConfigurationDirectory, true); } catch { };
                    }
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


        }


        private Kernel _kernel;
        public Kernel Kernel
        {
            get
            {
                return _kernel;
            }
        }

        private Enviroments Env
        {
            get
            {
                return _kernel.Enviroment;
            }
        }

        public string MailboxPath { get; private set; }
        public string MailQueuePath { get; private set; }

        private static TestService CreateTestServiceInternal()
        {
            var instance = new TestService();
            instance._kernel = new Kernel(true);

            instance._kernel.Events.RequestLogService += instance.KernelEvents_RequestLogService;

            var env = instance._kernel.Enviroment;

            // set executable directory
            var rdval = rd.Next(0, int.MaxValue);
            var dirName = $"{DateTime.Now.ToString("yyyyMMddHHmmssffff")}_{System.Threading.Thread.CurrentThread.ManagedThreadId}_{rdval}_{instance.GetHashCode()}";

            var dir = env.ExecutableDirectory;
            env.ExecutableDirectory = System.IO.Path.Combine(dir, dirName);
            Directory.CreateDirectory(env.ExecutableDirectory);

            //BJD.Lang.txtを作業ディレクトリにコピーする
            Copy(env, "BJD.Lang.txt", "BJD.Lang.txt");

            // メールボックスの生成
            instance.MailboxPath = System.IO.Path.Combine(env.ExecutableDirectory, "mailbox");

            // メールキューの生成
            instance.MailQueuePath = System.IO.Path.Combine(env.ExecutableDirectory, "MailQueue");

            return instance;
        }

        private void KernelEvents_RequestLogService(object sender, EventArgs e)
        {
            foreach (var lts in lsrvList)
            {
                Kernel.LogServices.Add(lts);
            }
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

        List<LogTestService> lsrvList = new List<LogTestService>();
        public void AddOutput(ITestOutputHelper output)
        {
            var lts = new LogTestService(output);
            lsrvList.Add(lts);
            Kernel.LogServices.Add(lts);
        }

        public void ContentFile(params string[] paths)
        {
            var src = Path.Combine(paths);
            var filename = Path.GetFileName(src);
            Copy(this.Kernel.Enviroment, src, filename);
        }

        public void ContentDirectory(params string[] paths)
        {
            var src = Path.Combine(paths);
            CreateDirectory(Env, src);
            var srcPath = System.IO.Path.Combine(ProjectDirectory, src);
            foreach (var f in Directory.GetFiles(srcPath))
            {
                var fPath = f.Substring(ProjectDirectory.Length + 1);
                Copy(Env, fPath, fPath);
            }
            foreach (var d in Directory.GetDirectories(src))
            {
                ContentDirectory(d);
            }
        }

        public void SetOption(params string[] paths)
        {
            var src = Path.Combine(paths);
            Copy(Env, src, "Option.ini");
        }

        public void AddMail(string srcFile, string user)
        {
            var filename = Path.GetFileName(srcFile);
            var destFilepath = Path.Combine(this.MailboxPath, user, filename);
            Copy(Env, srcFile, destFilepath);
        }

        public void AddMailQueue(string srcFile)
        {
            var filename = Path.GetFileName(srcFile);
            var destFilepath = Path.Combine(this.MailQueuePath, filename);
            Copy(Env, srcFile, destFilepath);
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
                //return System.IO.Directory.GetCurrentDirectory();
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
            if (!File.Exists(src))
            {
                throw new Exception($"file not found.{src}");
            }
            File.Copy(src, dst, true);
            return dst;
        }

        private static string CreateDirectory(Enviroments env, string directoryPath)
        {
            var dst = System.IO.Path.Combine(env.ExecutableDirectory, directoryPath);
            if (!Directory.Exists(dst))
                Directory.CreateDirectory(dst);
            return dst;
        }


        //テンポラリディレクトリの作成<br>
        //最初に呼ばれたとき、ディレクトリが存在しないので、新規に作成される
        public string GetTmpDir(string tmpDir)
        {
            var dir = Path.Combine(Kernel.Enviroment.ExecutableDirectory, tmpDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return dir;
        }

        //指定したテンポラリディレクトリ(tmpDir)の中での作成可能なテンポラリファイル(もしくはディレクトリ)名を生成する
        //return テンポラリファイル（ディレクトリ）名(パス)
        public string GetTmpPath(string tmpDir)
        {

            //var filename = string.Format("{0}\\{1}", GetTmpDir(tmpDir), Path.GetRandomFileName());
            var filename = Path.Combine(GetTmpDir(tmpDir), Path.GetRandomFileName());
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            return filename;
        }

        public int GetAvailablePort(IpKind ip, Conf conf)
        {
            var ipobj = new Ip(ip);
            return GetAvailablePort(ipobj, conf);
        }

        public int GetAvailablePort(Ip ip, Conf conf)
        {
            var port = (int)conf.Get("port");
            port = GetAvailablePort(ip, port);
            conf.Set("port", port);
            return port;
        }

        public int GetAvailablePort(string address, int port)
        {
            var ipobj = new Ip(address);
            return GetAvailablePort(ipobj, port);
        }
        public int GetAvailablePort(IpKind ip, int port)
        {
            var ipobj = new Ip(ip);
            return GetAvailablePort(ipobj, port);
        }
        public int GetAvailablePort(Ip ip, int port)
        {
            for (var i = port; i < 65000; i++)
            {
                lock (useTcpPortList)
                {
                    if (useTcpPortList.Contains(i)) continue;
                    if (SockUtil.IsAvailable(Kernel, ip, i))
                    {
                        useTcpPortList.Add(i);
                        return i;
                    }
                }

            }
            throw new Exception($"Available port not found {port}");
        }

        public int GetAvailableUdpPort(Ip ip, Conf conf)
        {
            var port = (int)conf.Get("port");
            port = GetAvailableUdpPort(ip, port);
            conf.Set("port", port);
            return port;
        }
        public int GetAvailableUdpPort(string address, int port)
        {
            var ipobj = new Ip(address);
            return GetAvailableUdpPort(ipobj, port);
        }
        public int GetAvailableUdpPort(IpKind ip, int port)
        {
            var ipobj = new Ip(ip);
            return GetAvailableUdpPort(ipobj, port);
        }
        public int GetAvailableUdpPort(Ip ip, int port)
        {
            for (var i = port; i < 65000; i++)
            {
                lock (useTcpPortList)
                {
                    if (useUdpPortList.Contains(i)) continue;
                    if (SockUtil.IsAvailableUdp(Kernel, ip, i))
                    {
                        useUdpPortList.Add(i);
                        return i;
                    }
                }
            }
            throw new Exception($"Available port not found {port}");
        }

    }

}
