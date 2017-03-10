using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using Bjd.WebServer;
using Bjd.Services;
using Bjd.Threading;
using Bjd.Net.Sockets;
using Xunit.Abstractions;
using Bjd.Test.Logs;

namespace WebServerTest
{

    public class CgiTest : ILife, IDisposable
    {
        private TestService _service;
        private Kernel _kernel;
        private ConfigurationBase _option;

        public CgiTest(ITestOutputHelper output)
        {
            _service = TestService.CreateTestService();
            _service.SetOption("CgiTest.ini");
            _service.ContentDirectory("public_html");

            _kernel = _service.Kernel;
            _kernel.ListInitialize();

            _option = _kernel.ListOption.Get("Web-localhost:7188");
            _service.AddOutput(output);

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
                Assert.Equal(true, find);

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
                Assert.Equal(true, find);

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
