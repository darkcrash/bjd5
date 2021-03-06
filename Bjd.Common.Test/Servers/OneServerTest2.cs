﻿using System.Threading;
using Bjd;
using Bjd.Controls;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using Bjd.Net.Sockets;
using Xunit;
using Bjd.Initialization;
using System;
using Bjd.Threading;

namespace Bjd.Test.Servers
{

    public class OneServerTest2 : ILife, IDisposable
    {
        TestService _service;
        Kernel _kernel;

        public OneServerTest2(Xunit.Abstractions.ITestOutputHelper helper)
        {
            _service = TestService.CreateTestService();
            _service.SetOption("Option.ini");

            _kernel = _service.Kernel;
            _kernel.ListInitialize();
            _service.AddOutput(helper);
        }

        public void Dispose()
        {
            _service.Dispose();
        }

        private class EchoServer : OneServer
        {
            private readonly ProtocolKind _protocolKind;

            public EchoServer(Kernel kernel, Conf conf, OneBind oneBind) : base(kernel, conf, oneBind)
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

            protected override void OnSubThread(ISocket sockObj)
            {
                switch (_protocolKind)
                {
                    case ProtocolKind.Tcp:
                        Tcp((SockTcp)sockObj);
                        break;
                    case ProtocolKind.Udp:
                        Udp((SockUdp)sockObj);
                        break;
                    case ProtocolKind.Internal:
                        Internal((SockInternal)sockObj);
                        break;
                }
            }

            private void Tcp(SockTcp sockTcp)
            {
                while (IsLife() && sockTcp.SockState == SockState.Connect)
                {
                    Thread.Sleep(10); //これが無いと、別スレッドでlifeをfalseにできない
                    var len = sockTcp.Length();
                    if (0 < len)
                    {
                        const int timeout = 10;
                        var buf = sockTcp.Recv(len, timeout, this);
                        sockTcp.Send(buf);
                        //break; //echoしたらセッションを閉じる
                    }
                }
            }

            private void Udp(SockUdp sockUdp)
            {
                var buf = sockUdp.RecvBuf;
                sockUdp.Send(buf);
                //echoしたらセッションを閉じる
            }

            private void Internal(SockInternal sockInternal)
            {
                while (IsLife() && sockInternal.SockState == SockState.Connect)
                {
                    var len = sockInternal.Length();
                    if (0 < len)
                    {
                        const int timeout = 10;
                        var buf = sockInternal.Recv(len, timeout, this);
                        sockInternal.Send(buf);
                        //break; //echoしたらセッションを閉じる
                    }
                }
            }

            //RemoteServerでのみ使用される
            public override void Append(LogMessage oneLog)
            {

            }

            protected override void CheckLang() { }
        }

        [Fact]
        public void OneServerを継承したEchoServer_TCP版_を使用して接続する()
        {

            const string addr = "127.0.0.1";
            //const int port = 9999;
            int port = _service.GetAvailablePort(addr, 9999);
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
            var conf = TestUtil.CreateConf(_kernel, "OptionSample");
            conf.Set("port", port);
            conf.Set("multiple", 10);
            conf.Set("acl", new Dat(new CtrlType[0]));
            conf.Set("enableAcl", 1);
            conf.Set("timeOut", timeout);

            using (var echoServer = new EchoServer(_service.Kernel, conf, oneBind))
            {
                echoServer.Start();

                //TCPクライアント

                const int max = 10000;
                var buf = new byte[max];
                buf[8] = 100; //CheckData
                for (int i = 0; i < 3; i++)
                {
                    var sockTcp = new SockTcp(_service.Kernel, ip, port, timeout, null);

                    sockTcp.Send(buf);

                    while (sockTcp.Length() == 0)
                    {
                        Thread.Sleep(2);
                    }

                    var len = sockTcp.Length();
                    if (0 < len)
                    {
                        var b = sockTcp.Recv(max, timeout, this);
                        Assert.Equal(b[8], buf[8]);//CheckData
                    }
                    Assert.Equal(max, len);

                    sockTcp.Close();

                }

                echoServer.Stop();
            }

        }

        [Fact]
        public void OneServerを継承したEchoServer_UDP版_を使用して接続する()
        {

            const string addr = "127.0.0.1";
            //const int port = 9992;
            int port = _service.GetAvailableUdpPort(addr, 9992);
            const int timeout = 8;
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
            var conf = TestUtil.CreateConf(_kernel, "OptionSample");
            conf.Set("port", port);
            conf.Set("multiple", 10);
            conf.Set("acl", new Dat(new CtrlType[0]));
            conf.Set("enableAcl", 1);
            conf.Set("timeOut", timeout);

            using (var echoServer = new EchoServer(_service.Kernel, conf, oneBind))
            {
                echoServer.Start();

                //TCPクライアント

                const int max = 1600;
                var buf = new byte[max];
                buf[8] = 100; //CheckData

                for (int i = 0; i < 3; i++)
                {
                    var sockUdp = new SockUdp(_service.Kernel, ip, port, null, buf);
                    var b = sockUdp.Recv(timeout);
                    Assert.Equal(b[8], buf[8]); //CheckData
                    Assert.Equal(max, b.Length);

                    sockUdp.Close();
                }

                echoServer.Stop();
            }

        }

        [Fact]
        public void OneServerを継承したEchoServer_Internal版_を使用して接続する()
        {

            const string addr = "127.0.0.1";
            int port = 0;
            const int timeout = 8;
            Ip ip = null;
            try
            {
                ip = new Ip(addr);
            }
            catch (ValidObjException ex)
            {
                Assert.False(true, ex.Message);
            }
            var oneBind = new OneBind(ip, ProtocolKind.Internal);
            var conf = TestUtil.CreateConf(_kernel, "OptionSample");
            conf.Set("port", port);
            conf.Set("multiple", 10);
            conf.Set("acl", new Dat(new CtrlType[0]));
            conf.Set("enableAcl", 1);
            conf.Set("timeOut", timeout);

            using (var echoServer = new EchoServer(_service.Kernel, conf, oneBind))
            {
                echoServer.Start();

                //TCPクライアント

                const int max = 10000;
                var buf = new byte[max];
                buf[8] = 100; //CheckData
                for (int i = 0; i < 3; i++)
                {
                    var sockInternal = echoServer.ConnectInternal();

                    sockInternal.Send(buf);

                    while (sockInternal.Length() == 0)
                    {
                        Thread.Sleep(2);
                    }

                    var len = sockInternal.Length();
                    if (0 < len)
                    {
                        var b = sockInternal.Recv(max, timeout, this);
                        Assert.Equal(b[8], buf[8]);//CheckData
                    }
                    Assert.Equal(max, len);

                    sockInternal.Close();

                }

                echoServer.Stop();
            }

        }


        public bool IsLife()
        {
            return true;
        }
    }
}
