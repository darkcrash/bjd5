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
    public class FtpServerTestDownUser : ILife, IDisposable, IClassFixture<FtpServerTestDownUser.InternalFixture>
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

        private FtpServerTestDownUser.InternalFixture _fixture;

        public FtpServerTestDownUser(FtpServerTestDownUser.InternalFixture fixture)
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
        public void DOWNユーザはSTORに失敗する_V4()
        {
            var cl = _v4Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user3", cl);

            //port
            var port = _fixture._service.GetAvailablePort(IpKind.V4Localhost, 20549);
            cl.StringSend($"PORT 127,0,0,1,0,{port}");
            var dl = SockUtil.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal("200 PORT command successful.\r\n", cl.StringRecv(1, this));

            //stor
            cl.StringSend("STOR 0.txt");
            Assert.Equal("550 Permission denied.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void DOWNユーザはSTORに失敗する_V6()
        {
            var cl = _v6Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user3", cl);

            //port
            var port = _fixture._service.GetAvailablePort(IpKind.V4Localhost, 20548);
            cl.StringSend($"PORT 127,0,0,1,0,{port}");
            var dl = SockUtil.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal("200 PORT command successful.\r\n", cl.StringRecv(1, this));

            //stor
            cl.StringSend("STOR 0.txt");
            Assert.Equal("550 Permission denied.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void DOWNユーザはDELEに失敗する_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user3", cl);

            //dele
            cl.StringSend("DELE 1.txt");
            Assert.Equal("550 Permission denied.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void DOWNユーザはDELEに失敗する_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user3", cl);

            //dele
            cl.StringSend("DELE 1.txt");
            Assert.Equal("550 Permission denied.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void DOWNユーザはRETRに成功する_V4()
        {
            var cl = _v4Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user3", cl);

            //port
            var port = _fixture._service.GetAvailablePort(IpKind.V4Localhost, 20650);
            cl.StringSend($"PORT 127,0,0,1,0,{port}");
            var dl = SockUtil.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal("200 PORT command successful.\r\n", cl.StringRecv(1, this));

            //retr
            cl.StringSend("RETR 3.txt");
            Assert.Equal("150 Opening ASCII mode data connection for 3.txt (24 bytes).\r\n", cl.StringRecv(1, this));

            for (var i = 0; i < 100; i++)
            {
                Thread.Sleep(10);
                if (dl.Length() >= 24) break;
            }
            Assert.Equal(24, dl.Length());

            dl.Close();
        }

        [Fact]
        public void DOWNユーザはRETRに成功する_V6()
        {
            var cl = _v6Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user3", cl);

            //port
            var port = _fixture._service.GetAvailablePort(IpKind.V4Localhost, 20651);
            cl.StringSend($"PORT 127,0,0,1,0,{port}");
            var dl = SockUtil.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal("200 PORT command successful.\r\n", cl.StringRecv(1, this));

            //retr
            cl.StringSend("RETR 3.txt");
            Assert.Equal("150 Opening ASCII mode data connection for 3.txt (24 bytes).\r\n", cl.StringRecv(1, this));

            for (var i = 0; i < 100; i++)
            {
                Thread.Sleep(10);
                if (dl.Length() >= 24) break;
            }
            Assert.Equal(24, dl.Length());

            dl.Close();
        }

        [Fact]
        public void DOWNユーザはRNFR_RNTO_ファイル名変更_に失敗する_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user3", cl);

            cl.StringSend("RNFR 1.txt");
            Assert.Equal("550 Permission denied.\r\n", cl.StringRecv(1, this));

            cl.StringSend("RNTO $$$.1.txt");
            Assert.Equal("550 Permission denied.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void DOWNユーザはRNFR_RNTO_ファイル名変更_に失敗する_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user3", cl);

            cl.StringSend("RNFR 1.txt");
            Assert.Equal("550 Permission denied.\r\n", cl.StringRecv(1, this));

            cl.StringSend("RNTO $$$.1.txt");
            Assert.Equal("550 Permission denied.\r\n", cl.StringRecv(1, this));

        }


        public bool IsLife()
        {
            return true;
        }
    }
}