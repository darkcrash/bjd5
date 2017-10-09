using Bjd.Net.Sockets;
using System;

namespace Bjd.FtpServer
{

    //セッションごとの情報
    public class Session
    {
        public string UserName { get; set; }
        public string RnfrName { get; set; }
        public int Port { get; set; }
        public CurrentDir CurrentDir { get; set; }
        public OneUser OneUser { get; set; }
        public FtpType FtpType { get; set; }
        public ISocket SockData { get; set; }
        public ISocket SockCtrl { get; private set; }

        public Session(ISocket sockCtrl)
        {
            SockCtrl = sockCtrl;

            ////PASV接続用ポート番号の初期化 (開始番号は2000～9900)
            //PASV接続用ポート番号の初期化 (開始番号は2010～2050)
            var rnd = new Random();
            Port = (rnd.Next(10) + 2010) ;

        }

        //１行送信
        public void StringSend(string str)
        {
            //Ver5.9.7
            //SockCtrl.StringSend(str,"ascii");
            //SockCtrl.StringSend(str, "shift-jis");
            SockCtrl.StringSend(str, "UTF-8");
        }
    }
}