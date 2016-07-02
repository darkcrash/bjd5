using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Bjd.Browse;
using Bjd.Logs;
using Bjd.Mails;
using Bjd.Net;
using Bjd.Options;
using Bjd.Plugins;
using Bjd.Remote;
using Bjd.Servers;
using Bjd.Net.Sockets;
using Bjd.Traces;
using Bjd.Utils;

namespace Bjd
{
    public sealed class Kernel : IDisposable
    {
        public Enviroments Enviroment { get; private set; } = new Enviroments();


        //プロセス起動時に初期化される変数 
        public RemoteConnect RemoteConnect { get; set; } //リモート制御で接続されている時だけ初期化される
        public DnsCache DnsCache { get; private set; }
        public MailBox MailBox { get; private set; }

        //サーバ起動時に最初期化される変数
        public ListOption ListOption { get; private set; }
        public ListServer ListServer { get; private set; }
        public LogFile LogFile { get; private set; }
        public bool IsJp { get; private set; } = true;
        private Logger _logger;

        public KernelEvents Events { get; private set; } = new KernelEvents();

        //Ver5.9.6
        public WebApi WebApi { get; private set; }

        //Ver5.8.6
        public IniDb Configuration { get; private set; }

        private CancellationTokenSource CancelTokenSource { get; set; }

        private CancellationToken CancelToken { get;  set; }


        public string ServerName
        {
            get
            {
                var oneOption = ListOption.Get("Basic");
                if (oneOption != null)
                {
                    return (String)oneOption.GetValue("serverName");
                }
                return "";
            }
        }


        private void OnCancel()
        {
            Events.OnCancel(this);
        }

        //* 通常使用されるコンストラクタ
        internal Kernel(Enviroments env)
        {
            Trace.TraceInformation("Kernel..ctor Start");
            this.Enviroment = env;
            DefaultInitialize();
            Trace.TraceInformation("Kernel..ctor End");
        }

        //起動時に、コンストラクタから呼び出される初期化
        private void DefaultInitialize()
        {
            Trace.TraceInformation("Kernel.DefaultInitialize Start");

            this.CancelTokenSource = new CancellationTokenSource();
            this.CancelToken = this.CancelTokenSource.Token;
            this.CancelToken.Register(this.OnCancel);

            RemoteConnect = null;//リモート制御で接続されている時だけ初期化される

            //プロセス起動時に初期化される
            DnsCache = new DnsCache();

            Configuration = new IniDb(this.Enviroment.ExecutableDirectory, "Option");

            MailBox = null;

            ListInitialize(); //サーバ再起動で、再度実行される初期化 

            //ウインドサイズの復元
            //var path = string.Format("{0}\\BJD.ini", Define.ExecutableDirectory);
            var path = $"{this.Enviroment.ExecutableDirectory}{Path.DirectorySeparatorChar}BJD.ini";

            Trace.TraceInformation("Kernel.DefaultInitialize End");
        }

        //サーバ再起動で、再度実行される初期化
        public void ListInitialize()
        {
            Trace.TraceInformation("Kernel.ListInitialize Start");
            //Loggerが使用できない間のログは、こちらに保存して、後でLoggerに送る
            var tmpLogger = new TmpLogger();

            //************************************************************
            // 破棄
            //************************************************************
            if (ListOption != null)
            {
                ListOption.Dispose();
                ListOption = null;
            }
            if (ListServer != null)
            {
                ListServer.Dispose();
                ListServer = null;
            }
            if (MailBox != null)
            {
                MailBox = null;
            }
            if (LogFile != null)
            {
                LogFile.Dispose();
                LogFile = null;
            }

            //var listPlugin = new ListPlugin(Define.ExecutableDirectory);
            var listPlugin = new ListPlugin();
            foreach (var o in listPlugin)
            {
                tmpLogger.Set(LogKind.Detail, null, 9000008, string.Format("{0}Server", o.Name));
            }

            IsJp = Configuration.IsJp();

            ListOption = new ListOption(this, listPlugin);

            //OptionBasic
            var confBasic = new Conf(ListOption.Get("Basic"));

            //OptionLog
            var confOption = new Conf(ListOption.Get("Log"));

            //LogFileの初期化
            var saveDirectory = (String)confOption.Get("saveDirectory");
            saveDirectory = ReplaceOptionEnv(saveDirectory);
            var normalLogKind = (int)confOption.Get("normalLogKind");
            var secureLogKind = (int)confOption.Get("secureLogKind");
            var saveDays = (int)confOption.Get("saveDays");

            //Ver6.0.7
            var useLogFile = (bool)confOption.Get("useLogFile");
            var useLogClear = (bool)confOption.Get("useLogClear");
            if (!useLogClear)
            {
                saveDays = 0; //ログの自動削除が無効な場合、saveDaysに0をセットする
            }
            if (saveDirectory == "")
            {
                tmpLogger.Set(LogKind.Error, null, 9000045, "It is not appointed");
            }
            else
            {
                tmpLogger.Set(LogKind.Detail, null, 9000032, saveDirectory);
                try
                {
                    LogFile = new LogFile(saveDirectory, normalLogKind, secureLogKind, saveDays, useLogFile);
                }
                catch (IOException e)
                {
                    LogFile = null;
                    tmpLogger.Set(LogKind.Error, null, 9000031, e.Message);
                }
            }

            //Ver5.8.7 Java fix
            //mailBox初期化
            var useMailBoxTag = new[] { "Smtp", "Pop3", "WebApi" };
            foreach (var o in ListOption.Where(_ => useMailBoxTag.Contains(_.NameTag) && _.UseServer))
            {
                //SmtpServer若しくは、Pop3Serverが使用される場合のみメールボックスを初期化する                
                var op = ListOption.Get("MailBox");
                var conf = new Conf(op);
                var dir = ReplaceOptionEnv((String)conf.Get("dir"));
                var datUser = (Dat)conf.Get("user");
                var logger = CreateLogger("MailBox", (bool)conf.Get("useDetailsLog"), null);
                var dirFullPath = Path.Combine(this.Enviroment.ExecutableDirectory, dir);
                MailBox = new MailBox(logger, datUser, dirFullPath);
                break;
            }

            _logger = CreateLogger("kernel", true, null);
            tmpLogger.Release(_logger);

            ListServer = new ListServer(this, listPlugin);

            //ListTool = new ListTool();
            //ListTool.Initialize(this);

            WebApi = new WebApi();

            Trace.TraceInformation("Kernel.ListInitialize End");
        }

        //Confの生成
        //事前にListOptionが初期化されている必要がある
        public Conf CreateConf(String nameTag)
        {
            Trace.TraceInformation($"Kernel.CreateConf {nameTag}");
            try
            {
                if (ListOption == null)
                {
                    Util.RuntimeException("createConf() ListOption==null");
                    return null;
                }
                var oneOption = ListOption.Get(nameTag);
                if (oneOption != null)
                {
                    return new Conf(oneOption);
                }
                return null;
            }
            finally
            {
                //Trace.TraceInformation($"Kernel.CreateConf {nameTag} End");
            }
        }

        //Loggerの生成
        //事前にListOptionが初期化されている必要がある
        public Logger CreateLogger(String nameTag, bool useDetailsLog, ILogger logger)
        {
            Trace.TraceInformation($"Kernel.CreateLogger {nameTag} useDetailsLog={useDetailsLog.ToString()}");
            if (ListOption == null)
            {
                Util.RuntimeException("CreateLogger() ListOption==null || LogFile==null");
            }
            var conf = CreateConf("Log");
            if (conf == null)
            {
                //CreateLoggerを使用する際に、OptionLogが検索できないのは、設計上の問題がある
                Util.RuntimeException("CreateLogger() conf==null");
                return null;
            }
            var dat = (Dat)conf.Get("limitString");
            var isDisplay = ((int)conf.Get("isDisplay")) == 0;
            var logLimit = new LogLimit(dat, isDisplay);

            var useLimitString = (bool)conf.Get("useLimitString");
            return new Logger(this, logLimit, LogFile, IsJp, nameTag, useDetailsLog, useLimitString, logger);
        }

        //終了処理
        public void Dispose()
        {
            Trace.TraceInformation("Kernel.Dispose Start");
            try
            {
                //**********************************************
                // 破棄
                //**********************************************
                //ListServer.Dispose(); //各サーバは停止される
                this.Stop();

                ListOption.Dispose();
                MailBox = null;

            }
            finally
            {
                Trace.TraceInformation("Kernel.Dispose End");
            }
        }

        //オプションで指定される変数を置き変える

        public String ReplaceOptionEnv(String str)
        {
            var executablePath = Define.ExecutableDirectory;
            executablePath = executablePath.Replace("\\\\", "\\\\\\\\");
            str = str.Replace("%ExecutablePath%", executablePath);
            return str;
        }

        internal void Start()
        {
            if (ListServer.Count == 0)
            {
                _logger.Set(LogKind.Error, null, 9000030, "");
            }
            else
            {
                ListServer.Start();
            }
        }

        internal void Stop()
        {
            this.CancelTokenSource.Cancel();
            this.CancelToken.WaitHandle.WaitOne(5000);
            ListServer.Stop();
        }

        //リモート操作(データの取得)
        public string Cmd(string cmdStr)
        {
            var sb = new StringBuilder();


            sb.Append(IsJp ? "(1) サービス状態" : "(1) Service Status");
            sb.Append("\b");

            foreach (var sv in ListServer)
            {
                sb.Append("  " + sv);
                sb.Append("\b");
            }
            sb.Append(" \b");

            sb.Append(IsJp ? "(2) ローカルアドレス" : "(2) Local address");
            sb.Append("\b");
            foreach (string addr in Define.ServerAddressList)
            {
                sb.Append(string.Format("  {0}", addr));
                sb.Append("\b");
            }

            return sb.ToString();
        }

        public String ChangeTag(String src)
        {
            var tagList = new[] { "$h", "$v", "$p", "$d", "$a", "$s" };

            foreach (var tag in tagList)
            {
                while (true)
                {
                    var index = src.IndexOf(tag);
                    if (index == -1)
                    {
                        break;
                    }
                    var tmp1 = src.Substring(0, index);
                    var tmp2 = "";
                    switch (tag)
                    {
                        case "$h":
                            var serverName = ServerName;
                            tmp2 = serverName == "" ? "localhost" : serverName;
                            break;
                        case "$v":
                            tmp2 = this.Enviroment.ProductVersion;
                            break;
                        case "$p":
                            tmp2 = this.Enviroment.ApplicationName;
                            break;
                        case "$d":
                            tmp2 = DateTime.Now.ToDateTimeString();
                            break;
                        case "$a":
                            var localAddress = LocalAddress.GetInstance();
                            tmp2 = localAddress.RemoteStr();
                            //tmp2 = Define.ServerAddress();
                            break;
                        case "$s":
                            tmp2 = ServerName;
                            break;
                        default:
                            Util.RuntimeException(string.Format("undefind tag = {0}", tag));
                            break;
                    }
                    var tmp3 = src.Substring(index + 2);
                    src = tmp1 + tmp2 + tmp3;
                }
            }
            return src;
        }

        //IPアドレスの一覧取得
        public List<Ip> GetIpList(String hostName)
        {
            var ar = new List<Ip>();
            try
            {
                var ip = new Ip(hostName);
                ar.Add(ip);
            }
            catch (ValidObjException)
            {
                ar = DnsCache.GetAddress(hostName).ToList();
            }
            return ar;
        }

        public string GetBrowseInfo(string path)
        {
            var sb = new StringBuilder();
            try
            {
                if (path == "")
                {
                    //path = "\\";
                    path = $"{Path.DirectorySeparatorChar}";
                }

                {
                    string[] dirs = Directory.GetDirectories(path);
                    Array.Sort(dirs);
                    foreach (string s in dirs)
                    {
                        var name = s.Substring(path.Length);
                        var info = new DirectoryInfo(s);
                        const long size = 0;
                        var dt = info.LastWriteTime;
                        var p = new OneBrowse(BrowseKind.Dir, name, size, dt); //１データ生成
                        sb.Append(p + "\t"); //送信文字列生成
                    }
                    var files = Directory.GetFiles(path);
                    Array.Sort(files);
                    foreach (var s in files)
                    {
                        var name = s.Substring(path.Length);
                        var info = new FileInfo(s);
                        var size = info.Length;
                        var dt = info.LastWriteTime;
                        var p = new OneBrowse(BrowseKind.File, name, size, dt); //１データ生成
                        sb.Append(p + "\t"); //送信文字列生成
                    }
                }
            }
            catch
            {
                sb.Length = 0;
            }
            return sb.ToString();
        }

    }
}


