using System;
using System.Text;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Net.Sockets;
using Xunit;
using Bjd.Initialization;
using Bjd.Threading;

namespace Bjd.Test.Sockets
{

    //テスト用のEchoサーバ
    internal class SockTcpTestEchoServer : ThreadBase
    {
        const int timeout = 20;
        //private readonly SockServer _sockServer;
        private readonly SockServerTcp _sockServer;
        private readonly Ip _ip;
        private readonly int _port;
        private readonly Ssl _ssl = null;

        public SockTcpTestEchoServer(Kernel kernel, Ip ip, int port) : base(kernel, null)
        {
            //_sockServer = new SockServer(new Kernel(),ProtocolKind.Tcp,_ssl);
            _sockServer = new SockServerTcp(kernel, ProtocolKind.Tcp, _ssl);
            _ip = ip;
            _port = port;
        }

        public override String GetMsg(int no)
        {
            return null;
        }

        protected override bool OnStartThread()
        {
            return true;
        }

        protected override void OnStopThread()
        {
            _sockServer.Close();
        }

        protected override void OnRunThread()
        {
            if (_sockServer.Bind(_ip, _port, 1))
            {
                //[C#]
                ThreadBaseKind = ThreadBaseKind.Running;

                while (IsLife())
                {
                    var child = _sockServer.Select(this);
                    if (child == null) break;
                    // 受信開始
                    child.BeginAsync();
                    while (IsLife() && child.SockState == SockState.Connect)
                    {
                        var len = child.Length();
                        if (len > 0)
                        {
                            var buf = child.Recv(len, timeout, this);
                            child.Send(buf);
                        }
                    }
                }
            }
            ThreadBaseKind = ThreadBaseKind.After;
        }
    }
}
