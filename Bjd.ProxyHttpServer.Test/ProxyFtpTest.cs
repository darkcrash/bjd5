using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using Bjd.ProxyHttpServer;
using Bjd.Initialization;
using Bjd.Threading;
using Bjd.Net.Sockets;

namespace ProxyHttpServerTest
{
    public class ProxyFtpTest : ILife, IDisposable, IClassFixture<ProxyFtpTest.ServerFixture>
    {

        public class ServerFixture : IDisposable
        {
            internal TestService _service;
            private Server _v6Sv; //サーバ
            private Server _v4Sv; //サーバ
            internal int port;

            public ServerFixture()
            {
                _service = TestService.CreateTestService();
                _service.SetOption("ProxyFtpTest.ini");

                Kernel kernel = _service.Kernel;
                kernel.ListInitialize();

                var option = kernel.ListOption.Get("ProxyHttp");
                Conf conf = new Conf(option);

                var ip = new Ip(IpKind.V4Localhost);
                port = _service.GetAvailablePort(ip, conf);

                //サーバ起動
                _v4Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
                _v4Sv.Start();

                _v6Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
                _v6Sv.Start();

            }

            public void Dispose()
            {
                //サーバ停止
                try
                {
                    _v4Sv.Stop();
                    _v6Sv.Stop();

                    _v4Sv.Dispose();
                    _v6Sv.Dispose();
                }
                finally
                {
                    _service.Dispose();
                }

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
            var ip = new Ip(IpKind.V4Localhost);
            var cl = Inet.Connect(kernel, ip, _fixture.port, 10, null);

            //cl.Send(Encoding.ASCII.GetBytes("GET ftp://ftp.iij.ad.jp/ HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n"));
            cl.Send(Encoding.ASCII.GetBytes("GET ftp://ftp.jaist.ac.jp/ HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n"));

            //exercise
            //var lines = Inet.RecvLines(cl, 20, this);

            var lines = new List<string>();
            for (var i = 0; i < 100; i++)
            {
                var buf = cl.LineRecv(5, this);
                if (buf == null) break;
                buf = Inet.TrimCrlf(buf);
                lines.Add(Encoding.ASCII.GetString(buf));
            }

            //verify
            Assert.NotEqual(0, lines.Count);
            Assert.Equal(lines[0], "HTTP/1.0 200 OK");

            cl.Close();
            cl.Dispose();

        }

        public bool IsLife()
        {
            return true;
        }
    }
}
