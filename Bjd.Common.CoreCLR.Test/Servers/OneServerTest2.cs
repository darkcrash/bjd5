﻿using System.Threading;
using Bjd;
using Bjd.Controls;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;
using Bjd.Sockets;
using Xunit;
using Bjd.Test.Services;

namespace Bjd.Test.Servers
{

    public class OneServerTest2 : ILife
    {
        public OneServerTest2()
        {
            TestService.ServiceTest();
        }

        private class EchoServer : OneServer
        {
            private readonly ProtocolKind _protocolKind;

            public EchoServer(Conf conf, OneBind oneBind) : base(new Kernel(), conf, oneBind)
            {
                _protocolKind = oneBind.Protocol;
            }

            public override string GetMsg(int no)
            {
                return null;
            }

            protected override void OnStopServer()
            {
            }

            protected override bool OnStartServer()
            {
                return true;
            }

            protected override void OnSubThread(SockObj sockObj)
            {
                if (_protocolKind == ProtocolKind.Tcp)
                {
                    Tcp((SockTcp)sockObj);
                }
                else
                {
                    Udp((SockUdp)sockObj);
                }
            }

            private void Tcp(SockTcp sockTcp)
            {
                while (IsLife() && sockTcp.SockState == SockState.Connect)
                {
                    Thread.Sleep(0); //これが無いと、別スレッドでlifeをfalseにできない
                    var len = sockTcp.Length();
                    if (0 < len)
                    {
                        const int timeout = 10;
                        var buf = sockTcp.Recv(len, timeout, this);
                        sockTcp.Send(buf);
                        break; //echoしたらセッションを閉じる
                    }
                }
            }

            private void Udp(SockUdp sockUdp)
            {
                var buf = sockUdp.RecvBuf;
                sockUdp.Send(buf);
                //echoしたらセッションを閉じる
            }

            //RemoteServerでのみ使用される
            public override void Append(OneLog oneLog)
            {

            }

            protected override void CheckLang() { }
        }

        [Fact]
        public void OneServerを継承したEchoServer_TCP版_を使用して接続する()
        {

            const string addr = "127.0.0.1";
            const int port = 9999;
            const int timeout = 300;
            Ip ip = null;
            try
            {
                ip = new Ip(addr);
            }
            catch (ValidObjException ex)
            {
                Assert.False(true, ex.Message);
            }
            var oneBind = new OneBind(ip, ProtocolKind.Tcp);
            var conf = TestUtil.CreateConf("OptionSample");
            conf.Set("port", port);
            conf.Set("multiple", 10);
            conf.Set("acl", new Dat(new CtrlType[0]));
            conf.Set("enableAcl", 1);
            conf.Set("timeOut", timeout);

            var echoServer = new EchoServer(conf, oneBind);
            echoServer.Start();

            //TCPクライアント

            const int max = 10000;
            var buf = new byte[max];
            buf[8] = 100; //CheckData
            for (int i = 0; i < 3; i++)
            {
                var sockTcp = new SockTcp(new Kernel(), ip, port, timeout, null);

                sockTcp.Send(buf);

                while (sockTcp.Length() == 0)
                {
                    Thread.Sleep(2);
                }

                var len = sockTcp.Length();
                if (0 < len)
                {
                    var b = sockTcp.Recv(len, timeout, this);
                    Assert.Equal(b[8], buf[8]);//CheckData
                }
                Assert.Equal(max, len);

                sockTcp.Close();

            }

            echoServer.Dispose();

        }

        [Fact]
        public void OneServerを継承したEchoServer_UDP版_を使用して接続する()
        {

            const string addr = "127.0.0.1";
            const int port = 9991;
            const int timeout = 5;
            Ip ip = null;
            try
            {
                ip = new Ip(addr);
            }
            catch (ValidObjException ex)
            {
                Assert.False(true, ex.Message);
            }
            var oneBind = new OneBind(ip, ProtocolKind.Udp);
            var conf = TestUtil.CreateConf("OptionSample");
            conf.Set("port", port);
            conf.Set("multiple", 10);
            conf.Set("acl", new Dat(new CtrlType[0]));
            conf.Set("enableAcl", 1);
            conf.Set("timeOut", timeout);

            var echoServer = new EchoServer(conf, oneBind);
            echoServer.Start();

            //TCPクライアント

            const int max = 1600;
            var buf = new byte[max];
            buf[8] = 100; //CheckData

            for (int i = 0; i < 3; i++)
            {
                var sockUdp = new SockUdp(new Kernel(), ip, port, null, buf);
                var b = sockUdp.Recv(timeout);
                Assert.Equal(b[8], buf[8]); //CheckData
                Assert.Equal(max, b.Length);

                sockUdp.Close();
            }

            echoServer.Dispose();

        }

        public bool IsLife()
        {
            return true;
        }
    }
}
