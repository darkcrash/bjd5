using System;
using System.Text;
using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using Bjd.WebServer;
using Bjd.Services;

namespace WebServerTest
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
                _op = new TmpOption("Bjd.WebServer.CoreCLR.Test", "WebServerTest.ini");

                Service.ServiceTest();

                var kernel = new Kernel();
                var option = kernel.ListOption.Get("Web-localhost:88");
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

        public ServerTest(ServerFixture fixture)
        {
            _fixture = fixture;

        }

        public void Dispose()
        {

        }


        [Fact]
        public void ステータス情報_ToString_の出力確認_V4()
        {

            var sv = _fixture._v4Sv;
            var expected = "+ サービス中 \t    Web-localhost:88\t[127.0.0.1\t:TCP 88]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 56);
            //verify
            Assert.Equal(actual, expected);

        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V6()
        {

            var sv = _fixture._v6Sv;
            var expected = "+ サービス中 \t    Web-localhost:88\t[::1\t:TCP 88]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 50);
            //verify
            Assert.Equal(actual, expected);

        }


        [Fact]
        public void Http10Test()
        {

            //setUp
            var _v4Cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 88, 10, null);
            var expected = "HTTP/1.0 200 Document follows\r\n";

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes("GET / HTTP/1.0\n\n"));
            var buf = _v4Cl.LineRecv(3, this);
            var actual = Encoding.ASCII.GetString(buf);
            //verify
            Assert.Equal(actual, expected);

            //tearDoen
            _v4Cl.Close();


        }

        [Fact]
        public void Http11Test()
        {

            //setUp
            var _v4Cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 88, 10, null);
            var expected = "HTTP/1.1 400 Missing Host header or incompatible headers detected.\r\n";

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes("GET / HTTP/1.1\n\n"));
            var buf = _v4Cl.LineRecv(3, this);
            var actual = Encoding.ASCII.GetString(buf);
            //verify
            Assert.Equal(actual, expected);

            //tearDoen
            _v4Cl.Close();

        }

        [Theory]
        [InlineData("9.0")]
        [InlineData("1")]
        [InlineData("")]
        [InlineData("?")]
        public void サポート外バージョンのリクエストは処理されない(string ver)
        {

            //setUp
            var _v4Cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 88, 10, null);
            byte[] expected = null;

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes(string.Format("GET / HTTP/{0}\n\n", ver)));
            var actual = _v4Cl.LineRecv(3, this);

            //verify
            Assert.Equal(actual, expected);

            //tearDoen
            _v4Cl.Close();


        }

        [Theory]
        [InlineData("XXX")]
        [InlineData("")]
        [InlineData("?")]
        [InlineData("*")]
        public void 無効なプロトコルのリクエストは処理されない(string protocol)
        {

            //setUp
            var _v4Cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 88, 10, null);
            byte[] expected = null;

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes(string.Format("GET / {0}/1.0\n\n", protocol)));
            var actual = _v4Cl.LineRecv(3, this);
            //verify
            Assert.Equal(actual, expected);

            //tearDoen
            _v4Cl.Close();
        }

        [Theory]
        [InlineData("?")]
        [InlineData(",")]
        [InlineData(".")]
        [InlineData("aaa")]
        [InlineData("")]
        [InlineData("_")]
        [InlineData("????")]
        public void 無効なURIは処理されない(string uri)
        {

            //setUp
            var _v4Cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 88, 10, null);
            byte[] expected = null;

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes(string.Format("GET {0} HTTP/1.0\n\n", uri)));
            var actual = _v4Cl.LineRecv(3, this);
            //verify
            Assert.Equal(actual, expected);

            //tearDoen
            _v4Cl.Close();
        }

        [Theory]
        [InlineData("SET")]
        [InlineData("POP")]
        [InlineData("")]
        public void 無効なメソッドは処理されない(string method)
        {

            //setUp
            var _v4Cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 88, 10, null);
            byte[] expected = null;

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes(string.Format("{0} / HTTP/1.0\n\n", method)));
            var actual = _v4Cl.LineRecv(3, this);
            //verify
            Assert.Equal(actual, expected);

            //tearDoen
            _v4Cl.Close();
        }

        [Theory]
        [InlineData("GET / HTTP/111")]
        [InlineData("GET /")]
        [InlineData("GET")]
        [InlineData("HTTP/1.0")]
        [InlineData("XXX / HTTP/1.0")]
        [InlineData("GET_/_HTTP/1.0")]
        [InlineData("")]
        public void 無効なリクエストは処理されない(string reauest)
        {

            //setUp
            var _v4Cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 88, 10, null);
            byte[] expected = null;

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes(reauest));
            var actual = _v4Cl.LineRecv(3, this);
            //verify
            Assert.Equal(actual, expected);

            //tearDoen
            _v4Cl.Close();
        }



        public bool IsLife()
        {
            return true;
        }
    }
}
