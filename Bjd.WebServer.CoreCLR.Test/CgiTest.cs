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
using Bjd.Net.Sockets;

namespace WebServerTest
{

    public class CgiTest : ILife
    {
        private TestService _service;
        private Kernel _kernel;
        private OneOption _option;

        public CgiTest()
        {
            _service = TestService.CreateTestService();
            _service.SetOption("CgiTest.ini");
            _service.ContentDirectory("public_html");
            _kernel = _service.Kernel;
            _option = _kernel.ListOption.Get("Web-localhost:7188");

        }

        public void Dispose()
        {
            _service.Dispose();

        }

        [Fact]
        public void EnvCgiTestv4()
        {
            var conf = new Conf(_option);
            var port = _service.GetAvailablePort(IpKind.V4Localhost, conf);
            //サーバ起動
            using (var sv = new WebServer(_kernel, conf, new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp)))
            {
                sv.Start();
                System.Threading.Thread.Sleep(500);

                SockTcp cl = null;
                for (var r = 0; r < 10; r++)
                {
                    cl = Inet.Connect(_kernel, new Ip(IpKind.V4Localhost), port, 20, null);
                    if (cl.SockState == Bjd.Net.Sockets.SockState.Connect) break;
                }

                cl.Send(Encoding.ASCII.GetBytes("GET /CgiTest/env.cgi HTTP/1.1\n"));
                //cl.Send(Encoding.ASCII.GetBytes("Connection: keep-alive\n"));
                cl.Send(Encoding.ASCII.GetBytes("Host: localhost\n"));
                cl.Send(Encoding.ASCII.GetBytes("\n"));
                int sec = 10; //CGI処理待ち時間（これで大丈夫?）

                //var lines = Inet.RecvLines(cl, sec, this);
                var lines = new List<string>();
                for (var i = 0; i < 78; i++)
                {
                    var result = cl.LineRecv(sec, this);
                    if (result == null) break;
                    result = Inet.TrimCrlf(result);
                    var text = Encoding.ASCII.GetString(result);
                    lines.Add(text);
                }
                const string pattern = "<b>SERVER_NAME</b>";
                var find = lines.Any(l => l.IndexOf(pattern) != -1);
                //Assert.Equal(find, true, string.Format("not found {0}", pattern));
                Assert.NotEqual(0, lines.Count);
                Assert.NotEqual(1, lines.Count);
                Assert.NotEqual(2, lines.Count);
                Assert.Equal(find, true);

                cl.Close();
                sv.Stop();
            }

        }

        [Fact]
        public void EnvCgiTestv6()
        {
            var conf = new Conf(_option);
            var port = _service.GetAvailablePort(IpKind.V6Localhost, conf);
            //サーバ起動
            using (var sv = new WebServer(_kernel, conf, new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp)))
            {
                sv.Start();

                var cl = Inet.Connect(_kernel, new Ip(IpKind.V6Localhost), port, 10, null);

                cl.Send(Encoding.ASCII.GetBytes("GET /CgiTest/env.cgi HTTP/1.1\n"));
                //cl.Send(Encoding.ASCII.GetBytes("Connection: keep-alive\n"));
                cl.Send(Encoding.ASCII.GetBytes("Host: localhost\n"));
                cl.Send(Encoding.ASCII.GetBytes("\n"));
                int sec = 10; //CGI処理待ち時間（これで大丈夫?）
                var lines = Inet.RecvLines(cl, sec, this);
                const string pattern = "<b>SERVER_NAME</b>";
                var find = lines.Any(l => l.IndexOf(pattern) != -1);
                //Assert.Equal(find, true, string.Format("not found {0}", pattern));
                Assert.NotEqual(0, lines.Count);
                Assert.NotEqual(1, lines.Count);
                Assert.NotEqual(2, lines.Count);
                Assert.Equal(find, true);

                cl.Close();
                sv.Stop();
            }
        }


        public bool IsLife()
        {
            return true;
        }
    }

}
