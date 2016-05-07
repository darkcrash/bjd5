using System;
using System.Text;
using Bjd.net;
using Bjd.option;
using Bjd.sock;
using Bjd.Common.Test;
using Xunit;
using Bjd.WebServer;
using Bjd;
using System.Net;

namespace WebServerTest
{

    public class EnvTest : IDisposable, IClassFixture<EnvTest.ServerFixture>
    {

        public class ServerFixture : IDisposable
        {
            internal Kernel _kernel;
            private TmpOption _op; //設定ファイルの上書きと退避
            internal OneOption option;

            public ServerFixture()
            {

                //設定ファイルの退避と上書き
                _op = new TmpOption("Bjd.WebServer.CoreCLR.Test", "WebServerTest.ini");

                Bjd.service.Service.ServiceTest();

                _kernel = new Kernel();
                option = _kernel.ListOption.Get("Web-localhost:88");



            }

            public void Dispose()
            {
                //設定ファイルのリストア
                _op.Dispose();

            }

        }

        private ServerFixture _fixture;

        public EnvTest(ServerFixture fixture)
        {
            _fixture = fixture;

        }

        public void Dispose()
        {
        }

        [Theory]
        [InlineData("PATHEXT", ".COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH;.MSC")]
        [InlineData("WINDIR", "C:\\Windows")]
        [InlineData("COMSPEC", "C:\\Windows\\system32\\cmd.exe")]
        [InlineData("SERVER_SOFTWARE", "BlackJumboDog/102.0.0.0 (windows)")]
        [InlineData("SystemRoot", "C:\\Windows")]
        public void OtherTest(string key, string val)
        {
            var request = new Request(null, null);
            var header = new Header();
            var tcpObj = new SockTcp(new Kernel(), new Ip(IpKind.V4_0), 88, 3, null);
            const string fileName = "";
            var env = new Env(_fixture._kernel, new Conf(_fixture.option), request, header, tcpObj, fileName);
            foreach (var e in env)
            {
                if (e.Key == key)
                {
                    if (e.Key == "SERVER_SOFTWARE" && e.Val.IndexOf(".1478") > 0)
                    {
                        Assert.Equal(e.Val.ToLower(), "BlackJumboDog/7.1.2000.1478 (Windows)".ToLower());
                    }
                    else
                    {
                        Assert.Equal(e.Val.ToLower(), val.ToLower());
                    }
                    return;
                }
            }
            Assert.Equal(key, "");
        }

        [Theory]
        [InlineData("DOCUMENT_ROOT", "D:\\work\\web")]
        [InlineData("SERVER_ADMIN", "root@localhost")]
        public void OptionTest(string key, string val)
        {
            var request = new Request(null, null);

            var conf = new Conf(_fixture.option);
            conf.Set("documentRoot", val);

            var header = new Header();
            var tcpObj = new SockTcp(new Kernel(), new Ip("0.0.0.0"), 88, 1, null);
            const string fileName = "";
            var env = new Env(_fixture._kernel, conf, request, header, tcpObj, fileName);
            foreach (var e in env)
            {
                if (e.Key == key)
                {
                    Assert.Equal(e.Val, val);
                    return;
                }
            }
            Assert.Equal(key, "");

        }

        [Theory]
        [InlineData("REMOTE_ADDR", "10.0.0.100")]
        [InlineData("REMOTE_PORT", "5000")]
        [InlineData("SERVER_ADDR", "127.0.0.1")]
        [InlineData("SERVER_PORT", "80")]
        public void TcpObjTest(string key, string val)
        {

            var conf = new Conf(_fixture.option);
            var request = new Request(null, null);
            var header = new Header();
            var tcpObj = new SockTcp(new Kernel(), new Ip("0.0.0.0"), 88, 1, null);
            tcpObj.LocalAddress = new IPEndPoint((new Ip("127.0.0.1")).IPAddress, 80);
            tcpObj.RemoteAddress = new IPEndPoint((new Ip("10.0.0.100")).IPAddress, 5000);
            const string fileName = "";
            var env = new Env(_fixture._kernel, conf, request, header, tcpObj, fileName);

            foreach (var e in env)
            {
                if (e.Key == key)
                {
                    Assert.Equal(e.Val, val);
                    return;
                }
            }
            Assert.Equal(key, "");
        }

        [Theory]
        [InlineData("HTTP_ACCEPT_ENCODING", "gzip,deflate,sdch")]
        [InlineData("HTTP_ACCEPT_LANGUAGE", "ja,en-US;q=0.8,en;q=0.6")]
        [InlineData("HTTP_ACCEPT", "text/html,application/xhtml")]
        [InlineData("HTTP_USER_AGENT", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)")]
        [InlineData("HTTP_CONNECTION", "keep-alive")]
        public void HeaderTest(string key, string val)
        {


            var request = new Request(null, null);
            var header = new Header();
            header.Append("Connection", Encoding.ASCII.GetBytes("keep-alive"));
            header.Append("User-Agent", Encoding.ASCII.GetBytes("Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)"));
            header.Append("Accept", Encoding.ASCII.GetBytes("text/html,application/xhtml"));
            header.Append("Accept-Encoding", Encoding.ASCII.GetBytes("gzip,deflate,sdch"));
            header.Append("Accept-Language", Encoding.ASCII.GetBytes("ja,en-US;q=0.8,en;q=0.6"));
            header.Append("Accept-Charset", Encoding.ASCII.GetBytes("Shift_JIS,utf-8;q=0.7,*;q=0.3"));
            header.Append("Cache-Control", Encoding.ASCII.GetBytes("max-age=0"));

            var tcpObj = new SockTcp(new Kernel(), new Ip("0.0.0.0"), 88, 3, null);
            const string fileName = "";
            var env = new Env(_fixture._kernel, new Conf(_fixture.option), request, header, tcpObj, fileName);
            foreach (var e in env)
            {
                if (e.Key == key)
                {
                    Assert.Equal(e.Val, val);
                    return;
                }
            }
            Assert.Equal(key, "");
        }


    }

}
