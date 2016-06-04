using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using Bjd.ProxyHttpServer;
using Bjd.Services;

namespace ProxyHttpServerTest
{
    public class ProxyFtpTest : ILife, IDisposable, IClassFixture<ProxyFtpTest.ServerFixture>
    {

        public class ServerFixture : IDisposable
        {
            internal TestService _service;
            private TestOption _op; //設定ファイルの上書きと退避
            private Server _v6Sv; //サーバ
            private Server _v4Sv; //サーバ

            public ServerFixture()
            {
                //srcDir = string.Format("{0}\\ProxyHttpServerTest", TestUtil.ProhjectDirectory());

                //設定ファイルの退避と上書き
                _op = new TestOption("Bjd.ProxyHttpServer.CoreCLR.Test", "ProxyHttpServerTest.ini");

                _service = TestService.CreateTestService(_op);

                Kernel kernel = _service.Kernel;
                var option = kernel.ListOption.Get("ProxyHttp");
                Conf conf = new Conf(option);

                //サーバ起動
                _v4Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
                _v4Sv.Start();

                _v6Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
                _v6Sv.Start();


            }

            public void Dispose()
            {

                //サーバ停止
                _v4Sv.Stop();
                _v6Sv.Stop();

                _v4Sv.Dispose();
                _v6Sv.Dispose();

                //設定ファイルのリストア
                _op.Dispose();

            }

        }

        private ServerFixture _fixture;

        public ProxyFtpTest(ServerFixture fixture)
        {
            _fixture = fixture;

        }

        public void Dispose()
        {

        }

        [Fact]
        public void HTTP経由のFTPサーバへのアクセス()
        {


            //setUp
            var kernel = _fixture._service.Kernel;
            var cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), 8888, 10, null);

            //cl.Send(Encoding.ASCII.GetBytes("GET ftp://ftp.iij.ad.jp/ HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n"));
            cl.Send(Encoding.ASCII.GetBytes("GET ftp://ftp.jaist.ac.jp/ HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n"));

            //exercise
            var lines = Inet.RecvLines(cl, 20, this);
            //verify
            Assert.Equal(lines[0], "HTTP/1.0 200 OK");

        }

        public bool IsLife()
        {
            return true;
        }
    }
}
