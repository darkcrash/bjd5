﻿using System;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Test;
using Bjd.FtpServer;
using Xunit;
using Bjd.Services;

namespace FtpServerTest
{

    //[TestFixture]
    public class ServerTest : ILife, IDisposable, IClassFixture<ServerTest.InternalFixture>
    {
        public class InternalFixture : IDisposable
        {
            public TestService _service;
            public Server _v6Sv; //サーバ
            public Server _v4Sv; //サーバ

            //[TestFixtureSetUp]
            public InternalFixture()
            {
                _service = TestService.CreateTestService();
                _service.SetOption("FtpServerTest.ini");
                _service.ContentDirectory("TestDir");

                Kernel kernel = _service.Kernel;
                var option = kernel.ListOption.Get("Ftp");
                Conf conf = new Conf(option);

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


        private SockTcp _v6Cl; //クライアント
        private SockTcp _v4Cl; //クライアント

        private ServerTest.InternalFixture _fixture;

        public ServerTest(ServerTest.InternalFixture fixture)
        {
            _fixture = fixture;
            var kernel = _fixture._service.Kernel;

            //クライアント起動
            _v4Cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), 2021, 10, null);
            _v6Cl = Inet.Connect(kernel, new Ip(IpKind.V6Localhost), 2021, 10, null);
            //クライアントの接続が完了するまで、少し時間がかかる
            //Thread.Sleep(10);

        }

        public void Dispose()
        {
            //クライアント停止
            _v4Cl.Close();
            _v6Cl.Close();
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
        private void Login(string userName, SockTcp cl)
        {
            var banner = cl.StringRecv(1, this);
            CheckBanner(banner);//バナーチェック

            cl.StringSend(string.Format("USER {0}", userName));
            Assert.Equal(cl.StringRecv(1, this), string.Format("331 Password required for {0}.\r\n", userName));
            cl.StringSend(string.Format("PASS {0}", userName));
            Assert.Equal(cl.StringRecv(10, this), string.Format("230 User {0} logged in.\r\n", userName));
        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V4()
        {

            var sv = this._fixture._v4Sv;
            var expected = "+ サービス中 \t                 Ftp\t[127.0.0.1\t:TCP 2021]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 58);
            //verify
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V6()
        {

            var sv = this._fixture._v6Sv;
            var expected = "+ サービス中 \t                 Ftp\t[::1\t:TCP 2021]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 52);
            //verify
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void パスワード認証成功_V4()
        {

            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("user user1");
            Assert.Equal(cl.StringRecv(1, this), "331 Password required for user1.\r\n");
            cl.StringSend("PASS user1");
            Assert.Equal(cl.StringRecv(1, this), "230 User user1 logged in.\r\n");

        }

        [Fact]
        public void パスワード認証成功_V6()
        {

            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("user user1");
            Assert.Equal(cl.StringRecv(1, this), "331 Password required for user1.\r\n");
            cl.StringSend("PASS user1");
            Assert.Equal(cl.StringRecv(1, this), "230 User user1 logged in.\r\n");
        }

        [Fact]
        public void アノニマス認証成功_V4()
        {

            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER Anonymous");
            Assert.Equal(cl.StringRecv(1, this), "331 Password required for Anonymous.\r\n");
            cl.StringSend("PASS user@aaa.com");
            Assert.Equal(cl.StringRecv(1, this), "230 User Anonymous logged in.\r\n");

        }

        [Fact]
        public void アノニマス認証成功_V6()
        {

            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER Anonymous");
            Assert.Equal(cl.StringRecv(1, this), "331 Password required for Anonymous.\r\n");
            cl.StringSend("PASS user@aaa.com");
            Assert.Equal(cl.StringRecv(1, this), "230 User Anonymous logged in.\r\n");

        }

        [Fact]
        public void アノニマス認証成功2_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER ANONYMOUS");
            Assert.Equal(cl.StringRecv(1, this), "331 Password required for ANONYMOUS.\r\n");
            cl.StringSend("PASS xxx");
            Assert.Equal(cl.StringRecv(1, this), "230 User ANONYMOUS logged in.\r\n");

        }

        [Fact]
        public void アノニマス認証成功2_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER ANONYMOUS");
            Assert.Equal(cl.StringRecv(1, this), "331 Password required for ANONYMOUS.\r\n");
            cl.StringSend("PASS xxx");
            Assert.Equal(cl.StringRecv(1, this), "230 User ANONYMOUS logged in.\r\n");

        }

        [Fact]
        public void パスワード認証失敗_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER user1");
            Assert.Equal(cl.StringRecv(1, this), "331 Password required for user1.\r\n");
            cl.StringSend("PASS xxxx");
            Assert.Equal(cl.StringRecv(10, this), "530 Login incorrect.\r\n");
        }

        [Fact]
        public void パスワード認証失敗_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER user1");
            Assert.Equal(cl.StringRecv(1, this), "331 Password required for user1.\r\n");
            cl.StringSend("PASS xxxx");
            Assert.Equal(cl.StringRecv(10, this), "530 Login incorrect.\r\n");
        }

        [Fact]
        public void USERの前にPASSコマンドを送るとエラーが返る_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック

            cl.StringSend("PASS user1");
            Assert.Equal(cl.StringRecv(1, this), "503 Login with USER first.\r\n");

        }

        [Fact]
        public void USERの前にPASSコマンドを送るとエラーが返る_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック

            cl.StringSend("PASS user1");
            Assert.Equal(cl.StringRecv(1, this), "503 Login with USER first.\r\n");

        }

        [Fact]
        public void パラメータが必要なコマンドにパラメータ指定が無かった場合エラーが返る_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER");
            Assert.Equal(cl.StringRecv(1, this), "500 USER: command requires a parameter.\r\n");
        }

        [Fact]
        public void パラメータが必要なコマンドにパラメータ指定が無かった場合エラーが返る_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("USER");
            Assert.Equal(cl.StringRecv(1, this), "500 USER: command requires a parameter.\r\n");
        }

        [Fact]
        public void 無効なコマンドでエラーが返る_V4()
        {
            var cl = _v4Cl;
            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("xxx");
            Assert.Equal(cl.StringRecv(1, this), "500 Command not understood.\r\n");
        }

        [Fact]
        public void 無効なコマンドでエラーが返る_V6()
        {
            var cl = _v6Cl;
            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("xxx");
            Assert.Equal(cl.StringRecv(1, this), "500 Command not understood.\r\n");
        }

        [Fact]
        public void 空行を送るとエラーが返る_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("");
            Assert.Equal(cl.StringRecv(1, this), "500 Invalid command: try being more creative.\r\n");
        }

        [Fact]
        public void 空行を送るとエラーが返る_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("");
            Assert.Equal(cl.StringRecv(1, this), "500 Invalid command: try being more creative.\r\n");
        }

        [Fact]
        public void 認証前に無効なコマンド_list_を送るとエラーが返る_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("LIST");
            Assert.Equal(cl.StringRecv(1, this), "530 Please login with USER and PASS.\r\n");
        }

        [Fact]
        public void 認証前に無効なコマンド_list_を送るとエラーが返る_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("LIST");
            Assert.Equal(cl.StringRecv(1, this), "530 Please login with USER and PASS.\r\n");
        }

        [Fact]
        public void 認証前に無効なコマンド_dele_を送るとエラーが返る_V4()
        {
            var cl = _v4Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("DELE");
            Assert.Equal(cl.StringRecv(1, this), "530 Please login with USER and PASS.\r\n");
        }

        [Fact]
        public void 認証前に無効なコマンド_dele_を送るとエラーが返る_V6()
        {
            var cl = _v6Cl;

            CheckBanner(cl.StringRecv(1, this));//バナーチェック
            cl.StringSend("DELE");
            Assert.Equal(cl.StringRecv(1, this), "530 Please login with USER and PASS.\r\n");
        }

        [Fact]
        public void 認証後にUSERコマンドを送るとエラーが返る_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //user
            cl.StringSend("USER user1");
            Assert.Equal(cl.StringRecv(1, this), "530 Already logged in.\r\n");

        }
        [Fact]
        public void 認証後にUSERコマンドを送るとエラーが返る_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //user
            cl.StringSend("USER user1");
            Assert.Equal(cl.StringRecv(1, this), "530 Already logged in.\r\n");

        }

        [Fact]
        public void 認証後にPASSコマンドを送るとエラーが返る_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //pass
            cl.StringSend("PASS user1");
            Assert.Equal(cl.StringRecv(1, this), "530 Already logged in.\r\n");

        }
        [Fact]
        public void 認証後にPASSコマンドを送るとエラーが返る_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //pass
            cl.StringSend("PASS user1");
            Assert.Equal(cl.StringRecv(1, this), "530 Already logged in.\r\n");

        }

        [Fact]
        public void PWDコマンド_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //pwd
            cl.StringSend("PWD");
            Assert.Equal(cl.StringRecv(1, this), "257 \"/\" is current directory.\r\n");

        }

        [Fact]
        public void PWDコマンド_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //pwd
            cl.StringSend("PWD");
            Assert.Equal(cl.StringRecv(1, this), "257 \"/\" is current directory.\r\n");

        }

        [Fact]
        public void SYSTコマンド_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //syst
            cl.StringSend("SYST");

            // Assert.Equal(cl.StringRecv(1, this), "215 Microsoft Windows NT 6.2.9200.0\r\n");
            var actual = cl.StringRecv(1, this);
            Assert.Equal("215 Windows 10", actual.Substring(0, 14));

        }

        [Fact]
        public void SYSTコマンド_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //syst
            cl.StringSend("SYST");

            // Assert.Equal(cl.StringRecv(1, this), "215 Microsoft Windows NT 6.2.9200.0\r\n");
            var actual = cl.StringRecv(1, this);
            Assert.Equal("215 Windows 10", actual.Substring(0, 14));

        }

        [Fact]
        public void TYPEコマンド_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //type
            cl.StringSend("TYPE A");
            Assert.Equal(cl.StringRecv(1, this), "200 Type set 'A'\r\n");
            cl.StringSend("TYPE I");
            Assert.Equal(cl.StringRecv(1, this), "200 Type set 'I'\r\n");
            cl.StringSend("TYPE X");
            Assert.Equal(cl.StringRecv(1, this), "500 command not understood.\r\n");

        }

        [Fact]
        public void TYPEコマンド_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //type
            cl.StringSend("TYPE A");
            Assert.Equal(cl.StringRecv(1, this), "200 Type set 'A'\r\n");
            cl.StringSend("TYPE I");
            Assert.Equal(cl.StringRecv(1, this), "200 Type set 'I'\r\n");
            cl.StringSend("TYPE X");
            Assert.Equal(cl.StringRecv(1, this), "500 command not understood.\r\n");

        }
        [Fact]
        public void PORTコマンド()
        {
            var cl = _v4Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user1", cl);

            int port = 256; //テストの連続のためにPORTコマンドのテストとはポート番号をずらす必要がある
            cl.StringSend("PORT 127,0,0,1,0,256");
            SockTcp dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal(cl.StringRecv(1, this), "200 PORT command successful.\r\n");

            dl.Close();
        }

        [Fact]
        public void PORTコマンド_パラメータ誤り()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("PORT 127,3,x,x,1,0,256");
            Assert.Equal(cl.StringRecv(1, this), "501 Illegal PORT command.\r\n");

        }

        [Fact]
        public void PASVコマンド()
        {
            var cl = _v4Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("PASV");

            //227 Entering Passive Mode. (127,0,0,1,xx,xx)
            string[] t = cl.StringRecv(1, this).Split(new[] { '(', ')' });
            string[] tmp = t[1].Split(',');
            int n = Convert.ToInt32(tmp[4]);
            int m = Convert.ToInt32(tmp[5]);
            int port = n * 256 + m;

            Thread.Sleep(10);
            SockTcp dl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), port, 10, null);
            Assert.Equal(dl.SockState, SockState.Connect);
            dl.Close();
        }

        [Fact]
        public void EPSVコマンド()
        {
            var cl = _v6Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("EPSV");

            //229 Entering Extended Passive Mode. (|||xxxx|)
            var tmp = cl.StringRecv(1, this).Split('|');
            var port = Convert.ToInt32(tmp[3]);
            var dl = Inet.Connect(kernel, new Ip(IpKind.V6Localhost), port, 10, null);
            Assert.Equal(dl.SockState, SockState.Connect);
            dl.Close();
        }

        [Fact]
        public void EPRTコマンド()
        {
            var cl = _v6Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user1", cl);

            var port = 252; //テストの連続のためにPORTコマンドのテストとはポート番号をずらす必要がある
            cl.StringSend("EPRT |2|::1|252|");
            var dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V6Localhost), port, null, this);
            Assert.Equal(cl.StringRecv(1, this), "200 EPRT command successful.\r\n");

            dl.Close();
        }

        [Fact]
        public void EPORTコマンド_パラメータ誤り()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("EPRT |x|");
            Assert.Equal(cl.StringRecv(1, this), "501 Illegal EPRT command.\r\n");

        }

        [Fact]
        public void MKD_RMDコマンド_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("MKD test");
            Assert.Equal(cl.StringRecv(1, this), "257 Mkd command successful.\r\n");

            cl.StringSend("RMD test");
            Assert.Equal(cl.StringRecv(1, this), "250 Rmd command successful.\r\n");
        }
        [Fact]
        public void MKD_RMDコマンド_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("MKD test");
            Assert.Equal(cl.StringRecv(1, this), "257 Mkd command successful.\r\n");

            cl.StringSend("RMD test");
            Assert.Equal(cl.StringRecv(1, this), "250 Rmd command successful.\r\n");
        }

        [Fact]
        public void MKDコマンド_既存の名前を指定するとエラーとなる_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("MKD home0");
            Assert.Equal(cl.StringRecv(1, this), "451 Mkd error.\r\n");

        }

        [Fact]
        public void MKDコマンド_既存の名前を指定するとエラーとなる_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("MKD home0");
            Assert.Equal(cl.StringRecv(1, this), "451 Mkd error.\r\n");

        }

        [Fact]
        public void RMDコマンド_存在しない名前を指定するとエラーとなる_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RMD test");
            Assert.Equal(cl.StringRecv(1, this), "451 Rmd error.\r\n");

        }
        [Fact]
        public void RMDコマンド_存在しない名前を指定するとエラーとなる_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RMD test");
            Assert.Equal(cl.StringRecv(1, this), "451 Rmd error.\r\n");

        }

        [Fact]
        public void RETRコマンド_V4()
        {
            var cl = _v4Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //port
            var port = 20250;
            cl.StringSend("PORT 127,0,0,1,0,20250");
            var dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal(cl.StringRecv(1, this), "200 PORT command successful.\r\n");

            //retr
            cl.StringSend("RETR 3.txt");
            Assert.Equal("150 Opening ASCII mode data connection for 3.txt (24 bytes).\r\n", cl.StringRecv(1, this));
            Thread.Sleep(10);
            Assert.Equal(dl.Length(), 24);

            dl.Close();
        }


        [Fact]
        public void RETRコマンド_V6()
        {
            var cl = _v6Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //port
            var port = 20350;
            cl.StringSend("PORT 127,0,0,1,0,20350");
            var dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal(cl.StringRecv(1, this), "200 PORT command successful.\r\n");

            //retr
            cl.StringSend("RETR 3.txt");
            Assert.Equal(cl.StringRecv(1, this), "150 Opening ASCII mode data connection for 3.txt (24 bytes).\r\n");
            Thread.Sleep(10);
            Assert.Equal(dl.Length(), 24);

            dl.Close();
        }
        [Fact]
        public void STOR_DELEマンド_V4()
        {
            var cl = _v4Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //port
            var port = 20249;
            cl.StringSend("PORT 127,0,0,1,0,20249");
            var dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal("200 PORT command successful.\r\n", cl.StringRecv(2, this));

            //stor
            cl.StringSend("STOR 0.txt");
            Assert.Equal("150 Opening ASCII mode data connection for 0.txt.\r\n", cl.StringRecv(2, this));

            dl.Send(new byte[3]);
            dl.Close();

            Assert.Equal("226 Transfer complete.\r\n", cl.StringRecv(1, this));

            //dele
            cl.StringSend("DELE 0.txt");
            Assert.Equal("250 Dele command successful.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void STOR_DELEマンド_V6()
        {
            var cl = _v6Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //port
            var port = 20349;
            cl.StringSend("PORT 127,0,0,1,0,20349");
            var dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal(cl.StringRecv(1, this), "200 PORT command successful.\r\n");

            //stor
            cl.StringSend("STOR 0.txt");
            Assert.Equal(cl.StringRecv(1, this), "150 Opening ASCII mode data connection for 0.txt.\r\n");

            dl.Send(new byte[3]);
            dl.Close();

            Assert.Equal(cl.StringRecv(1, this), "226 Transfer complete.\r\n");

            //dele
            cl.StringSend("DELE 0.txt");
            Assert.Equal(cl.StringRecv(1, this), "250 Dele command successful.\r\n");

        }


        [Fact]
        public void UPユーザはRETRに失敗する_V4()
        {
            var cl = _v4Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user2", cl);

            //port
            var port = 20450;
            cl.StringSend("PORT 127,0,0,1,0,20450");
            var dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal(cl.StringRecv(1, this), "200 PORT command successful.\r\n");

            //retr
            cl.StringSend("RETR 3.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");
            //		Thread.Sleep(10);
            //		Assert.Equal(dl.Length, 24);

            dl.Close();
        }



        [Fact]
        public void UPユーザはRETRに失敗する_V6()
        {
            var cl = _v6Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user2", cl);

            //port
            var port = 20451;
            cl.StringSend("PORT 127,0,0,1,0,20451");
            var dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal(cl.StringRecv(1, this), "200 PORT command successful.\r\n");

            //retr
            cl.StringSend("RETR 3.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");
            //		Thread.Sleep(10);
            //		Assert.Equal(dl.Length, 24);

            dl.Close();
        }

        [Fact]
        public void UPユーザはDELEに失敗する_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user2", cl);

            //dele
            cl.StringSend("DELE 1.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

        }

        [Fact]
        public void UPユーザはDELEに失敗する_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user2", cl);

            //dele
            cl.StringSend("DELE 1.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

        }

        [Fact]
        public void UPユーザはRNFR_RNTO_ファイル名変更_に失敗する_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user2", cl);

            cl.StringSend("RNFR 1.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

            cl.StringSend("RNTO $$$.1.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

        }

        [Fact]
        public void UPユーザはRNFR_RNTO_ファイル名変更_に失敗する_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user2", cl);

            cl.StringSend("RNFR 1.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

            cl.StringSend("RNTO $$$.1.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

        }

        [Fact]
        public void DOWNユーザはSTORに失敗する_V4()
        {
            var cl = _v4Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user3", cl);

            //port
            var port = 20549;
            cl.StringSend("PORT 127,0,0,1,0,20549");
            var dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal(cl.StringRecv(1, this), "200 PORT command successful.\r\n");

            //stor
            cl.StringSend("STOR 0.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

        }

        [Fact]
        public void DOWNユーザはSTORに失敗する_V6()
        {
            var cl = _v6Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user3", cl);

            //port
            var port = 20548;
            cl.StringSend("PORT 127,0,0,1,0,20548");
            var dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal(cl.StringRecv(1, this), "200 PORT command successful.\r\n");

            //stor
            cl.StringSend("STOR 0.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

        }

        [Fact]
        public void DOWNユーザはDELEに失敗する_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user3", cl);

            //dele
            cl.StringSend("DELE 1.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

        }

        [Fact]
        public void DOWNユーザはDELEに失敗する_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user3", cl);

            //dele
            cl.StringSend("DELE 1.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

        }

        [Fact]
        public void DOWNユーザはRETRに成功する_V4()
        {
            var cl = _v4Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user3", cl);

            //port
            var port = 20650;
            cl.StringSend("PORT 127,0,0,1,0,20650");
            var dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal(cl.StringRecv(1, this), "200 PORT command successful.\r\n");

            //retr
            cl.StringSend("RETR 3.txt");
            Assert.Equal(cl.StringRecv(1, this), "150 Opening ASCII mode data connection for 3.txt (24 bytes).\r\n");

            Thread.Sleep(10);
            Assert.Equal(dl.Length(), 24);

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
            var port = 20651;
            cl.StringSend("PORT 127,0,0,1,0,20651");
            var dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal(cl.StringRecv(1, this), "200 PORT command successful.\r\n");

            //retr
            cl.StringSend("RETR 3.txt");
            Assert.Equal(cl.StringRecv(1, this), "150 Opening ASCII mode data connection for 3.txt (24 bytes).\r\n");

            Thread.Sleep(10);
            Assert.Equal(dl.Length(), 24);

            dl.Close();
        }

        [Fact]
        public void DOWNユーザはRNFR_RNTO_ファイル名変更_に失敗する_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user3", cl);

            cl.StringSend("RNFR 1.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

            cl.StringSend("RNTO $$$.1.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

        }

        [Fact]
        public void DOWNユーザはRNFR_RNTO_ファイル名変更_に失敗する_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user3", cl);

            cl.StringSend("RNFR 1.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

            cl.StringSend("RNTO $$$.1.txt");
            Assert.Equal(cl.StringRecv(1, this), "550 Permission denied.\r\n");

        }

        [Fact]
        public void DELEマンド_存在しない名前を指定するとエラーとなる_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //dele
            cl.StringSend("DELE 99.txt");
            Assert.Equal(cl.StringRecv(1, this), "451 Dele error.\r\n");

        }

        [Fact]
        public void DELEマンド_存在しない名前を指定するとエラーとなる_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //dele
            cl.StringSend("DELE 99.txt");
            Assert.Equal(cl.StringRecv(1, this), "451 Dele error.\r\n");

        }

        [Fact]
        public void LISTコマンド_V4()
        {
            var cl = _v4Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //port
            var port = 20751;
            cl.StringSend("PORT 127,0,0,1,0,20751");
            var dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal(cl.StringRecv(1, this), "200 PORT command successful.\r\n");

            //list
            cl.StringSend("LIST -la");
            Assert.Equal(cl.StringRecv(1, this), "150 Opening ASCII mode data connection for ls.\r\n");

            Assert.Equal(listMask(dl.StringRecv(3, this)), "drwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm home0\r\n");
            Assert.Equal(listMask(dl.StringRecv(3, this)), "drwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm home1\r\n");
            Assert.Equal(listMask(dl.StringRecv(3, this)), "drwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm home2\r\n");
            Assert.Equal(listMask(dl.StringRecv(3, this)), "-rwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm 1.txt\r\n");
            Assert.Equal(listMask(dl.StringRecv(3, this)), "-rwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm 2.txt\r\n");
            Assert.Equal(listMask(dl.StringRecv(3, this)), "-rwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm 3.txt\r\n");

            dl.Close();
        }

        [Fact]
        public void LISTコマンド_V6()
        {
            var cl = _v6Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //port
            var port = 20752;
            cl.StringSend("PORT 127,0,0,1,0,20752");
            var dl = SockServerTcp.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal(cl.StringRecv(1, this), "200 PORT command successful.\r\n");

            //list
            cl.StringSend("LIST -la");
            Assert.Equal(cl.StringRecv(1, this), "150 Opening ASCII mode data connection for ls.\r\n");

            Assert.Equal(listMask(dl.StringRecv(3, this)), "drwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm home0\r\n");
            Assert.Equal(listMask(dl.StringRecv(3, this)), "drwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm home1\r\n");
            Assert.Equal(listMask(dl.StringRecv(3, this)), "drwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm home2\r\n");
            Assert.Equal(listMask(dl.StringRecv(3, this)), "-rwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm 1.txt\r\n");
            Assert.Equal(listMask(dl.StringRecv(3, this)), "-rwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm 2.txt\r\n");
            Assert.Equal(listMask(dl.StringRecv(3, this)), "-rwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm 3.txt\r\n");

            dl.Close();
        }
        private string listMask(string str)
        {
            var tmp = str.Split(' ');
            return string.Format("{0} {1} {2} {3} nnnn mon dd hh:mm {4}", tmp[0], tmp[1], tmp[2], tmp[3], tmp[8]);
        }

        [Fact]
        public void CWDコマンドで有効なディレクトリに移動_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //cwd
            cl.StringSend("CWD home0");
            Assert.Equal(cl.StringRecv(1, this), "250 CWD command successful.\r\n");

        }
        [Fact]
        public void CWDコマンドで有効なディレクトリに移動_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //cwd
            cl.StringSend("CWD home0");
            Assert.Equal(cl.StringRecv(1, this), "250 CWD command successful.\r\n");

        }
        [Fact]
        public void CWDコマンドで無効なディレクトリに移動しようとするとエラーが返る_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //cwd
            cl.StringSend("CWD xxx");
            Assert.Equal(cl.StringRecv(1, this), "550 xxx: No such file or directory.\r\n");
            cl.StringSend("PWD");

        }
        [Fact]
        public void CWDコマンドで無効なディレクトリに移動しようとするとエラーが返る_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //cwd
            cl.StringSend("CWD xxx");
            Assert.Equal(cl.StringRecv(1, this), "550 xxx: No such file or directory.\r\n");
            cl.StringSend("PWD");

        }
        [Fact]
        public void CWDコマンドでルートより上に移動しようとするとエラーが返る_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);


            //cwd
            cl.StringSend("CWD home0");
            Assert.Equal(cl.StringRecv(1, this), "250 CWD command successful.\r\n");
            cl.StringSend("CWD ..\\..");
            Assert.Equal(cl.StringRecv(1, this), "550 ..\\..: No such file or directory.\r\n");

        }

        [Fact]
        public void CWDコマンドでルートより上に移動しようとするとエラーが返る_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);


            //cwd
            cl.StringSend("CWD home0");
            Assert.Equal(cl.StringRecv(1, this), "250 CWD command successful.\r\n");
            cl.StringSend("CWD ..\\..");
            Assert.Equal(cl.StringRecv(1, this), "550 ..\\..: No such file or directory.\r\n");

        }
        [Fact]
        public void CDUPコマンド_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //cwd
            cl.StringSend("CWD home0");
            Assert.Equal(cl.StringRecv(1, this), "250 CWD command successful.\r\n");
            //cdup
            cl.StringSend("CDUP");
            Assert.Equal(cl.StringRecv(1, this), "250 CWD command successful.\r\n");
            //pwd ルートに戻っていることを確認する
            cl.StringSend("PWD");
            Assert.Equal(cl.StringRecv(1, this), "257 \"/\" is current directory.\r\n");

        }
        [Fact]
        public void CDUPコマンド_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //cwd
            cl.StringSend("CWD home0");
            Assert.Equal(cl.StringRecv(1, this), "250 CWD command successful.\r\n");
            //cdup
            cl.StringSend("CDUP");
            Assert.Equal(cl.StringRecv(1, this), "250 CWD command successful.\r\n");
            //pwd ルートに戻っていることを確認する
            cl.StringSend("PWD");
            Assert.Equal(cl.StringRecv(1, this), "257 \"/\" is current directory.\r\n");

        }

        [Fact]
        public void RNFR_RNTOコマンド_ファイル名変更_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RNFR 1.txt");
            Assert.Equal(cl.StringRecv(1, this), "350 File exists, ready for destination name.\r\n");

            cl.StringSend("RNTO $$$.1.txt");
            Assert.Equal(cl.StringRecv(1, this), "250 RNTO command successful.\r\n");

            cl.StringSend("RNFR $$$.1.txt");
            Assert.Equal(cl.StringRecv(1, this), "350 File exists, ready for destination name.\r\n");

            cl.StringSend("RNTO 1.txt");
            Assert.Equal(cl.StringRecv(1, this), "250 RNTO command successful.\r\n");
        }
        [Fact]
        public void RNFR_RNTOコマンド_ファイル名変更_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RNFR 1.txt");
            Assert.Equal(cl.StringRecv(1, this), "350 File exists, ready for destination name.\r\n");

            cl.StringSend("RNTO $$$.1.txt");
            Assert.Equal(cl.StringRecv(1, this), "250 RNTO command successful.\r\n");

            cl.StringSend("RNFR $$$.1.txt");
            Assert.Equal(cl.StringRecv(1, this), "350 File exists, ready for destination name.\r\n");

            cl.StringSend("RNTO 1.txt");
            Assert.Equal(cl.StringRecv(1, this), "250 RNTO command successful.\r\n");
        }


        [Fact]
        public void RNFR_RNTOコマンド_ディレクトリ名変更_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RNFR home0");
            Assert.Equal(cl.StringRecv(1, this), "350 File exists, ready for destination name.\r\n");

            cl.StringSend("RNTO $$$.home0");
            Assert.Equal(cl.StringRecv(1, this), "250 RNTO command successful.\r\n");

            cl.StringSend("RNFR $$$.home0");
            Assert.Equal(cl.StringRecv(1, this), "350 File exists, ready for destination name.\r\n");

            cl.StringSend("RNTO home0");
            Assert.Equal(cl.StringRecv(1, this), "250 RNTO command successful.\r\n");
        }

        [Fact]
        public void RNFR_RNTOコマンド_ディレクトリ名変更_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RNFR home0");
            Assert.Equal(cl.StringRecv(1, this), "350 File exists, ready for destination name.\r\n");

            cl.StringSend("RNTO $$$.home0");
            Assert.Equal(cl.StringRecv(1, this), "250 RNTO command successful.\r\n");

            cl.StringSend("RNFR $$$.home0");
            Assert.Equal(cl.StringRecv(1, this), "350 File exists, ready for destination name.\r\n");

            cl.StringSend("RNTO home0");
            Assert.Equal(cl.StringRecv(1, this), "250 RNTO command successful.\r\n");
        }

        [Fact]
        public void RMDコマンド_空でないディレクトリの削除は失敗する_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RMD home0");
            Assert.Equal(cl.StringRecv(1, this), "451 Rmd error.\r\n");

        }

        [Fact]
        public void RMDコマンド_空でないディレクトリの削除は失敗する_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RMD home0");
            Assert.Equal(cl.StringRecv(1, this), "451 Rmd error.\r\n");

        }

        public bool IsLife()
        {
            return true;
        }
    }
}