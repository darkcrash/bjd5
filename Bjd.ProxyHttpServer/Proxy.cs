using System;
using System.Collections.Generic;
using System.Linq;
using Bjd;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Threading;

namespace Bjd.ProxyHttpServer
{
    class Proxy : IDisposable
    {
        readonly Kernel _kernel;
        public Logger Logger { get; private set; }
        public int OptionTimeout { get; private set; }

        //ソケット
        ISocket _sockClient;
        ISocket _sockServer;
     
        //上位プロキシ情報
        public UpperProxy UpperProxy { get; private set; }
     
        //接続中のサーバ情報(URLから取得したホスト名)
        public string HostName = "";
        public int Port = 0;//Ver5.0.1

        public Proxy(Kernel kernel, Logger logger, ISocket clientSocket, int optionTimeout, UpperProxy upperProxy)
        {
            _kernel = kernel;
            Logger = logger;
            OptionTimeout = optionTimeout;
            UpperProxy = upperProxy;

            _sockClient = clientSocket;
            _sockServer = null;
            //sock[CS.CLIENT].SendTimeout = optionTimeout; //Ver5.0.2 送信タイムアウトは設定しない

            ProxyProtocol = ProxyProtocol.Unknown;
        }
        // 終了処理
        public void Dispose()
        {
            _sockClient?.Dispose();
            _sockClient = null;

            _sockServer?.Dispose();
            _sockServer = null;

            ////Ver5.0.0-b3 使用終了を明示的に記述する
            //foreach (CS cs in Enum.GetValues(typeof(CS)))
            //{
            //    _sock[cs] = null;
            //}
            //_sock = null;
        }

        public ProxyProtocol ProxyProtocol { get; private set; }

        public ISocket Sock(CS cs)
        {
            if (cs == CS.Client) return _sockClient;
            if (cs == CS.Server) return _sockServer;
            throw new ArgumentException();
        }

        public SockState SockState(CS cs)
        {
            if (cs == CS.Client) return _sockClient.SockState;
            if (cs == CS.Server) return _sockServer?.SockState ??  Net.Sockets.SockState.Idle;
            throw new ArgumentException();
        }

        //ソケットに到着しているデータ量
        public int Length(CS cs)
        {
            if (cs == CS.Client) return _sockClient.Length();
            if (cs == CS.Server) return _sockServer?.Length() ?? 0;
            throw new ArgumentException();
        }

        public void NoConnect(string host, int port)
        {
            //キャッシュにヒットした場合に、サーバ側のダミーソケットを作成する
            //_sockServer = new SockTcp(_kernel, new Ip(IpKind.V4_0), 0, 0, null);

            //Request.HostNameを保存して、現在接続中のホストを記憶する
            HostName = host;
            Port = port;
        }

        public bool Connect(ILife iLife, string host1, int port1, string requestStr, ProxyProtocol proxyProtocol)
        {

            ProxyProtocol = proxyProtocol;

            if (_sockServer != null)
            {
                //Ver5.0.1
                //if(_host == HostName) {
                if (host1 == HostName && port1 == Port)
                {
                    return true;
                }
                //Ver5.0.0-b21
                //_sockServer.Close();
                _sockServer.Dispose();
                _sockServer = null;
            }

            if (UpperProxy.Use)
            {
                // 上位プロキシのチェック
                // 上位プロキシを経由しないサーバの確認
                foreach (string address in UpperProxy.DisableAdderssList)
                {
                    if (ProxyProtocol == ProxyProtocol.Ssl)
                    {
                        if (host1.IndexOf(address) == 0)
                        {
                            UpperProxy.Use = false;
                            break;
                        }
                    }
                    else
                    {
                        string str = requestStr.Substring(11);
                        if (str.IndexOf(address) == 0)
                        {
                            UpperProxy.Use = false;
                            break;
                        }
                    }

                }
            }

            string host = host1;
            int port = port1;
            if (UpperProxy.Use)
            {
                host = UpperProxy.Server;
                port = UpperProxy.Port;
            }

            List<Ip> ipList = null;
            try
            {
                ipList = new List<Ip>();
                ipList.Add(new Ip(host));
            }
            catch (ValidObjException)
            {
                ipList = _kernel.DnsCache.GetAddress(host).ToList();
                if (ipList == null || ipList.Count == 0)
                {
                    Logger.Set(LogKind.Error, null, 11, host);
                    return false;
                }
            }

            Ssl ssl = null;
            foreach (Ip ip in ipList)
            {
                int timeout = 3;
                _sockServer = Inet.Connect(_kernel, ip, port, timeout, ssl);
                if (_sockServer != null) break;
            }
            if (_sockServer == null)
            {
                Logger.Set(LogKind.Detail, _sockClient, 26, string.Format("{0}:{1}", ipList[0], port));
                return false;
            }
            //sock[CS.SERVER].SendTimeout = OptionTimeout;//Ver5.0.2 送信タイムアウトは設定しない

            HostName = host1;//Request.HostNameを保存して、現在接続中のホストを記憶する
            //Ver5.6.1
            Port = port1;
            return true;

        }

    }
}
