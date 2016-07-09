using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using Bjd.WebServer;
using Bjd.Services;
using Bjd.Threading;

namespace WebServerTest
{

    public class CgiTest : ILife, IClassFixture<CgiTest.ServerFixture>
    {

        public class ServerFixture : IDisposable
        {
            internal TestService _service;
            private WebServer _v6Sv; //サーバ
            private WebServer _v4Sv; //サーバ

            public ServerFixture()
            {
                _service = TestService.CreateTestService();
                _service.SetOption("CgiTest.ini");
                _service.ContentDirectory("public_html");

                var kernel = _service.Kernel;
                var option = kernel.ListOption.Get("Web-localhost:7188");
                var conf = new Conf(option);

                //サーバ起動
                _v4Sv = new WebServer(kernel, conf, new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
                _v4Sv.Start();

                _v6Sv = new WebServer(kernel, conf, new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
                _v6Sv.Start();

            }

            public void Dispose()
            {
                //サーバ停止
                _v4Sv.Stop();
                _v6Sv.Stop();

                _v4Sv.Dispose();
                _v6Sv.Dispose();

                _service.Dispose();

            }

        }

        private ServerFixture _fixture;

        public CgiTest(ServerFixture fixture)
        {
            _fixture = fixture;

        }

        [Fact]
        public void EnvCgiTestv4()
        {
            var kernel = _fixture._service.Kernel;
            var cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), 7188, 10, null);

            cl.Send(Encoding.ASCII.GetBytes("GET /CgiTest/env.cgi HTTP/1.1\n"));
            //cl.Send(Encoding.ASCII.GetBytes("Connection: keep-alive\n"));
            cl.Send(Encoding.ASCII.GetBytes("Host: localhost\n"));
            cl.Send(Encoding.ASCII.GetBytes("\n"));
            int sec = 5; //CGI処理待ち時間（これで大丈夫?）
            var lines = Inet.RecvLines(cl, sec, this);
            const string pattern = "<b>SERVER_NAME</b>";
            var find = lines.Any(l => l.IndexOf(pattern) != -1);
            //Assert.Equal(find, true, string.Format("not found {0}", pattern));
            Assert.NotEqual(0, lines.Count);
            Assert.NotEqual(1, lines.Count);
            Assert.NotEqual(2, lines.Count);
            Assert.Equal(find, true);

            cl.Close();
        }

        [Fact]
        public void EnvCgiTestv6()
        {
            var kernel = _fixture._service.Kernel;
            var cl = Inet.Connect(kernel, new Ip(IpKind.V6Localhost), 7188, 10, null);

            cl.Send(Encoding.ASCII.GetBytes("GET /CgiTest/env.cgi HTTP/1.1\n"));
            //cl.Send(Encoding.ASCII.GetBytes("Connection: keep-alive\n"));
            cl.Send(Encoding.ASCII.GetBytes("Host: localhost\n"));
            cl.Send(Encoding.ASCII.GetBytes("\n"));
            int sec = 5; //CGI処理待ち時間（これで大丈夫?）
            var lines = Inet.RecvLines(cl, sec, this);
            const string pattern = "<b>SERVER_NAME</b>";
            var find = lines.Any(l => l.IndexOf(pattern) != -1);
            //Assert.Equal(find, true, string.Format("not found {0}", pattern));
            Assert.NotEqual(0, lines.Count);
            Assert.NotEqual(1, lines.Count);
            Assert.NotEqual(2, lines.Count);
            Assert.Equal(find, true);

            cl.Close();
        }


        public bool IsLife()
        {
            return true;
        }
    }

}
