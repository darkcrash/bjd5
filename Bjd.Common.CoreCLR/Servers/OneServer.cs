using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bjd.Acls;
using Bjd.Controls;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Options;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Threading;

namespace Bjd.Servers
{

    //OneServer １つのバインドアドレス：ポートごとにサーバを表現するクラス<br>
    //各サーバオブジェクトの基底クラス<br>
    public abstract class OneServer : ThreadBase
    {
        public Logger Logger;
        public String NameTag { get; private set; }

        protected Conf _conf;
        protected bool IsJp;
        protected int TimeoutSec;//sec

        //Ver5.9.2 Java fix
        protected Ssl ssl = null;
        protected AclList AclList = null;

        private SockServerTcp _sockServerTcp;
        private SockServerUdp _sockServerUdp;

        // 子スレッド管理 - 排他制御オブジェクト
        private readonly object SyncObj = new object();

        // 子スレッドコレクション
        private int _count = 0;
        private ManualResetEventSlim _childNone = new ManualResetEventSlim(true);

        // 同時接続数
        private readonly int _multiple;
        private readonly OneBind _oneBind;

        //ステータス表示用
        public override String ToString()
        {
            var stat = IsJp ? "+ サービス中 " : "+ In execution ";
            if (ThreadBaseKind != ThreadBaseKind.Running)
            {
                stat = IsJp ? "- 停止 " : "- Initialization failure ";
            }
            return string.Format("{0}\t{1,20}\t[{2}\t:{3} {4}]\tThread {5}/{6}", stat, NameTag, _oneBind.Addr, _oneBind.Protocol.ToString().ToUpper(), (int)_conf.Get("port"), Count, _multiple);
        }

        public int Count
        {
            get { return _count; }
            private set
            {
                _count = value;
                if (_count == 0)
                {
                    _childNone.Set();
                }
                else
                {
                    _childNone.Reset();
                }
            }
        }

        //リモート操作(データの取得)
        public String cmd(String cmdStr)
        {
            return "";
        }

        public SockState SockState
        {
            get
            {
                if (_sockServerTcp != null)
                    return _sockServerTcp.SockState;
                if (_sockServerUdp != null)
                    return _sockServerUdp.SockState;
                return SockState.Error;
            }
        }

        //Ver6.1.6
        protected readonly Lang Lang;

        //コンストラクタ
        protected OneServer(Kernel kernel, Conf conf, OneBind oneBind)
            : base(kernel, kernel.CreateLogger(conf.NameTag, true, null))
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            if (conf == null) throw new ArgumentNullException("conf");
            if (oneBind == null) throw new ArgumentNullException("oneBind");

            _kernel = kernel;
            _conf = conf;
            _oneBind = oneBind;

            IsJp = kernel.IsJp;
            NameTag = conf.NameTag;


            //Ver6.1.6
            Lang = new Lang(kernel, IsJp ? LangKind.Jp : LangKind.En, "Server" + conf.NameTag);
            CheckLang();//定義のテスト

            Logger = kernel.CreateLogger(conf.NameTag, (bool)_conf.Get("useDetailsLog"), this);
            _multiple = (int)_conf.Get("multiple");

            //DHCPにはACLが存在しない
            if (NameTag != "Dhcp")
            {
                //ACLリスト 定義が無い場合は、aclListを生成しない
                var acl = (Dat)_conf.Get("acl");
                AclList = new AclList(acl, (int)_conf.Get("enableAcl"), Logger);
            }

            TimeoutSec = (int)_conf.Get("timeOut");

        }


        public override void Dispose()
        {
            base.Dispose();
        }

        //スレッド停止処理
        protected abstract void OnStopServer(); //スレッド停止処理

        protected override void OnStopThread()
        {
            System.Diagnostics.Trace.TraceInformation($"{this.GetType().FullName}.OnStopThread  ");
            OnStopServer(); //子クラスのスレッド停止処理
            if (ssl != null)
            {
                ssl.Dispose();
            }

            // TCPソケットサーバーがなければ何もしない
            if (_sockServerTcp == null && _sockServerUdp == null)
            {
                return; //すでに終了処理が終わっている
            }

            // キャンセルして、停止
            if (_sockServerTcp != null) _sockServerTcp.Cancel();
            if (_sockServerUdp != null) _sockServerUdp.Cancel();

            if (_sockServerTcp != null) _sockServerTcp.Close();
            if (_sockServerUdp != null) _sockServerUdp.Close();

            // クライアント接続終了まで待機する
            // 全部の子スレッドが終了するのを待つ
            //while (Count() > 0)
            //{
            //    Thread.Sleep(200);
            //}
            _childNone.Wait();

            _sockServerTcp = null;
            _sockServerUdp = null;

        }

        //スレッド開始処理
        //サーバが正常に起動できる場合(isInitSuccess==true)のみスレッド開始できる
        protected abstract bool OnStartServer(); //�X���b�h�J�n����

        protected override bool OnStartThread()
        {
            System.Diagnostics.Trace.TraceInformation($"{this.GetType().FullName}.OnStartThread  ");

            var result = OnStartServer(); //子クラスのスレッド開始処理
            if (!result) return false;

            var port = (int)_conf.Get("port");
            var bindStr = string.Format("{0}:{1} {2}", _oneBind.Addr, port, _oneBind.Protocol);

            //Ver5.9,2 Java fix
            //_sockServer = new SockServer(this.Kernel,_oneBind.Protocol);
            switch (_oneBind.Protocol)
            {
                case ProtocolKind.Tcp:
                    _sockServerTcp = new SockServerTcp(_kernel, _oneBind.Protocol, ssl);
                    _sockServerTcp.SocketStateChanged += _sockServerTcp_SocketStateChanged;
                    if (ssl != null && !ssl.Status)
                    {
                        Logger.Set(LogKind.Error, null, 9000024, bindStr);
                    }
                    break;
                case ProtocolKind.Udp:
                    _sockServerUdp = new SockServerUdp(_kernel, _oneBind.Protocol, ssl);
                    _sockServerUdp.SocketStateChanged += _sockServerUdp_SocketStateChanged;
                    break;
            }

            return true;

        }

        protected override void OnRunThread()
        {
            System.Diagnostics.Trace.TraceInformation($"{this.GetType().FullName}.OnRunThread  ");

            var port = (int)_conf.Get("port");
            var bindStr = string.Format("{0}:{1} {2}", _oneBind.Addr, port, _oneBind.Protocol);

            //DOSを受けた場合、multiple数まで連続アクセスまでは記憶してしまう
            //DOSが終わった後も、その分だけ復帰に時間を要する

            Logger.Set(LogKind.Normal, null, 9000000, bindStr);

            //Ver5.9,2 Java fix
            //_sockServer = new SockServer(this.Kernel,_oneBind.Protocol);
            switch (_oneBind.Protocol)
            {
                case ProtocolKind.Tcp:
                    if (this.SockState != SockState.Error)
                    {
                        RunTcpServer(port);
                    }
                    _sockServerTcp.Close();
                    break;
                case ProtocolKind.Udp:
                    if (this.SockState != SockState.Error)
                    {
                        RunUdpServer(port);
                    }
                    _sockServerUdp.Close();
                    break;
            }

            //Java fix
            Logger.Set(LogKind.Normal, null, 9000001, bindStr);

        }

        private void _sockServerUdp_SocketStateChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.TraceInformation($"{this.GetType().FullName}._sockServerUdp_SocketStateChanged  ");
            ThreadBaseKind = ThreadBaseKind.Running;
            _sockServerUdp.SocketStateChanged -= _sockServerUdp_SocketStateChanged;
        }

        private void _sockServerTcp_SocketStateChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.TraceInformation($"{this.GetType().FullName}._sockServerTcp_SocketStateChanged  ");
            ThreadBaseKind = ThreadBaseKind.Running;
            _sockServerTcp.SocketStateChanged -= _sockServerTcp_SocketStateChanged;
        }

        private void RunTcpServer(int port)
        {
            System.Diagnostics.Trace.TraceInformation($"{this.GetType().FullName}.RunTcpServer  ");

            //[C#]
            //ThreadBaseKind = ThreadBaseKind.Running;

            const int listenMax = 5;

            if (!_sockServerTcp.Bind(_oneBind.Addr, port, listenMax))
            {
                Logger.Set(LogKind.Error, _sockServerTcp, 9000006, _sockServerTcp.GetLastEror());
                return;
            }

            // 生存してる限り実行し続ける
            while (IsLife())
            {
                var child = _sockServerTcp.Select(this);

                // Nullが返されたときは終了する
                if (child == null)
                    break;

                // 同時接続数チェック
                if (Count >= _multiple)
                {
                    // 同時接続数を超えたのでリクエストをキャンセルします
                    System.Diagnostics.Trace.TraceInformation($"OneServer.RunTcpServer over count:{Count}/multiple:{_multiple}");
                    Logger.Set(LogKind.Secure, _sockServerTcp, 9000004, string.Format("count:{0}/multiple:{1}", Count, _multiple));
                    try { child.Close(); }
                    catch { }
                    try { child.Dispose(); }
                    catch { }
                    continue;
                }


                // 子タスクで処理させる
                var t = new Task(
                    () =>
                    {
                        // ACL制限のチェック
                        if (AclCheck(child) == AclKind.Deny)
                        {
                            child.Close();
                            child.Dispose();
                            return;
                        }
                        // 各実装へ
                        this.SubThread(child);
                    }, _cancelToken, TaskCreationOptions.LongRunning);

                this.StartTask(t);

            }

        }

        private void RunUdpServer(int port)
        {
            System.Diagnostics.Trace.TraceInformation($"{this.GetType().FullName}.RunUdpServer  ");

            //[C#]
            //ThreadBaseKind = ThreadBaseKind.Running;

            if (!_sockServerUdp.Bind(_oneBind.Addr, port))
            {
                Logger.Set(LogKind.Error, _sockServerUdp, 9000006, _sockServerUdp.GetLastEror());
                //println(string.Format("bind()=false %s", sockServer.getLastEror()));
                return;
            }

            while (IsLife())
            {
                var child = _sockServerUdp.Select(this);

                //Selectで例外が発生した場合は、そのコネクションを捨てて、次の待ち受けに入る
                if (child == null)
                    continue;

                // 同時接続数チェック
                if (Count >= _multiple)
                {
                    // 同時接続数を超えたのでリクエストをキャンセルします
                    System.Diagnostics.Trace.TraceInformation($"OneServer.RunUdpServer over count:{Count}/multiple:{_multiple}");
                    Logger.Set(LogKind.Secure, _sockServerUdp, 9000004, string.Format("count:{0}/multiple:{1}", Count, _multiple));
                    try { child.Close(); }
                    catch { }
                    try { child.Dispose(); }
                    catch { }
                    continue;
                }

                // 子タスクで処理させる
                var t = new Task(
                    () =>
                    {
                        // ACL制限のチェック
                        if (AclCheck(child) == AclKind.Deny)
                        {
                            child.Close();
                            return;
                        }
                        // 各実装へ
                        this.SubThread(child);
                    }, _cancelToken, TaskCreationOptions.LongRunning);

                this.StartTask(t);
            }

        }
        private void RemoveTask(Task t)
        {
            lock (SyncObj)
            {
                Count -= 1;
            }
        }
        private void StartTask(Task t)
        {
            lock (SyncObj)
            {
                Count += 1;
            }
            t.ContinueWith(this.RemoveTask, TaskContinuationOptions.ExecuteSynchronously);
            t.Start();
        }

        //ACL制限のチェック
        //sockObj 検査対象のソケット
        private AclKind AclCheck(SockObj sockObj)
        {
            var aclKind = AclKind.Allow;
            if (AclList != null)
            {
                var ip = new Ip(sockObj.RemoteAddress.Address.ToString());
                aclKind = AclList.Check(ip);
            }
            return aclKind;
        }

        protected abstract void OnSubThread(SockObj sockObj);

        //１リクエストに対する子スレッドとして起動される
        private void SubThread(SockObj o)
        {
            var sockObj = (SockObj)o;

            System.Threading.Thread.CurrentThread.Name = this.GetType().FullName;

            //クライアントのホスト名を逆引きする
            sockObj.Resolve((bool)_conf.Get("useResolve"), Logger);

            //_subThreadの中でSockObjは破棄する（ただしUDPの場合は、クローンなのでClose()してもsocketは破棄されない）
            Logger.Set(LogKind.Detail, sockObj, 9000002, string.Format("count={0} Local={1} Remote={2}", Count, sockObj.LocalAddress, sockObj.RemoteAddress));

            //Ver5.8.9 Java fix 接続単位のすべての例外をキャッチしてプログラムの停止を避ける
            //OnSubThread(sockObj); //接続単位の処理
            try
            {
                OnSubThread(sockObj); //接続単位の処理
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.Fail(ex.Message);
                System.Diagnostics.Trace.Fail(ex.StackTrace);
                if (Logger != null)
                {
                    Logger.Set(LogKind.Error, null, 9000061, ex.Message);
                    Logger.Exception(ex, null, 2);
                }
            }
            finally
            {
                Logger.Set(LogKind.Detail, sockObj, 9000003, string.Format("count={0} Local={1} Remote={2}", Count, sockObj.LocalAddress, sockObj.RemoteAddress));
                sockObj.Close();
                sockObj.Dispose();
            }

        }

        //Java Fix
        //RemoteServerでのみ使用される
        public abstract void Append(OneLog oneLog);

        //1行読込待機
        public Cmd WaitLine(SockTcp sockTcp)
        {
            var tout = new Utils.Timeout(TimeoutSec);

            while (IsLife())
            {
                Cmd cmd = recvCmd(sockTcp);
                if (cmd == null)
                {
                    return null;
                }
                if (cmd.CmdStr != "")
                {
                    return cmd;
                }
                if (tout.IsFinish())
                {
                    return null;
                }
                Thread.Sleep(100);
            }
            return null;
        }

        //TODO RecvCmdのパラメータ形式を変更するが、これは、後ほど、Web,Ftp,SmtpのServerで使用されているため影響がでる予定
        //コマンド取得
        //コネクション切断などエラーが発生した時はnullが返される
        protected Cmd recvCmd(SockTcp sockTcp)
        {
            if (sockTcp.SockState != SockState.Connect)
            {
                //切断されている
                return null;
            }
            var recvbuf = sockTcp.LineRecv(TimeoutSec, this);
            //切断された場合
            if (recvbuf == null)
            {
                return null;
            }

            //受信待機中の場合
            if (recvbuf.Length == 0)
            {
                //Ver5.8.5 Java fix
                //return new Cmd("", "", "");
                return new Cmd("waiting", "", ""); //待機中の場合、そのことが分かるように"waiting"を返す
            }

            //CRLFの排除
            recvbuf = Inet.TrimCrlf(recvbuf);

            //String str = new String(recvbuf, Charset.forName("Shift-JIS"));
            //var str = Encoding.GetEncoding("Shift-JIS").GetString(recvbuf);
            var str = Encoding.GetEncoding("utf-8").GetString(recvbuf);
            if (str == "")
            {
                return new Cmd("", "", "");
            }
            //受信行をコマンドとパラメータに分解する（コマンドとパラメータは１つ以上のスペースで区切られている）
            String cmdStr = null;
            String paramStr = null;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ' ')
                {
                    if (cmdStr == null)
                    {
                        cmdStr = str.Substring(0, i);
                    }
                }
                if (cmdStr == null || str[i] == ' ')
                {
                    continue;
                }
                paramStr = str.Substring(i);
                break;
            }
            if (cmdStr == null)
            {
                //パラメータ区切りが見つからなかった場合
                cmdStr = str; //全部コマンド
            }
            return new Cmd(str, cmdStr, paramStr);
        }

        //未実装
        //        public void Append(OneLog oneLog){
        //            Util.RuntimeException("OneServer.Append(OneLog) 未実装");
        //        }

        //リモート操作(データの取得)
        public virtual String Cmd(String cmdStr)
        {
            return "";
        }

        /********************************************************/
        //移植のための暫定処置(POP3でのみ使用されている)
        /********************************************************/
        protected bool RecvCmd(SockTcp sockTcp, ref string str, ref string cmdStr, ref string paramStr)
        {

            var cmd = recvCmd(sockTcp);
            if (cmd == null)
            {
                return false;
            }
            cmdStr = cmd.CmdStr;
            paramStr = cmd.ParamStr;
            str = cmd.Str;
            return true;
        }

        public bool WaitLine(SockTcp sockTcp, ref string cmdStr, ref string paramStr)
        {
            var cmd = WaitLine(sockTcp);
            if (cmd == null)
            {
                return false;
            }
            cmdStr = cmd.CmdStr;
            paramStr = cmd.ParamStr;
            return true;
        }

        //Ver6.1.6
        // string GetMsg(int messageNo)の各メッセージがBJD.Lang.txtに定義されているかどうかの確認
        protected abstract void CheckLang();
    }
}

