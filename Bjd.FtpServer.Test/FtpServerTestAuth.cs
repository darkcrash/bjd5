using System;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Test;
using Bjd.FtpServer;
using Xunit;
using Bjd.Initialization;
using Bjd.Threading;

namespace FtpServerTest
{

    //[TestFixture]
    public class FtpServerTestAuth : ILife, IDisposable, IClassFixture<FtpServerTestAuth.InternalFixture>
    {
        public class InternalFixture : IDisposable
        {
            public TestService _service;
            public Server _v6Sv; //サーバ
            public Server _v4Sv; //サーバ
            public readonly int port;

            //[TestFixtureSetUp]
            public InternalFixture()
            {
                _service = TestService.CreateTestService();
                _service.SetOption("FtpServerTest.ini");
                _service.ContentDirectory("TestDir");

                Kernel kernel = _service.Kernel;
                kernel.ListInitialize();

                var option = kernel.ListOption.Get("Ftp");
                Conf conf = new Conf(option);
                port = _service.GetAvailablePort(IpKind.V4Localhost, conf);

                //サーバ起動
                _v4Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
                _v4Sv.Start();

                _v6Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
                _v6Sv.Start();

            }


            //[TestFixtureTearDown]
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

        private ISocket _v6Cl; //クライアント
        private ISocket _v4Cl; //クライアント

        private FtpServerTestAuth.InternalFixture _fixture;

        public FtpServerTestAuth(FtpServerTestAuth.InternalFixture fixture)
        {
            _fixture = fixture;
            var kernel = _fixture._service.Kernel;
            var port = _fixture.port;

            //クライアント起動
            _v4Cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), port, 10, null);
            _v6Cl = Inet.Connect(kernel, new Ip(IpKind.V6Localhost), port, 10, null);
            //クライアントの接続が完了するまで、少し時間がかかる
            //Thread.Sleep(10);

        }

        public void Dispose()
        {
            //クライアント停止
            _v4Cl.Close();
            _v6Cl.Close();

            _v4Cl.Dispose();
            _v6Cl.Dispose();

            _v4Cl = null;
            _v6Cl = null;

        }

        //共通処理(バナーチェック)  Resharperのバージョンを吸収
        private void CheckBanner(string str)
        {
            //テストの際は、バージョン番号はテストツール（ReSharper）のバージョンになる
            //const string bannerStr0 = "220 FTP ( BlackJumboDog Version 9.0.0.0 ) ready\r\n";
            const string bannerStr = "220 FTP ( BlackJumboDog .NET Core Version ";
            //Assert.Equal(_v6cl.StringRecv(1, this), BannerStr);
            //if (str != bannerStr0 && str != bannerStr1 && str != bannerStr2 && str != bannerStr3)
            //{
            //    Assert.Fail();
            //}
            if (str.IndexOf(bannerStr) != 0)
            {
                Assert.False(true);
            }
        }


        //共通処理(ログイン成功)
        private void Login(string userName, ISocket cl)
        {
            var banner = cl.StringRecv(1, this);
            CheckBanner(banner);//バナーチェック

            cl.StringSend($"USER {userName}");
            Assert.Equal($"331 Password required for {userName}.\r\n", cl.StringRecv(1, this));
            cl.StringSend($"PASS {userName}");
            Assert.Equal($"230 User {userName} logged in.\r\n", cl.StringRecv(10, this));
        }


        [Fact]
        public void パスワード認証成功_V4()
        {

            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("user user1");
            Assert.Equal("331 Password required for user1.\r\n", cl.StringRecv(1, this));
            cl.StringSend("PASS user1");
            Assert.Equal("230 User user1 logged in.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void パスワード認証成功_V6()
        {

            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("user user1");
            Assert.Equal("331 Password required for user1.\r\n", cl.StringRecv(1, this));
            cl.StringSend("PASS user1");
            Assert.Equal("230 User user1 logged in.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void アノニマス認証成功_V4()
        {

            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER Anonymous");
            Assert.Equal("331 Password required for Anonymous.\r\n", cl.StringRecv(1, this));
            cl.StringSend("PASS user@aaa.com");
            Assert.Equal("230 User Anonymous logged in.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void アノニマス認証成功_V6()
        {

            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER Anonymous");
            Assert.Equal("331 Password required for Anonymous.\r\n", cl.StringRecv(1, this));
            cl.StringSend("PASS user@aaa.com");
            Assert.Equal("230 User Anonymous logged in.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void アノニマス認証成功2_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER ANONYMOUS");
            Assert.Equal("331 Password required for ANONYMOUS.\r\n", cl.StringRecv(1, this));
            cl.StringSend("PASS xxx");
            Assert.Equal("230 User ANONYMOUS logged in.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void アノニマス認証成功2_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER ANONYMOUS");
            Assert.Equal("331 Password required for ANONYMOUS.\r\n", cl.StringRecv(1, this));
            cl.StringSend("PASS xxx");
            Assert.Equal("230 User ANONYMOUS logged in.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void パスワード認証失敗_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER user1");
            Assert.Equal("331 Password required for user1.\r\n", cl.StringRecv(1, this));
            cl.StringSend("PASS xxxx");
            Assert.Equal("530 Login incorrect.\r\n", cl.StringRecv(10, this));

        }

        [Fact]
        public void パスワード認証失敗_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER user1");
            Assert.Equal("331 Password required for user1.\r\n", cl.StringRecv(1, this));
            cl.StringSend("PASS xxxx");
            Assert.Equal("530 Login incorrect.\r\n", cl.StringRecv(10, this));
        }

        [Fact]
        public void USERの前にPASSコマンドを送るとエラーが返る_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック

            cl.StringSend("PASS user1");
            Assert.Equal("503 Login with USER first.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void USERの前にPASSコマンドを送るとエラーが返る_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック

            cl.StringSend("PASS user1");
            Assert.Equal("503 Login with USER first.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void パラメータが必要なコマンドにパラメータ指定が無かった場合エラーが返る_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER");
            Assert.Equal("500 USER: command requires a parameter.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void パラメータが必要なコマンドにパラメータ指定が無かった場合エラーが返る_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER");
            Assert.Equal("500 USER: command requires a parameter.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void 無効なコマンドでエラーが返る_V4()
        {
            var cl = _v4Cl;
            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("xxx");
            Assert.Equal("500 Command not understood.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void 無効なコマンドでエラーが返る_V6()
        {
            var cl = _v6Cl;
            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("xxx");
            Assert.Equal("500 Command not understood.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void 空行を送るとエラーが返る_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("");
            Assert.Equal("500 Invalid command: try being more creative.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void 空行を送るとエラーが返る_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("");
            Assert.Equal("500 Invalid command: try being more creative.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void 認証前に無効なコマンド_list_を送るとエラーが返る_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("LIST");
            Assert.Equal("530 Please login with USER and PASS.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void 認証前に無効なコマンド_list_を送るとエラーが返る_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("LIST");
            Assert.Equal("530 Please login with USER and PASS.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void 認証前に無効なコマンド_dele_を送るとエラーが返る_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("DELE");
            Assert.Equal("530 Please login with USER and PASS.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void 認証前に無効なコマンド_dele_を送るとエラーが返る_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("DELE");
            Assert.Equal("530 Please login with USER and PASS.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void 認証後にUSERコマンドを送るとエラーが返る_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //user
            cl.StringSend("USER user1");
            Assert.Equal("530 Already logged in.\r\n", cl.StringRecv(1, this));

        }
        [Fact]
        public void 認証後にUSERコマンドを送るとエラーが返る_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //user
            cl.StringSend("USER user1");
            Assert.Equal("530 Already logged in.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void 認証後にPASSコマンドを送るとエラーが返る_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //pass
            cl.StringSend("PASS user1");
            Assert.Equal("530 Already logged in.\r\n", cl.StringRecv(1, this));

        }
        [Fact]
        public void 認証後にPASSコマンドを送るとエラーが返る_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //pass
            cl.StringSend("PASS user1");
            Assert.Equal("530 Already logged in.\r\n", cl.StringRecv(1, this));

        }


        public bool IsLife()
        {
            return true;
        }
    }
}