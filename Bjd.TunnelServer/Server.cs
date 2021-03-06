﻿using System.Collections.Generic;
using System.Net;
using Bjd;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using Bjd.Net.Sockets;
using Bjd.Utils;

namespace Bjd.TunnelServer
{
    partial class Server : OneServer
    {

        //通常のServerThreadの子クラスと違い、オプションはリストで受け取る
        //親クラスは、そのリストの0番目のオブジェクトで初期化する
        readonly string _targetServer;
        readonly int _targetPort;
        readonly ProtocolKind _protocolKind;


        //コンストラクタ
        public Server(Kernel kernel, Conf conf, OneBind oneBind)
            : base(kernel, conf, oneBind)
        {

            _targetServer = (string)_conf.Get("targetServer");
            _targetPort = (int)_conf.Get("targetPort");

            if (_targetServer == "")
            {
                Logger.Set(LogKind.Error, null, 1, "");
            }
            if (_targetPort == 0)
            {
                Logger.Set(LogKind.Error, null, 2, "");
            }

            _protocolKind = oneBind.Protocol;
        }

        override protected bool OnStartServer() { return true; }
        override protected void OnStopServer() { }

        //接続単位の処理
        override protected void OnSubThread(ISocket sockObj)
        {

            if (_protocolKind == ProtocolKind.Tcp)
            {
                TcpTunnel((ISocket)sockObj);
            }
            else {
                UdpTunnel((ISocket)sockObj);
            }

        }
        void TcpTunnel(ISocket tcpObj)
        {

            var client = tcpObj;
            ISocket server = null;

            //***************************************************************
            // サーバとの接続
            //***************************************************************
            {
                var port = _targetPort;

                //var ipList = new List<Ip>{new Ip(_targetServer)};
                //if (ipList[0].ToString() == "0.0.0.0") {
                //    ipList = Kernel.DnsCache.Get(_targetServer);
                //    if(ipList.Count==0){
                //        Logger.Set(LogKind.Normal,null,4,string.Format("{0}:{1}",_targetServer,_targetPort));
                //        goto end;
                //    }
                //}
                var ipList = _kernel.GetIpList(_targetServer);
                if (ipList.Count == 0)
                {
                    Logger.Set(LogKind.Normal, null, 4, string.Format("{0}:{1}", _targetServer, _targetPort));
                    goto end;
                }
                foreach (var ip in ipList)
                {
                    server = Inet.Connect(_kernel, ip, port, TimeoutSec, null);
                    if (server != null)
                        break;
                }
                if (server == null)
                {
                    Logger.Set(LogKind.Normal, server, 5, string.Format("{0}:{1}", _targetServer, _targetPort));
                    goto end;
                }
            }
            Logger.Set(LogKind.Normal, server, 6, string.Format("TCP {0}:{1} - {2}:{3}", client.RemoteHostname, client.RemoteAddress.Port, _targetServer, _targetPort));

            //***************************************************************
            // パイプ
            //***************************************************************
            var tunnel = new Tunnel(Logger, (int)_conf.Get("idleTime"), TimeoutSec);
            tunnel.Pipe(server, client, this);
            end:
            if (client != null)
                client.Close();
            if (server != null)
                server.Close();
        }
        void UdpTunnel(ISocket udpObj)
        {
            //int timeout = this.OpBase.ValInt("timeOut");
            var sock = new Dictionary<CS, ISocket>(2);
            sock[CS.Client] = udpObj;
            sock[CS.Server] = null;

            //***************************************************************
            // サーバとの接続
            //***************************************************************
            {
                int port = _targetPort;
                //var ip = new Ip(_targetServer);
                //if (ip.ToString() == "0.0.0.0") {
                //    try {
                //        var iphe = Dns.GetHostEntry(_targetServer);
                //        if (iphe.AddressList.Length == 0) {
                //            goto end;
                //        }
                //        ip = new Ip(iphe.AddressList[0].ToString());
                //    } catch {//名前に失敗した場合
                //        Logger.Set(LogKind.Normal,null, 4, string.Format("{0}:{1}", _targetServer, _targetPort));
                //        goto end;
                //    }
                //}
                Ip ip;
                try
                {
                    ip = new Ip(_targetServer);
                }
                catch (ValidObjException)
                {
                    try
                    {
                        var ipheWait = Dns.GetHostEntryAsync(_targetServer);
                        ipheWait.Wait();
                        var iphe = ipheWait.Result;
                        if (iphe.AddressList.Length == 0)
                        {
                            goto end;
                        }
                        ip = new Ip(iphe.AddressList[0].ToString());
                    }
                    catch
                    {//名前に失敗した場合
                        Logger.Set(LogKind.Normal, null, 4, string.Format("{0}:{1}", _targetServer, _targetPort));
                        goto end;
                    }
                }


                sock[CS.Server] = new SockUdp(_kernel, ip, port, null, new byte[0]);
                if (sock[CS.Server].SockState == SockState.Error)
                    goto end;
            }
            sock[CS.Server].Send(sock[CS.Client].RecvBuf);//サーバへ送信
            //if (sock[CS.Server].Recv(Timeout)) {//サーバからの受信
            var buf = sock[CS.Server].Recv(TimeoutSec);
            if (buf.Length == 0)
            {
                sock[CS.Client].Send(buf);//クライアントへ送信
            }
            Logger.Set(LogKind.Normal, sock[CS.Server], 7, string.Format("UDP {0}:{1} - {2}:{3} {4}byte", sock[CS.Client].RemoteHostname, sock[CS.Client].RemoteAddress.Port, _targetServer, _targetPort, buf.Length));

            end:
            //udpObj.Close();UDPソケット(udpObj)はクローンなのでクローズしても、処理されない※Close()を呼び出しても問題はない
            if (sock[CS.Client] != null)
                sock[CS.Client].Close();
            if (sock[CS.Server] != null)
                sock[CS.Server].Close();

        }

        //RemoteServerでのみ使用される
        public override void Append(LogMessage oneLog)
        {

        }

    }
}


