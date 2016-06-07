﻿using System.Collections.Generic;
using System.Text;
using Bjd;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;
using Bjd.Net.Sockets;
using Bjd.Utils;

namespace Bjd.ProxySmtpServer {

    //POP3プロキシとSMTPプロキシは、ほとんど同じなので、共通部分をこのファイルにまとめ
    //namespaceのみ書き換えて両方のDLLで使用する

    abstract class MailProxyServer:OneServer {

        //通常のServerThreadの子クラスと違い、オプションはリストで受け取る
        //親クラスは、そのリストの0番目のオブジェクトで初期化する

        //コンストラクタ
        protected MailProxyServer(Kernel kernel,Conf conf,OneBind oneBind)
            : base(kernel, conf,oneBind) {

            //特別なユーザのリスト初期化
             _specialUser = new SpecialUser((Dat)_conf.Get("specialUser"));
        }

        readonly SpecialUser _specialUser;//特別なユーザのリスト
        abstract protected string BeforeJob(SockTcp client,List<byte[]> clientBuf);//接続前の処理
        abstract protected string ConnectJob(SockTcp client, SockTcp server, List<byte[]> clientBuf);//接続後の処理
        protected enum MailProxyProtocolKind {
            Unknown = 0,
            Pop3 = 1,
            Smtp = 2
        }
        protected MailProxyProtocolKind Protocol = MailProxyProtocolKind.Unknown;

        override protected bool OnStartServer() { return true; }
        override protected void OnStopServer() { }
        //接続単位の処理
        override protected void OnSubThread(SockObj sockObj) {

            var client = (SockTcp)sockObj;
            SockTcp server = null;
            
            string _targetServer;
            int _targetPort;
            var clientBuf = new List<byte[]>();

            _targetServer = (string)_conf.Get("targetServer");
            _targetPort = (int)_conf.Get("targetPort");
            if(_targetServer == "") {
                Logger.Set(LogKind.Error,client,1,"");
                goto end;
            }
            if(_targetPort == 0) {
                Logger.Set(LogKind.Error,client,2,"");
                goto end;
            }


            //***************************************************************
            //前処理（接続先・ユーザの取得と特別なユーザの置換)
            //***************************************************************
            {
                var keyWord = BeforeJob(client,clientBuf);//接続前の処理
                if(keyWord == null)
                    goto end;


                //特別なユーザにヒットしているかどうかの確認
                OneSpecialUser oneSpecialUser = _specialUser.Search(keyWord);
                if(oneSpecialUser != null) {//ヒットした場合
                    //置換
                    _targetServer = oneSpecialUser.Server;//サーバ
                    _targetPort = oneSpecialUser.Port;//ポート番号

                    for(var i = 0;i < clientBuf.Count;i++) {
                        //string str = Inet.TrimCRLF(Encoding.ASCII.GetString(clientBuf[i]));
                        var str = Encoding.ASCII.GetString(clientBuf[i]);
                        if((Protocol == MailProxyProtocolKind.Smtp && str.ToUpper().IndexOf("MAIL FROM:") == 0) ||
                            (Protocol == MailProxyProtocolKind.Pop3 && str.ToUpper().IndexOf("USER") == 0)) {
                            str = Util.SwapStr(oneSpecialUser.Before,oneSpecialUser.After,str);
                            clientBuf[i] = Encoding.ASCII.GetBytes(str);
                            break;
                        }
                    }
                    Logger.Set(LogKind.Normal,client,3,string.Format("{0}->{1} {2}:{3}",oneSpecialUser.Before,oneSpecialUser.After,_targetServer,_targetPort));
                }
            }

            //***************************************************************
            // サーバとの接続
            //***************************************************************
            {
                var port = _targetPort;
                //var ipList = new List<Ip>{new Ip(_targetServer)};
                //if(ipList[0].ToString() == "0.0.0.0") {
                //    ipList = Kernel.DnsCache.Get(_targetServer);
                //    if(ipList.Count == 0) {
                //        Logger.Set(LogKind.Normal,client,4,string.Format("{0}:{1}",_targetServer,_targetPort));
                //        goto end;
                //    }
                //}
                var ipList = _kernel.GetIpList(_targetServer);
                if (ipList.Count == 0) {
                    Logger.Set(LogKind.Normal, client, 4, string.Format("{0}:{1}", _targetServer, _targetPort));
                    goto end;
                }

                foreach(var ip in ipList) {
                    server = Inet.Connect(_kernel,ip,port,TimeoutSec,null);
                    if(server != null)
                        break;
                }
                if(server == null) {
                    Logger.Set(LogKind.Normal,client,5,string.Format("{0}:{1}",_targetServer,_targetPort));
                    goto end;
                }

                if(null == ConnectJob(client,server,clientBuf))//接続後の処理
                    goto end;

            }
            Logger.Set(LogKind.Normal,client,4,string.Format("connect {0}:{1}",_targetServer,_targetPort));

            //***************************************************************
            //後処理（接続先・ユーザの取得と特別なユーザの置換)
            //***************************************************************

            foreach(byte[] buf in clientBuf) {
                //Ver5.2.5
                //byte[] serverBuf = server.LineRecv(timeout,OPERATE_CRLF.NO,ref life);

                //クライアントからの受信分を送信する
                server.LineSend(buf);
            }

            //***************************************************************
            // パイプ
            //***************************************************************
            var tunnel = new Tunnel(Logger,(int)_conf.Get("idleTime"),TimeoutSec);
            tunnel.Pipe(server,client,this);
        end:
            if(client != null)
                client.Close();
            if(server != null)
                server.Close();
        }

        public override string GetMsg(int messageNo) {
            switch(messageNo) {
                case 1: return _kernel.IsJp ? "接続先サーバが指定されていません" : "Connection ahead server is not appointed";
                case 2: return _kernel.IsJp ? "接続先ポートが指定されていません" : "Connection ahead port is not appointed";
                case 3: return _kernel.IsJp ? "特別なユーザにヒットしました" : "made a hit in a special user";
                case 4: return _kernel.IsJp ? "メールストリームをトンネルしました" : "I do a tunnel of a Mail stream";
            }
            return "unknown";
        }

    }

}

