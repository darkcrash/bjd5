using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Globalization;

using Bjd;
using Bjd.Acls;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.WebServer.IO;
using Bjd.WebServer.Handlers;
using Bjd.WebServer.Outside;
using Bjd.WebServer.WebDav;
using Bjd.WebServer.Authority;
using Bjd.WebServer.Memory;
using System.Threading.Tasks;
using Bjd.Memory;

namespace Bjd.WebServer
{
    partial class WebServer : OneServer
    {
        private HandlerSelector _Selector;
        private HttpContentType _contentType;
        private Authorization _authorization;

        //通常は各ポートごと１種類のサーバが起動するのでServerTread.option を使用するが、
        //バーチェルホストの場合、１つのポートで複数のサーバが起動するのでオプションリスト（webOptionList）
        //から適切なものを選択し、opBaseにコピーして使用する
        //_subThreadが呼び出されるまでは、ポート番号の代表である ServerThread.option （webOptionList[0]と同じ ）が使用されている
        //Ver5.1.4
        readonly List<WebDavDb> _webDavDbList = new List<WebDavDb>();
        WebDavDb _webDavDb;//WevDAVのDethプロパテイを管理するクラス

        protected List<ConfigurationBase> WebOptionList = null;

        private CultureInfo _culture = new CultureInfo("en-US");

        private int timeOut;
        private bool useDot;
        private bool useDetailsLog;
        private bool useExpansion;
        private HttpContextPool contextPool;

        //通常のServerThreadの子クラスと違い、オプションはリストで受け取る
        //親クラスは、そのリストの0番目のオブジェクトで初期化する

        protected override bool AsyncMode => true;

        //コンストラクタ
        public WebServer(Kernel kernel, Conf conf, OneBind oneBind)
            : base(kernel, conf, oneBind)
        {

            //同一ポートで待ち受けている仮想サーバのオプションをすべてリストする
            WebOptionList = new List<ConfigurationBase>();
            foreach (var o in kernel.ListOption)
            {
                if (o.NameTag == _conf.NameTag)
                {
                    WebOptionList.Add(o);
                }
                else if (o.NameTag.IndexOf("Web-") == 0)
                {
                    if ((int)o.GetValue("port") == (int)_conf.Get("port"))
                    {
                        WebOptionList.Add(o);
                    }
                }

            }
            //WebDAVリストの初期化
            foreach (var o in WebOptionList)
            {
                if (o.UseServer)
                {
                    _webDavDbList.Add(new WebDavDb(kernel, NameTag));
                }
            }
            _webDavDb = _webDavDbList[0];

            //Ver5.1.2「Cgiパス」「WebDAVパス」「別名」のオプションの修正
            var tagList = new List<string> { "cgiPath", "webDavPath", "aliaseList" };
            foreach (string tag in tagList)
            {
                var dat = (Dat)_conf.Get(tag);
                var changed = false;
                foreach (var o in dat)
                {
                    var str = o.ColumnValueList[0];
                    if (str[0] != '/')
                    {
                        changed = true;
                        str = '/' + str;
                    }
                    if (str.Length > 1 && str[str.Length - 1] != '/')
                    {
                        changed = true;
                        str = str + '/';
                    }
                    o.ColumnValueList[0] = str;
                }
                if (changed)
                    _conf.Set(tag, dat);
            }


            //当初、opBase及びloggerは、weboptionList[0]で暫定的に初期化される 
            var protocol = (int)_conf.Get("protocol");
            if (protocol == 1)
            {//HTTPS
                var op = kernel.ListOption.Get("VirtualHost");
                var privateKeyPassword = (string)op.GetValue("privateKeyPassword");
                var certificate = (string)op.GetValue("certificate");

                //サーバ用SSLの初期化
                ssl = new Ssl(Logger, certificate, privateKeyPassword);
            }

            _Selector = new HandlerSelector(_kernel, _conf, Logger);
            _contentType = new HttpContentType(_conf);
            _authorization = new Authorization(_kernel, _conf, Logger);
            LoadConfig();

            contextPool = HttpContextPool.InitializePool(_kernel, Logger, _conf, _contentType);

        }

        //終了処理
        new public void Dispose()
        {
            foreach (var db in _webDavDbList)
            {
                db.Dispose();
            }
            contextPool.Dispose();
            base.Dispose();
        }

        private void LoadConfig()
        {
            timeOut = (int)_conf.Get("timeOut");
            useDot = (bool)_conf.Get("useDot");
            useDetailsLog = (bool)_conf.Get("useDetailsLog");
            useExpansion = (bool)_conf.Get("useExpansion");
        }

        //スレッド開始処理
        override protected bool OnStartServer()
        {
            return true;
        }
        //スレッド停止処理
        override protected void OnStopServer()
        {

        }

        //接続単位の処理
        override protected void OnSubThread(SockObj sockObj)
        {

        }

        protected override async Task OnSubThreadAsync(SockObj sockObj)
        {
            _kernel.Logger.DebugInformation($"WebServer.OnSubThread ");
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            System.Globalization.CultureInfo.CurrentCulture = _culture;

            // create Connection Context
            using (var connection = contextPool.Get())
            {
                connection.Initialize();
                connection.Connection = (SockTcp)sockObj;
                connection.RemoteIp = connection.Connection.RemoteIp;

                //opBase 及び loggerはバーチャルホストで変更されるので、
                //このポインタを初期化に使用できない
                //接続が継続している間は、このループの中にいる(継続か否かをkeepAliveで保持する)
                //「continue」は、次のリクエストを待つ　「break」は、接続を切断する事を意味する

                var sw = new System.Diagnostics.Stopwatch();

                while (connection.KeepAlive && IsLife())
                {
                    // create Request Context
                    var request = connection.GetRequestContext();
                    var result = await RequestProcessAsync(connection, request);

                    if (!result)
                    {
                        break;
                    }

                }

            }

        }

        private async Task<bool> RequestProcessAsync(HttpConnectionContext connection, HttpContext context)
        {
            //***************************************************************
            //データ取得
            //***************************************************************
            //リクエスト取得
            //ここのタイムアウト値は、大きすぎるとブラウザの切断を取得できないでブロックしてしまう
            using (var requestStr = await connection.Connection.AsciiRecvCharsAsync(TimeoutSec))
            {
                if (requestStr == null || requestStr.DataSize == 0)
                    return false;

                //\r\nの削除
                Inet.TrimCrlf(requestStr);
                //Ver5.8.8 リクエストの解釈に失敗した場合に、処理を中断する
                if (!context.Request.Init(requestStr))
                {
                    return false;
                }
            }

            //ヘッダ取得（内部データは初期化される）
            if (!await context.RequestHeader.RecvAsync(connection.Connection, timeOut, this))
                return false;

            {
                //Ver5.1.x
                var hostStr = context.RequestHeader.Host.ValString;
                context.Url = hostStr == null ? null : string.Format("{0}://{1}", (ssl != null) ? "https" : "http", hostStr);
                _kernel.Logger.DebugInformation($"WebServer.OnSubThread {context.Url}");
            }

            //***************************************************************
            // ドキュメント生成クラスの初期化
            //***************************************************************

            context.Auth = _authorization;
            context.AuthName = "";

            //入力取得（POST及びPUTの場合）
            if (context.RequestHeader.ContentLength.Enabled)
            {
                var contentLengthStr = context.RequestHeader.ContentLength.ValString;
                try
                {
                    //max,lenはともにlong
                    var max = Convert.ToInt64(contentLengthStr);
                    if (max != 0)
                    {//送信データあり
                        context.InputStream = new WebStream((256000 < max) ? -1 : (int)max);
                        var errorCount = 0;
                        while (context.InputStream.Length < max && IsLife())
                        {

                            var len = max - context.InputStream.Length;
                            if (len > 51200000)
                            {
                                len = 51200000;
                            }
                            var b = connection.Connection.Recv((int)len, timeOut, this);
                            if (!context.InputStream.Add(b))
                            {
                                errorCount++;//エラー蓄積
                                Logger.Set(LogKind.Error, null, 41, string.Format("content-Length={0} Recv={1}", max, context.InputStream.Length));
                            }
                            else
                            {
                                errorCount = 0;//初期化
                            }
                            Logger.Set(LogKind.Detail, null, 38, string.Format("Content-Length={0} {1}bytes Received.", max, context.InputStream.Length));
                            if (errorCount > 5)
                            {//５回連続して受信が無かった場合、サーバエラー
                                context.ResponseCode = 500;
                                goto SEND;//サーバエラー
                            }
                            Thread.Sleep(10);
                        }
                        Logger.Set(LogKind.Detail, null, 39, string.Format("Content-Length={0} {1}bytes", max, context.InputStream.Length));
                    }
                }
                catch (Exception ex)
                {
                    _kernel.Logger.TraceError($"WebServer.OnSubThread {ex.Message}");
                    Logger.Set(LogKind.Error, null, 40, ex.Message);
                }
            }

            //***************************************************************
            //バーチャルホストの検索を実施し、opBase、logger及び webDavDb を置き換える
            //***************************************************************
            if (connection.CheckVirtual)
            {//初回のみ
                ReplaceVirtualHost(context.RequestHeader.Host.ValString, connection.Connection.LocalAddress.Address, connection.Connection.LocalAddress.Port);
                connection.CheckVirtual = false;
            }
            //***************************************************************
            //接続を継続するかどうかの判断 keepAliveの初期化
            //***************************************************************
            //if (ssl != null)
            //{
            //    connection.KeepAlive = false;//SSL通信では、１回づつコネクションが必要
            //}
            //else
            //{
            //    if (request.Request.Ver == "HTTP/1.1")
            //    {//HTTP1.1はデフォルトで keepAlive=true
            //        connection.KeepAlive = true;
            //    }
            //    else
            //    { // HTTP/1.1以外の場合、継続接続は、Connection: Keep-Aliveの有無に従う
            //        connection.KeepAlive = request.Header.Connection.ValString == "Keep-Alive";
            //    }
            //}
            if (context.Request.Ver == "HTTP/1.1")
            {//HTTP1.1はデフォルトで keepAlive=true
                connection.KeepAlive = true;
            }
            else
            { // HTTP/1.1以外の場合、継続接続は、Connection: Keep-Aliveの有無に従う
                connection.KeepAlive = context.RequestHeader.Connection.ValString == "Keep-Alive";
            }


            //***************************************************************
            // ログ
            //***************************************************************
            Logger.Set(LogKind.Normal, connection.Connection, ssl != null ? 23 : 24, context.Request.LogStr);

            //***************************************************************
            // 認証
            //***************************************************************
            if (!context.Auth.Check(context.Request.Uri, context.RequestHeader.Authorization.ValString, ref context.AuthName))
            {
                context.ResponseCode = 401;
                connection.KeepAlive = false;//切断
                goto SEND;
            }
            //***************************************************************
            // 不正なURIに対するエラー処理
            //***************************************************************
            //URIを点検して不正な場合はエラーコードを返す
            context.ResponseCode = CheckUri(connection.Connection, context.Request, context.RequestHeader);
            if (context.ResponseCode != 200)
            {
                connection.KeepAlive = false;//切断
                goto SEND;
            }

            //***************************************************************
            //ターゲットオブジェクトの初期化
            //***************************************************************
            if (_Selector.DocumentRoot == null)
            {
                Logger.Set(LogKind.Error, connection.Connection, 14, string.Format("documentRoot={0}", _conf.Get("documentRoot")));//ドキュメントルートで指定されたフォルダが存在しません（処理を継続できません）
                return false;//ドキュメントルートが無効な場合は、処理を継続できない
            }
            var handleSelectorResult = _Selector.InitFromUri(context.Request.Uri);

            //***************************************************************
            // 送信ヘッダの追加
            //***************************************************************
            // 特別拡張 BlackJumboDog経由のリクエストの場合 送信ヘッダにRemoteHostを追加する
            if (useExpansion)
            {
                if (context.RequestHeader.Host.ValString != null)
                {
                    context.ResponseHeader.Append("RemoteHost", connection.Connection.RemoteAddress.Address.ToString());
                }
            }
            //受信ヘッダに「PathInfo:」が設定されている場合、送信ヘッダに「PathTranslated」を追加する
            if (context.RequestHeader.PathInfo.Enabled)
            {
                var pathInfo = context.RequestHeader.PathInfo.ValString;
                pathInfo = _Selector.DocumentRoot + pathInfo;
                //document.AddHeader("PathTranslated", Util.SwapChar('/', '\\', pathInfo));
                context.ResponseHeader.Append("PathTranslated", Util.SwapChar('/', Path.DirectorySeparatorChar, pathInfo));
            }

            //***************************************************************
            //メソッドに応じた処理 OPTIONS 対応 Ver5.1.x
            //***************************************************************
            if (WebDav.WebDav.IsTarget(context.Request.Method))
            {
                var webDav = new WebDav.WebDav(Logger, _webDavDb, handleSelectorResult, context.Response, context.Url, context.RequestHeader.GetVal("Depth"), _contentType, (bool)_conf.Get("useEtag"));

                var inputBuf = new byte[0];
                if (context.InputStream != null)
                {
                    inputBuf = context.InputStream.GetBytes();
                }

                switch (context.Request.Method)
                {
                    case HttpMethod.Options:
                        context.ResponseCode = webDav.Option();
                        break;
                    case HttpMethod.Delete:
                        context.ResponseCode = webDav.Delete();
                        break;
                    case HttpMethod.Put:
                        context.ResponseCode = webDav.Put(inputBuf);
                        break;
                    case HttpMethod.Proppatch:
                        context.ResponseCode = webDav.PropPatch(inputBuf);
                        break;
                    case HttpMethod.Propfind:
                        context.ResponseCode = webDav.PropFind();
                        break;
                    case HttpMethod.Mkcol:
                        context.ResponseCode = webDav.MkCol();
                        break;
                    case HttpMethod.Copy:
                    case HttpMethod.Move:
                        context.ResponseCode = 405;
                        //Destnationで指定されたファイルは書き込み許可されているか？
                        var dstTarget = new HandlerSelector(_kernel, _conf, Logger);
                        if (context.RequestHeader.Destination.Enabled)
                        {
                            string destinationStr = context.RequestHeader.Destination.ValString;
                            if (destinationStr.IndexOf("://") == -1)
                            {
                                destinationStr = context.Url + destinationStr;
                            }
                            var uri = new Uri(destinationStr);
                            var result = dstTarget.InitFromUri(uri.LocalPath);

                            if (result.WebDavKind == WebDavKind.Write)
                            {
                                var overwrite = false;
                                var overwriteStr = context.RequestHeader.GetVal("Overwrite");
                                if (overwriteStr != null)
                                {
                                    if (overwriteStr == "F")
                                    {
                                        overwrite = true;
                                    }
                                }
                                context.ResponseCode = webDav.MoveCopy(result, overwrite, context.Request.Method);
                                //context.ResponseHeader.Append("Location", destinationStr);
                                context.ResponseHeader.SetLocation(destinationStr);
                            }
                        }
                        break;
                }
                //WebDAVに対するリクエストは、ここで処理完了
                goto SEND;

            }

            // handler
            if (!handleSelectorResult.Handler.Request(context, handleSelectorResult))
            {
                return false;
            }

            SEND:
            await SendAsync(context);

            return true;
        }

        //********************************************************
        // Host:ヘッダを見て、バーチャルホストの設定にヒットした場合は
        // オプション等を置き換える
        //********************************************************
        void ReplaceVirtualHost(string host, IPAddress ip, int port)
        {
            _kernel.Logger.DebugInformation($"WebServer.ReplaceVirtualHost ");

            //Ver5.0.0-b12
            if (host == null)
            {
                return;
            }

            //Ver5.0.0-a6 仮想Webの検索をホスト名（アドレス）＋ポート番号に修正
            for (int n = 0; n < 2; n++)
            {
                if (n == 0)
                {//１回目はホスト名で検索する
                    //Ver5.0.0-a6 「ホスト名:ポート番号」の形式で検索する
                    if (host.IndexOf(':') < 0)
                    {
                        host = string.Format("{0}:{1}", host, port);
                    }
                    host = host.ToUpper();//ホスト名は、大文字・小文字を区別しない
                }
                else
                {//２回目はアドレスで検索する
                    host = string.Format("{0}:{1}", ip, port);
                }

                //バーチャルホスト指定の場合オプションを変更する
                foreach (var op in WebOptionList)
                {
                    //先頭のWeb-を削除する
                    string name = op.NameTag.Substring(4).ToUpper();
                    if (name == host)
                    {
                        if (op.NameTag != _conf.NameTag)
                        {
                            //Ver5.1.4 webDavDbを置き換える
                            foreach (var db in _webDavDbList)
                            {
                                if (db.NameTag == op.NameTag)
                                {
                                    _webDavDb = db;
                                }
                            }
                            //オプション及びロガーを再初期化する
                            //OneOption = op;
                            _conf = new Conf(op);
                            LoadConfig();
                            Logger = _kernel.CreateLogger(op.NameTag, useDetailsLog, this);
                        }
                        return;
                    }
                }
            }
        }

        //********************************************************
        //URIを点検して不正な場合はエラーコードを返す
        //return 200 エラーなし
        //********************************************************
        int CheckUri(SockTcp sockTcp, HttpRequest request, HttpRequestHeaders recvHeader)
        {
            _kernel.Logger.DebugInformation($"WebServer.CheckUri ");
            var responseCode = 200;

            // v2.3.1 Uri の１文字目が/で無い場合
            if (request.Uri[0] != '/')
            {
                //responseCode = 400;
                return 400;
            }

            // Uriの最後に空白が入っている場合
            var lastchar = request.Uri[request.Uri.Length - 1];
            if (lastchar == (' ') || lastchar == ('.'))
            {
                //responseCode = 404;
                return 404;
            }

            // ./の含まれるリクエストは404で落とす
            // %20/の含まれるリクエストは404で落とす
            if ((0 <= request.Uri.IndexOf("./")) || (0 <= request.Uri.IndexOf(" /")))
            {
                //responseCode = 404;
                return 404;
            }

            // HTTP1.1でhostヘッダのないものはエラー
            //else if (request.Ver == "HTTP/1.1" && recvHeader.GetVal("Host") == null)
            if (request.Ver == "HTTP/1.1" && recvHeader.Host.ValString == null)
            {
                //responseCode = 400;
                return 400;
            }

            // ..を参照するパスの排除
            if (!useDot && 0 <= request.Uri.IndexOf(".."))
            {
                Logger.Set(LogKind.Secure, sockTcp, 13, "URI=" + request.Uri);//.. が含まれるリクエストは許可されていません。
                //responseCode = 403;
                return 403;
            }

            return responseCode;
        }

        private async Task SendAsync(HttpContext contextRequest)
        {
            var contextConnection = contextRequest.Connection;
            var response = contextRequest.Response;
            var responseHeader = contextRequest.ResponseHeader;

            _kernel.Logger.DebugInformation($"WebServer.OnSubThread SEND");
            //レスポンスコードが200以外の場合は、ドキュメント（及び送信ヘッダ）をエラー用に変更する
            if (contextRequest.ResponseCode != 200 && contextRequest.ResponseCode != 302 && contextRequest.ResponseCode != 206 && contextRequest.ResponseCode != 207 && contextRequest.ResponseCode != 204 && contextRequest.ResponseCode != 201)
            {
                //ResponceCodeの応じてエラードキュメントを生成する
                if (!response.CreateFromErrorCode(contextRequest.Request, contextRequest.ResponseCode))
                    return;

                if (contextRequest.ResponseCode == 301)
                {//ターゲットがファイルではなくディレクトの間違いの場合
                    if (contextRequest.Url != null)
                    {
                        var str = string.Format("{0}{1}/", contextRequest.Url, contextRequest.Request.Uri);
                        //response.AddHeader("Location", Encoding.UTF8.GetBytes(str));
                        responseHeader.SetLocation(str.ToUtf8BufferData());
                    }
                }

                if (contextRequest.ResponseCode == 304 || contextRequest.ResponseCode == 301)
                {//304 or 301 の場合は、ヘッダのみになる
                    response.Clear();
                }
                else
                {
                    if (contextRequest.ResponseCode == 401)
                    {
                        responseHeader.Append("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", contextRequest.AuthName));
                    }
                }
            }

            //Ver5.6.2 request.Send()廃止
            //Logger.Set(LogKind.Detail, contextConnection.Connection, 4, responseStr);//ログ

            using (var responseChars = contextRequest.Request.CreateResponseChars(contextRequest.ResponseCode))
            {
                Logger.Set(LogKind.Detail, contextConnection.Connection, 4, responseChars);//ログ
                await contextConnection.Connection.AsciiLineSendAsync(responseChars);//レスポンス送信
            }


            await response.SendAsync(contextConnection.KeepAlive, this);//ドキュメント本体送信
        }

        //テスト用
        public String DocumentRoot
        {
            get
            {
                return (string)_conf.Get("documentRoot");
            }
        }

        //RemoteServerでのみ使用される
        public override void Append(LogMessage oneLog)
        {

        }

    }
}
