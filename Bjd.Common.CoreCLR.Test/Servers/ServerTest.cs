using System.Threading;
using Bjd;
using Bjd.Controls;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;
using Bjd.Net.Sockets;
using Xunit;
using Bjd.Services;
using System;

namespace Bjd.Test.Servers
{

    public class ServerTest : IDisposable
    {
        TestService _service;
        public ServerTest()
        {
            _service = TestService.CreateTestService();
        }
        public void Dispose()
        {
            _service.Dispose();
        }

        //サーバ動作確認用
        private class MyServer : OneServer
        {
            public MyServer(Kernel kernel, Conf conf, OneBind oneBind) : base(kernel, conf, oneBind)
            {
            }

            protected override bool OnStartServer()
            {
                return true;
            }

            protected override void OnStopServer()
            {
            }

            public override string GetMsg(int no)
            {
                return "";
            }

            protected override void OnSubThread(SockObj sockObj)
            {
                for (var i = 3; i >= 0 && IsLife(); i--)
                {
                    if (sockObj.SockState != SockState.Connect)
                    {
                        //TestUtil.prompt(String.format("接続中...sockAccept.getSockState!=Connect"));
                        break;
                    }

                    //TestUtil.prompt(String.format("接続中...あと%d回待機", i));
                    Thread.Sleep(1000);
                }
            }
            //RemoteServerでのみ使用される
            public override void Append(OneLog oneLog)
            {

            }

            protected override void CheckLang() { }
        }

        [Fact]
        public void A001()
        {
            var ip = new Ip(IpKind.V4Localhost);
            var port = _service.GetAvailablePort(ip, 8888);
            var oneBind = new OneBind(ip, ProtocolKind.Tcp);
            var conf = TestUtil.CreateConf("OptionSample");
            conf.Set("protocolKind", (int)ProtocolKind.Tcp);
            conf.Set("port", port);
            conf.Set("multiple", 10);
            conf.Set("acl", new Dat(new CtrlType[0]));
            conf.Set("enableAcl", 1);
            conf.Set("timeOut", 3);

            using (var myServer = new MyServer(_service.Kernel, conf, oneBind))
            {
                myServer.Start();
                for (var i = 10; i > 0; i--)
                {
                    Thread.Sleep(1);
                }
                myServer.Stop();
            }

        }
    }
}
