using System;
using System.Text;
using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Sockets;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using Bjd.WebApiServer;
using Bjd.Services;

namespace WebApiServerTest
{

    public class ServerTest : ILife, IDisposable, IClassFixture<ServerTest.ServerFixture>
    {
        public class ServerFixture : IDisposable
        {

            private TmpOption _op; //設定ファイルの上書きと退避
            internal Server _v6Sv; //サーバ
            internal Server _v4Sv; //サーバ

            public ServerFixture()
            {
                TestUtil.CopyLangTxt();//BJD.Lang.txt


                //設定ファイルの退避と上書き
                _op = new TmpOption("Bjd.WebApiServer.CoreCLR.Test", "WebApiServerTest.ini");

                Service.ServiceTest();

                var kernel = new Kernel();
                var option = kernel.ListOption.Get("WebApi");
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


        public ServerTest(ServerFixture fixture)
        {
            _fixture = fixture;

        }

        public void Dispose()
        {
        }

        //クライアントの生成
        SockTcp CreateClient(InetKind inetKind)
        {
            var port = 5050;
            if (inetKind == InetKind.V4)
            {
                return Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), port, 10, null);
            }
            return Inet.Connect(new Kernel(), new Ip(IpKind.V6Localhost), port, 10, null);
        }


        [Fact]
        public void ステータス情報_ToString_の出力確認_V4()
        {
            //setUP
            var sv = _fixture._v4Sv;
            var expected = "+ サービス中 \t              WebApi\t[127.0.0.1\t:TCP 5050]\tThread";
            //exercise
            var actual = sv.ToString().Substring(0, 58);
            //verify
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V6()
        {
            //setUP
            var sv = _fixture._v6Sv;
            var expected = "+ サービス中 \t              WebApi\t[::1\t:TCP 5050]\tThread";
            //exercise
            var actual = sv.ToString().Substring(0, 52);
            //verify
            Assert.Equal(actual, expected);

        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void Test(InetKind inetKind)
        {

            //setUp
            var cl = CreateClient(inetKind);
            var expected = "{\"code\":500,\"message\":\"Not Implemented []\"}";

            //exercise
            cl.Send(Encoding.ASCII.GetBytes("GET / HTTP/1.1\n\n"));

            var buf = cl.Recv(3000, 3, this);

            var str = Encoding.UTF8.GetString(buf);
            var actual = str.Substring(str.IndexOf("\r\n\r\n") + 4);
            //verify
            Assert.Equal(actual, expected);

            //tearDown
            cl.Close();


        }


        public bool IsLife()
        {
            return true;
        }
    }
}

