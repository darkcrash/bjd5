using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Bjd;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using Bjd.Net.Sockets;
using Xunit;
using Bjd.Controls;
using Bjd.Initialization;
using Bjd.Threading;
using Xunit.Abstractions;

namespace Bjd.Test.Servers
{

    public class OneServerTest : IDisposable
    {

        TestService _service;
        Kernel _kernel;

        public OneServerTest(ITestOutputHelper output)
        {
            _service = TestService.CreateTestService();
            _service.SetOption("Option.ini");

            _kernel = _service.Kernel;
            _kernel.ListInitialize();
            _service.AddOutput(output);
        }

        public void Dispose()
        {
            _service.Dispose();
        }

        private class MyServer : OneServer
        {
            public MyServer(Kernel kernel, Conf conf, OneBind oneBind) : base(kernel, conf, oneBind)
            {

            }

            public override string GetMsg(int messageNo)
            {
                return "";
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
                while (IsLife())
                {
                    Thread.Sleep(10); //これが無いと、別スレッドでlifeをfalseにできない
                    if (sockObj.SockState != SockState.Connect)
                    {
                        Console.WriteLine(@">>>>>sockAccept.getSockState()!=SockState.CONNECT");
                        break;
                    }
                }
            }
            //RemoteServerでのみ使用される
            public override void Append(LogMessage oneLog)
            {

            }

            protected override void CheckLang() { }
        }

        internal class MyClient
        {
            private Socket _s = null;
            private readonly String _addr;
            private readonly int _port;
            private Thread _t;
            private bool _life;

            public MyClient(String addr, int port)
            {
                _addr = addr;
                _port = port;
            }

            public void Connet()
            {

                _life = true;
                _t = new Thread(Loop) { IsBackground = true };
                _t.Start();

                //接続完了まで少し時間が必要
                while (_s == null || !_s.Connected)
                {
                    Thread.Sleep(10);
                }

            }
            void Loop()
            {
                _s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _s.Connect(_addr, _port);
                while (_life)
                {
                    Thread.Sleep(10);
                }

            }

            public void Dispose()
            {
                //			try {
                //				s.shutdownInput();
                //				s.shutdownOutput();
                //				s.close();
                //			} catch (IOException e1) {
                //				e1.printStackTrace();
                //			}
                _life = false;
                while (_t.IsAlive)
                {
                    Thread.Sleep(10);
                }
                //_s.Close();
                _s.Dispose();
            }
        }

        [Fact]
        public void RepeatStartStop_TCP()
        {
            var kernel = _kernel;

            var ip = new Ip(IpKind.V4Localhost);
            var port = _service.GetAvailablePort(ip, 9997);
            var oneBind = new OneBind(ip, ProtocolKind.Tcp);
            Conf conf = TestUtil.CreateConf(_kernel, "OptionSample");
            conf.Set("port", port);
            conf.Set("multiple", 10);
            conf.Set("acl", new Dat(new CtrlType[0]));
            conf.Set("enableAcl", 1);
            conf.Set("timeOut", 3);

            using (var myServer = new MyServer(kernel, conf, oneBind))
            {

                for (var i = 0; i < 10; i++)
                {
                    // Start
                    myServer.Start();
                    Assert.Equal(ThreadBaseKind.Running, myServer.ThreadBaseKind);
                    Assert.Equal(SockState.Bind, myServer.SockState);

                    // Stop
                    myServer.Stop();
                    Assert.Equal(ThreadBaseKind.After, myServer.ThreadBaseKind);
                    Assert.Equal(SockState.Error, myServer.SockState);

                }

            }
        }

        [Fact]
        public void RepeatStartStop_UDP()
        {
            var kernel = _kernel;

            var ip = new Ip(IpKind.V4Localhost);
            var port = _service.GetAvailableUdpPort(ip, 9991);
            var oneBind = new OneBind(ip, ProtocolKind.Udp);
            Conf conf = TestUtil.CreateConf(_kernel, "OptionSample");
            conf.Set("port", port);
            conf.Set("multiple", 10);
            conf.Set("acl", new Dat(new CtrlType[0]));
            conf.Set("enableAcl", 1);
            conf.Set("timeOut", 3);

            using (var myServer = new MyServer(kernel, conf, oneBind))
            {

                for (var i = 0; i < 10; i++)
                {
                    // Start
                    myServer.Start();
                    Assert.Equal(ThreadBaseKind.Running, myServer.ThreadBaseKind);
                    Assert.Equal(SockState.Bind, myServer.SockState);

                    // Stop
                    myServer.Stop();
                    Assert.Equal(ThreadBaseKind.After, myServer.ThreadBaseKind);
                    Assert.Equal(SockState.Error, myServer.SockState);
                }

            }
        }


        [Fact]
        public void RepeatNewStartStopDispose_TCP()
        {
            var kernel = _kernel;

            var ip = new Ip(IpKind.V4Localhost);
            var port = _service.GetAvailablePort(ip, 10088);
            var oneBind = new OneBind(ip, ProtocolKind.Tcp);
            Conf conf = TestUtil.CreateConf(_kernel, "OptionSample");
            conf.Set("port", port);
            conf.Set("multiple", 10);
            conf.Set("acl", new Dat(new CtrlType[0]));
            conf.Set("enableAcl", 1);
            conf.Set("timeOut", 3);

            for (var i = 0; i < 10; i++)
            {
                using (var myServer = new MyServer(kernel, conf, oneBind))
                {

                    myServer.Start();
                    Assert.Equal(ThreadBaseKind.Running, myServer.ThreadBaseKind);
                    Assert.Equal(SockState.Bind, myServer.SockState);

                    myServer.Stop();
                    Assert.Equal(ThreadBaseKind.After, myServer.ThreadBaseKind);
                    Assert.Equal(SockState.Error, myServer.SockState);

                }
            }
        }

        [Fact]
        public void RepeatNewStartStopDispose_UDP()
        {
            var kernel = _kernel;

            var ip = new Ip(IpKind.V4Localhost);
            var port = _service.GetAvailableUdpPort(ip, 10089);
            var oneBind = new OneBind(ip, ProtocolKind.Udp);
            Conf conf = TestUtil.CreateConf(_kernel, "OptionSample");
            conf.Set("port", port);
            conf.Set("multiple", 10);
            conf.Set("acl", new Dat(new CtrlType[0]));
            conf.Set("enableAcl", 1);
            conf.Set("timeOut", 3);

            for (var i = 0; i < 10; i++)
            {
                using (var myServer = new MyServer(kernel, conf, oneBind))
                {
                    myServer.Start();
                    Assert.Equal(myServer.ThreadBaseKind, ThreadBaseKind.Running);
                    Assert.Equal(myServer.SockState, SockState.Bind);

                    myServer.Stop();
                    Assert.Equal(myServer.ThreadBaseKind, ThreadBaseKind.After);
                    Assert.Equal(myServer.SockState, SockState.Error);

                }
            }
        }


        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        public void multipleを超えたリクエストは破棄される事をcountで確認する(int multiple)
        {
            var kernel = _kernel;

            //const int port = 8889;
            const string address = "127.0.0.1";
            var ip = new Ip(address);
            var port = _service.GetAvailablePort(ip, 8889);
            var oneBind = new OneBind(ip, ProtocolKind.Tcp);
            Conf conf = TestUtil.CreateConf(_kernel, "OptionSample");
            conf.Set("port", port);
            conf.Set("multiple", multiple);
            conf.Set("acl", new Dat(new CtrlType[0]));
            conf.Set("enableAcl", 1);
            conf.Set("timeOut", 3);

            var ar = new List<MyClient>();
            using (var myServer = new MyServer(kernel, conf, oneBind))
            {

                myServer.Start();

                for (int i = 0; i < multiple + 5; i++)
                {
                    var myClient = new MyClient(address, port);
                    myClient.Connet();
                    ar.Add(myClient);
                }
                Thread.Sleep(500);

                //multiple以上は接続できない
                Assert.Equal(multiple, myServer.Count);

                myServer.Stop();

            }

            foreach (var c in ar)
            {
                c.Dispose();
            }

        }
    }
}
