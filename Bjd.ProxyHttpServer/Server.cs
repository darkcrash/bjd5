﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Bjd;
using System.Text.RegularExpressions;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using Bjd.Net.Sockets;
using System.Threading.Tasks;

namespace Bjd.ProxyHttpServer
{

    public partial class Server : OneServer
    {
        //enum Charset {
        //    Unknown=0,
        //    Ascii=1,
        //    Sjis=2,
        //    Euc=3,
        //    Utf8=4,
        //    Utf7=5,
        //    Jis=6//iso-2022-jp
        //}

        private const int DataPortMin = 20000;
        private const int DataPortMax = 21000;
        int _dataPort;

        Cache _cache;
        // 上位プロキシを経由しないサーバのリスト
        readonly List<string> _disableAddressList = new List<string>();

        readonly LimitUrl _limitUrl;//URL制限
        readonly LimitString _limitString;//コンテンツ制限

        //リクエストを通常ログで表示する
        readonly bool _useRequestLog;

        readonly bool useUpperProxy;
        readonly string upperProxyServer;
        readonly int upperProxyPort;
        readonly bool upperProxyUseAuth;
        readonly string upperProxyAuthName;
        readonly string upperProxyAuthPass;

        protected override bool AsyncMode => true;


        public Server(Kernel kernel, Conf conf, OneBind oneBind)
            : base(kernel, conf, oneBind)
        {

            _cache = new Cache(kernel, this.Logger, conf);

            // 上位プロキシを経由しないサーバのリスト
            foreach (var o in (Dat)_conf.Get("disableAddress"))
            {
                if (o.Enable)
                {//有効なデータだけを対象にする
                    _disableAddressList.Add(o.ColumnValueList[0]);
                }
            }
            //URL制限
            var allow = (Dat)_conf.Get("limitUrlAllow");
            var deny = (Dat)_conf.Get("limitUrlDeny");
            //Ver5.4.5正規表現の誤りをチェックする
            for (var i = 0; i < 2; i++)
            {
                foreach (var a in (i == 0) ? allow : deny)
                {
                    if (a.Enable && a.ColumnValueList[1] == "3")
                    {//正規表現
                        try
                        {
                            var regex = new Regex(a.ColumnValueList[0]);
                        }
                        catch
                        {
                            Logger.Set(LogKind.Error, null, 28, a.ColumnValueList[0]);
                        }
                    }
                }
            }
            _limitUrl = new LimitUrl(allow, deny);


            //リクエストを通常ログで表示する
            _useRequestLog = (bool)_conf.Get("useRequestLog");

            //コンテンツ制限
            _limitString = new LimitString((Dat)_conf.Get("limitString"));
            if (_limitString.Length == 0)
                _limitString = null;

            _dataPort = DataPortMin;


            useUpperProxy = (bool)_conf.Get("useUpperProxy");
            upperProxyServer = (string)_conf.Get("upperProxyServer");
            upperProxyPort = (int)_conf.Get("upperProxyPort");
            upperProxyUseAuth = (bool)_conf.Get("upperProxyUseAuth");
            upperProxyAuthName = (string)_conf.Get("upperProxyAuthName");
            upperProxyAuthPass = (string)_conf.Get("upperProxyAuthPass");
        }

        //リモート操作（データの取得）
        override public string Cmd(string cmdStr)
        {

            if (cmdStr == "Refresh-DiskCache" || cmdStr == "Refresh-MemoryCache")
            {
                var infoList = new List<CacheInfo>();
                _cache.GetInfo((cmdStr == "Refresh-MemoryCache") ? CacheKind.Memory : CacheKind.Disk, ref infoList);
                var sb = new StringBuilder();
                foreach (CacheInfo cacheInfo in infoList)
                {
                    sb.Append(cacheInfo + "\b");
                }
                return sb.ToString();
            }
            if (cmdStr.IndexOf("Cmd-Remove") == 0)
            {

                var tmp = cmdStr.Split('\t');

                if (tmp.Length != 5)
                    return "false";
                var kind = (CacheKind)Enum.Parse(typeof(CacheKind), tmp[1]);
                var hostName = tmp[2];
                var port = Convert.ToInt32(tmp[3]);
                var uri = tmp[4];
                if (_cache.Remove(kind, hostName, port, uri))
                    return "true";
                return "false";
            }
            return "";
        }

        new public void Dispose()
        {
            _cache?.Dispose();
            _cache = null;

            base.Dispose();
        }
        override protected bool OnStartServer()
        {
            _cache?.Start();
            return true;
        }
        override protected void OnStopServer()
        {
            if (_cache != null)
            {
                _cache.Stop();
                _cache.Dispose();
            }
        }
        //接続単位の処理
        override protected void OnSubThread(ISocket sockObj)
        {

            //    //Ver5.6.9
            //    //UpperProxy upperProxy = new UpperProxy((bool)Conf.Get("useUpperProxy"),(string)this.Conf.Get("upperProxyServer"),(int)this.Conf.Get("upperProxyPort"),disableAddressList);
            //    var upperProxy = new UpperProxy((bool)_conf.Get("useUpperProxy"), (string)_conf.Get("upperProxyServer"), (int)_conf.Get("upperProxyPort"), _disableAddressList,
            //        (bool)_conf.Get("upperProxyUseAuth"),
            //        (string)_conf.Get("upperProxyAuthName"),
            //        (string)_conf.Get("upperProxyAuthPass"));
            //    var proxy = new Proxy(_kernel, Logger, (SockTcp)sockObj, TimeoutSec, upperProxy);//プロキシ接続情報
            //    ProxyObj proxyObj = null;
            //    OneObj oneObj = null;

            //    //最初のリクエスト取得
            //    for (int i = 0; IsLife() && proxy.Length(CS.Client) == 0; i++)
            //    {
            //        //まだサーバと未接続の段階では、クライアントからのリクエストがない場合、
            //        //このスレッドはエラーとなる
            //        Thread.Sleep(50);
            //        if (i > 100)
            //            goto end;//切断
            //    }
            //    //新たなHTTPオブジェクトを生成する
            //    oneObj = new OneObj(proxy);

            //    //リクエスト行・ヘッダ・POSTデータの読み込み・URL制限
            //    if (!oneObj.RecvRequest(_useRequestLog, _limitUrl, this))
            //        goto end;

            //    //HTTPの場合
            //    if (oneObj.Request.Protocol == ProxyProtocol.Http)
            //    {

            //        proxyObj = new ProxyHttp(proxy, _kernel, _conf, _cache, _limitString);//HTTPデータ管理オブジェクト

            //        //最初のオブジェクトの追加
            //        proxyObj.Add(oneObj);

            //        while (IsLife())
            //        {//デフォルトで継続型

            //            //*******************************************************
            //            //プロキシ処理
            //            //*******************************************************
            //            if (!proxyObj.Pipe(this))
            //                goto end;

            //            if (!((ProxyHttp)proxyObj).KeepAlive)
            //            {
            //                if (proxyObj.IsFinish())
            //                {
            //                    Logger.Set(LogKind.Debug, null, 999, "break keepAlive=false");
            //                    break;
            //                }
            //            }

            //            //*******************************************************
            //            //次のリクエストを取得
            //            //*******************************************************
            //            //if(((ProxyHttp)proxyObj).KeepAlive) {
            //            for (var i = 0; i < 30; i++)
            //            {
            //                if (proxy.Length(CS.Client) != 0)
            //                {

            //                    //Ver5.9.0
            //                    if (oneObj != null)
            //                    {
            //                        oneObj.Dispose();
            //                    }
            //                    //新たなHTTPオブジェクトを生成する
            //                    oneObj = new OneObj(proxy);

            //                    //リクエスト行・ヘッダ・POSTデータの読み込み・URL制限
            //                    if (!oneObj.RecvRequest(_useRequestLog, _limitUrl, this))
            //                        goto end;

            //                    if (oneObj.Request.Protocol != ProxyProtocol.Http)
            //                    {
            //                        goto end;//Ver5.0.2
            //                    }
            //                    //HTTPオブジェクトの追加
            //                    proxyObj.Add(oneObj);

            //                }
            //                else
            //                {
            //                    if (!proxyObj.IsFinish())
            //                        break;

            //                    //Ver5.6.1 最適化
            //                    if (!proxyObj.WaitProcessing())
            //                    {
            //                        Thread.Sleep(5);
            //                    }
            //                }
            //            }
            //            //}
            //            //デバッグログ
            //            //proxyObj.DebugLog();

            //            if (proxyObj.IsTimeout())
            //            {
            //                Logger.Set(LogKind.Debug, null, 999, string.Format("break waitTime>{0}sec [Option Timeout]", proxy.OptionTimeout));
            //                break;
            //            }
            //            //Ver5.1.4-b1
            //            //Thread.Sleep(500);

            //            Thread.Sleep(1);//Ver5.6.1これを0にするとCPU使用率が100%になってしまう
            //        }
            //    }
            //    else if (oneObj.Request.Protocol == ProxyProtocol.Ssl)
            //    {

            //        proxyObj = new ProxySsl(proxy);//SSLデータ管理オブジェクト

            //        //オブジェクトの追加
            //        proxyObj.Add(oneObj);

            //        while (IsLife())
            //        {//デフォルトで継続型

            //            //*******************************************************
            //            //プロキシ処理
            //            //*******************************************************
            //            if (!proxyObj.Pipe(this))
            //                goto end;

            //            //デバッグログ
            //            //proxyObj.DebugLog();

            //            if (proxyObj.IsTimeout())
            //            {
            //                Logger.Set(LogKind.Debug, null, 999, string.Format("break waitTime>{0}sec [Option Timeout]", proxy.OptionTimeout));
            //                break;
            //            }
            //            //Ver5.0.0-b13
            //            //Thread.Sleep(500);
            //            Thread.Sleep(1);
            //        }
            //    }
            //    else if (oneObj.Request.Protocol == ProxyProtocol.Ftp)
            //    {
            //        proxyObj = new ProxyFtp(proxy, _kernel, _conf, this, ++_dataPort);//FTPデータ管理オブジェクト

            //        //オブジェクトの追加
            //        proxyObj.Add(oneObj);

            //        //*******************************************************
            //        //プロキシ処理
            //        //*******************************************************
            //        proxyObj.Pipe(this);

            //        _dataPort = ((ProxyFtp)proxyObj).DataPort;
            //        if (_dataPort > DataPortMax)
            //            _dataPort = DataPortMin;

            //    }
            //end:
            //    //Ver5.9.0
            //    if (oneObj != null)
            //    {
            //        oneObj.Dispose();
            //    }


            //    //終了処理
            //    if (proxyObj != null)
            //        proxyObj.Dispose();
            //    proxy.Dispose();

            //    //Java fix Ver5.9.0
            //    //GC.Collect();
        }

        protected override async Task OnSubThreadAsync(ISocket sockObj)
        {

            //Ver5.6.9
            //UpperProxy upperProxy = new UpperProxy((bool)Conf.Get("useUpperProxy"),(string)this.Conf.Get("upperProxyServer"),(int)this.Conf.Get("upperProxyPort"),disableAddressList);
            var upperProxy = new UpperProxy(useUpperProxy, upperProxyServer, upperProxyPort, _disableAddressList,
                upperProxyUseAuth,
                upperProxyAuthName,
                upperProxyAuthPass);
            var proxy = new Proxy(_kernel, Logger, sockObj, TimeoutSec, upperProxy);//プロキシ接続情報

            ProxyObj proxyObj = null;
            ProxyHttp proxyHttpObj = null;
            ProxySsl proxySslObj = null;
            OneObj oneObj = null;
            try
            {

                // デフォルトで継続型
                while (IsLife())
                {

                    // 新たなHTTPオブジェクトを生成する
                    oneObj?.Dispose();
                    oneObj = new OneObj(proxy);

                    // リクエスト行・ヘッダ・POSTデータの読み込み・URL制限
                    if (!await oneObj.RecvRequestAsync(_useRequestLog, _limitUrl, this))
                        break;

                    // HTTPの場合
                    if (oneObj.Request.Protocol == ProxyProtocol.Http)
                    {
                        // HTTPデータ管理オブジェクト
                        if (proxyHttpObj == null) proxyHttpObj = new ProxyHttp(proxy, _kernel, _conf, _cache, _limitString);

                        // 最初のオブジェクトの追加
                        proxyHttpObj.Add(oneObj);

                        // プロキシ処理
                        if (!await proxyHttpObj.PipeAsync(this))
                            break;

                        if (!proxyHttpObj.KeepAlive)
                        {
                            //if (proxyHttpObj.IsFinish())
                            //{
                            //    Logger.Set(LogKind.Debug, null, 999, "break keepAlive=false");
                            //    break;
                            //}
                            Logger.Set(LogKind.Debug, null, 999, "break keepAlive=false");
                            break;
                        }

                        //if (proxyHttpObj.IsTimeout())
                        //{
                        //    Logger.Set(LogKind.Debug, null, 999, string.Format("break waitTime>{0}sec [Option Timeout]", proxy.OptionTimeout));
                        //    break;
                        //}

                    }
                    else if (oneObj.Request.Protocol == ProxyProtocol.Ssl)
                    {

                        if (proxySslObj == null) proxySslObj = new ProxySsl(proxy);//SSLデータ管理オブジェクト

                        //オブジェクトの追加
                        proxySslObj.Add(oneObj);

                        while (IsLife())
                        {//デフォルトで継続型

                            //*******************************************************
                            //プロキシ処理
                            //*******************************************************
                            if (!await proxySslObj.PipeAsync(this)) return;

                            if (proxySslObj.IsTimeout())
                            {
                                Logger.Set(LogKind.Debug, null, 999, string.Format("break waitTime>{0}sec [Option Timeout]", proxy.OptionTimeout));
                                return;
                            }
                            //Ver5.0.0-b13
                            //Thread.Sleep(500);
                            Thread.Sleep(1);
                        }

                    }
                    else if (oneObj.Request.Protocol == ProxyProtocol.Ftp)
                    {
                        if (proxyObj == null) proxyObj = new ProxyFtp(proxy, _kernel, _conf, this, ++_dataPort);//FTPデータ管理オブジェクト

                        //オブジェクトの追加
                        proxyObj.Add(oneObj);

                        //*******************************************************
                        //プロキシ処理
                        //*******************************************************
                        await proxyObj.PipeAsync(this);

                        _dataPort = ((ProxyFtp)proxyObj).DataPort;
                        if (_dataPort > DataPortMax) _dataPort = DataPortMin;

                        break;
                    }

                }

            }
            finally
            {
                //終了処理
                oneObj?.Dispose();
                proxyObj?.Dispose();
                proxyHttpObj?.Dispose();
                proxySslObj?.Dispose();
                proxy?.Dispose();
            }

        }

        //RemoteServerでのみ使用される
        public override void Append(LogMessage oneLog)
        {

        }
    }
}

