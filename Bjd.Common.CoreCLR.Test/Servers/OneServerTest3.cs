using System;
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
using Bjd.Threading;

namespace Bjd.Test.Servers
{
    public class OneServerTest3 : ILife, IDisposable
    {
        TestService _service;
        Kernel _kernel;

        public OneServerTest3()
        {
            _service = TestService.CreateTestService();
            _kernel = _service.Kernel;
            _kernel.ListInitialize();
        }

        public void Dispose()
        {
            _service.Dispose();
        }

        private class EchoServer : OneServer
        {


            public EchoServer(Kernel kernel, Conf conf, OneBind oneBind) : base(kernel, conf, oneBind)
            {

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
                //サーバ終了までキープする
                while (IsLife())
                {
                    Thread.Sleep(100);
                }
            }
            //RemoteServerでのみ使用される
            public override void Append(LogMessage oneLog)
            {

            }

            protected override void CheckLang() { }
        }

        EchoServer StartServer(Ip ip, int port, int enableAcl, Dat acl)
        {
            const int timeout = 300;
            var oneBind = new OneBind(ip, ProtocolKind.Tcp);
            var conf = TestUtil.CreateConf(_kernel, "OptionSample");
            conf.Set("port", port);
            conf.Set("multiple", 10);
            conf.Set("acl", acl);
            conf.Set("enableAcl", enableAcl);
            conf.Set("timeOut", timeout);

            var sv = new EchoServer(_service.Kernel, conf, oneBind);
            sv.Start();
            return sv;
        }

        SockTcp StartClient(Ip ip, int port)
        {

            var cl = new SockTcp(_service.Kernel, ip, port, 300, null);
            Thread.Sleep(300);
            return cl;
        }

        [Fact]
        public void 許可リスト無し_のみ許可する_Deny()
        {

            //setUp
            var ip = TestUtil.CreateIp("127.0.0.1");
            //const int port = 9986;
            int port = _service.GetAvailablePort(ip, 9986);
            const int enableAcl = 0; //指定したアドレスからのアクセスのみを許可する
            var acl = new Dat(new CtrlType[0]); //許可リストなし

            using (var sut = StartServer(ip, port, enableAcl, acl))
            {
                var cl = StartClient(ip, port);
                var expected = 0; //　Deny

                //exercise
                var actual = sut.Count;

                //verify
                Assert.Equal(expected, actual);

                //tearDown
                cl.Close();
                sut.Stop();
            }

        }


        [Fact]
        public void 許可リスト無し_のみ禁止する_Allow()
        {
            //setUp
            var ip = TestUtil.CreateIp("127.0.0.1");
            //const int port = 9987;
            int port = _service.GetAvailablePort(ip, 9987);
            const int enableAcl = 1; //指定したアドレスからのアクセスのみを禁止する
            var acl = new Dat(new CtrlType[0]); //許可リストなし

            using (var sut = StartServer(ip, port, enableAcl, acl))
            {
                var cl = StartClient(ip, port);
                var expected = 1; //　Allow

                //exercise
                var actual = sut.Count;

                //verify
                Assert.Equal(expected, actual);

                //tearDown
                cl.Close();
                sut.Stop();
            }
        }

        [Fact]
        public void 許可リスト有り_のみ許可する_Allow()
        {

            //setUp
            var ip = TestUtil.CreateIp("127.0.0.1");
            //const int port = 9988;
            int port = _service.GetAvailablePort(ip, 9988);
            const int enableAcl = 0; //指定したアドレスからのアクセスのみを許可する
            var acl = new Dat(new[] { CtrlType.TextBox, CtrlType.TextBox }); //許可リストあり
            acl.Add(true, "NAME\t127.0.0.1");


            using (var sut = StartServer(ip, port, enableAcl, acl))
            {
                var cl = StartClient(ip, port);
                var expected = 1; //　Allow

                //exercise
                var actual = sut.Count;

                //verify
                Assert.Equal(expected, actual);

                //tearDown
                cl.Close();
                sut.Stop();
            }

        }

        [Fact]
        public void 許可リスト有り_のみ禁止する_Deny()
        {

            //setUp
            var ip = TestUtil.CreateIp("127.0.0.1");
            //const int port = 9989;
            int port = _service.GetAvailablePort(ip, 9989);
            const int enableAcl = 1; //指定したアドレスからのアクセスのみを禁止する
            var acl = new Dat(new[] { CtrlType.TextBox, CtrlType.TextBox }); //許可リストあり
            acl.Add(true, "NAME\t127.0.0.1");

            using (var sut = StartServer(ip, port, enableAcl, acl))
            {
                var cl = StartClient(ip, port);
                var expected = 0; //　Deny

                //exercise
                var actual = sut.Count;

                //verify
                Assert.Equal(expected, actual);

                //tearDown
                cl.Close();
                sut.Stop();
            }

        }

        public bool IsLife()
        {
            return true;
        }
    }
}