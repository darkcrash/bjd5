using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd;
using Bjd.Net;
using Bjd.Option;
using Bjd.Utils;
using Bjd.Common.Test;
using Xunit;
using Bjd.WebServer;

namespace WebServerTest
{

    public class CgiTest : ILife, IClassFixture<CgiTest.ServerFixture>
    {

        public class ServerFixture : IDisposable
        {
            private TmpOption _op; //設定ファイルの上書きと退避
            private Server _v6Sv; //サーバ
            private Server _v4Sv; //サーバ

            public ServerFixture()
            {
                //設定ファイルの退避と上書き
                _op = new TmpOption("Bjd.WebServer.CoreCLR.Test", "WebServerTest.ini");

                Bjd.Service.Service.ServiceTest();

                var kernel = new Kernel();
                var option = kernel.ListOption.Get("Web-localhost:88");
                var conf = new Conf(option);

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

        public CgiTest(ServerFixture fixture)
        {
            _fixture = fixture;

        }

        [Fact]
        public void EnvCgiTest()
        {

            var cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 88, 10, null);

            cl.Send(Encoding.ASCII.GetBytes("GET /CgiTest/env.cgi HTTP/1.1\nHost: ws00\n\n"));
            int sec = 10; //CGI処理待ち時間（これで大丈夫?）
            var lines = Inet.RecvLines(cl, sec, this);
            const string pattern = "<b>SERVER_NAME</b>";
            var find = lines.Any(l => l.IndexOf(pattern) != -1);
            //Assert.Equal(find, true, string.Format("not found {0}", pattern));
            Assert.Equal(find, true);

            cl.Close();
        }

        public bool IsLife()
        {
            return true;
        }
    }

}
