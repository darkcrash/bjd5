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
    public class FtpServerTest : ILife, IDisposable, IClassFixture<FtpServerTest.InternalFixture>
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

        private FtpServerTest.InternalFixture _fixture;

        public FtpServerTest(FtpServerTest.InternalFixture fixture)
        {
            _fixture = fixture;
            var kernel = _fixture._service.Kernel;
            var port = _fixture.port;

            //クライアント起動
            _v4Cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), port, 5, null);
            _v6Cl = Inet.Connect(kernel, new Ip(IpKind.V6Localhost), port, 5, null);
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
        public void ステータス情報_ToString_の出力確認_V4()
        {

            var sv = this._fixture._v4Sv;
            var expected = $"+ サービス中 \t                 Ftp\t[127.0.0.1\t:TCP {_fixture.port}]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 58);
            //verify
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V6()
        {

            var sv = this._fixture._v6Sv;
            var expected = $"+ サービス中 \t                 Ftp\t[::1\t:TCP {_fixture.port}]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 52);
            //verify
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void PWDコマンド_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //pwd
            cl.StringSend("PWD");
            Assert.Equal("257 \"/\" is current directory.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void PWDコマンド_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //pwd
            cl.StringSend("PWD");
            Assert.Equal("257 \"/\" is current directory.\r\n", cl.StringRecv(1, this));

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
            Assert.Equal($"215 {_fixture._service.Kernel.Enviroment.OperatingSystem}\r\n", actual);

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
            Assert.Equal($"215 {_fixture._service.Kernel.Enviroment.OperatingSystem}\r\n", actual);

        }

        [Fact]
        public void TYPEコマンド_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //type
            cl.StringSend("TYPE A");
            Assert.Equal("200 Type set 'A'\r\n", cl.StringRecv(1, this));
            cl.StringSend("TYPE I");
            Assert.Equal("200 Type set 'I'\r\n", cl.StringRecv(1, this));
            cl.StringSend("TYPE X");
            Assert.Equal("500 command not understood.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void TYPEコマンド_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //type
            cl.StringSend("TYPE A");
            Assert.Equal("200 Type set 'A'\r\n", cl.StringRecv(1, this));
            cl.StringSend("TYPE I");
            Assert.Equal("200 Type set 'I'\r\n", cl.StringRecv(1, this));
            cl.StringSend("TYPE X");
            Assert.Equal("500 command not understood.\r\n", cl.StringRecv(1, this));

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
            ISocket dl = SockUtil.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal("200 PORT command successful.\r\n", cl.StringRecv(1, this));

            dl.Close();
        }

        [Fact]
        public void PORTコマンド_パラメータ誤り()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("PORT 127,3,x,x,1,0,256");
            Assert.Equal("501 Illegal PORT command.\r\n", cl.StringRecv(1, this));

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
            ISocket dl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), port, 10, null);
            Assert.Equal(SockState.Connect, dl.SockState);
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
            Assert.Equal(SockState.Connect, dl.SockState);
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
            var dl = SockUtil.CreateConnection(kernel, new Ip(IpKind.V6Localhost), port, null, this);
            Assert.Equal("200 EPRT command successful.\r\n", cl.StringRecv(1, this));

            dl.Close();
        }

        [Fact]
        public void EPORTコマンド_パラメータ誤り()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("EPRT |x|");
            Assert.Equal("501 Illegal EPRT command.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void MKD_RMDコマンド_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("MKD test");
            Assert.Equal("257 Mkd command successful.\r\n", cl.StringRecv(1, this));

            cl.StringSend("RMD test");
            Assert.Equal("250 Rmd command successful.\r\n", cl.StringRecv(1, this));
        }
        [Fact]
        public void MKD_RMDコマンド_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("MKD test");
            Assert.Equal("257 Mkd command successful.\r\n", cl.StringRecv(1, this));

            cl.StringSend("RMD test");
            Assert.Equal("250 Rmd command successful.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void MKDコマンド_既存の名前を指定するとエラーとなる_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("MKD home0");
            Assert.Equal("451 Mkd error.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void MKDコマンド_既存の名前を指定するとエラーとなる_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("MKD home0");
            Assert.Equal("451 Mkd error.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void RMDコマンド_存在しない名前を指定するとエラーとなる_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RMD test");
            Assert.Equal("451 Rmd error.\r\n", cl.StringRecv(1, this));

        }
        [Fact]
        public void RMDコマンド_存在しない名前を指定するとエラーとなる_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RMD test");
            Assert.Equal("451 Rmd error.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void DELEマンド_存在しない名前を指定するとエラーとなる_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //dele
            cl.StringSend("DELE 99.txt");
            Assert.Equal("451 Dele error.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void DELEマンド_存在しない名前を指定するとエラーとなる_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //dele
            cl.StringSend("DELE 99.txt");
            Assert.Equal("451 Dele error.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void LISTコマンド_V4()
        {
            var cl = _v4Cl;
            var kernel = _fixture._service.Kernel;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //port
            var port = _fixture._service.GetAvailablePort(IpKind.V4Localhost, 20751);
            cl.StringSend($"PORT 127,0,0,1,0,{port}");
            var dl = SockUtil.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal("200 PORT command successful.\r\n", cl.StringRecv(1, this));

            //list
            cl.StringSend("LIST -la");
            Assert.Equal("150 Opening ASCII mode data connection for ls.\r\n", cl.StringRecv(1, this));

            Assert.Equal("drwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm home0\r\n", listMask(dl.StringRecv(3, this)));
            Assert.Equal("drwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm home1\r\n", listMask(dl.StringRecv(3, this)));
            Assert.Equal("drwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm home2\r\n", listMask(dl.StringRecv(3, this)));
            Assert.Equal("-rwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm 1.txt\r\n", listMask(dl.StringRecv(3, this)));
            Assert.Equal("-rwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm 2.txt\r\n", listMask(dl.StringRecv(3, this)));
            Assert.Equal("-rwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm 3.txt\r\n", listMask(dl.StringRecv(3, this)));

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
            var port = _fixture._service.GetAvailablePort(IpKind.V4Localhost, 20752);
            cl.StringSend($"PORT 127,0,0,1,0,{port}");
            var dl = SockUtil.CreateConnection(kernel, new Ip(IpKind.V4Localhost), port, null, this);
            Assert.Equal("200 PORT command successful.\r\n", cl.StringRecv(1, this));

            //list
            cl.StringSend("LIST -la");
            Assert.Equal("150 Opening ASCII mode data connection for ls.\r\n", cl.StringRecv(1, this));

            Assert.Equal("drwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm home0\r\n", listMask(dl.StringRecv(3, this)));
            Assert.Equal("drwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm home1\r\n", listMask(dl.StringRecv(3, this)));
            Assert.Equal("drwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm home2\r\n", listMask(dl.StringRecv(3, this)));
            Assert.Equal("-rwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm 1.txt\r\n", listMask(dl.StringRecv(3, this)));
            Assert.Equal("-rwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm 2.txt\r\n", listMask(dl.StringRecv(3, this)));
            Assert.Equal("-rwxrwxrwx 1 nobody nogroup nnnn mon dd hh:mm 3.txt\r\n", listMask(dl.StringRecv(3, this)));

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
            Assert.Equal("250 CWD command successful.\r\n", cl.StringRecv(1, this));

        }
        [Fact]
        public void CWDコマンドで有効なディレクトリに移動_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //cwd
            cl.StringSend("CWD home0");
            Assert.Equal("250 CWD command successful.\r\n", cl.StringRecv(1, this));

        }
        [Fact]
        public void CWDコマンドで無効なディレクトリに移動しようとするとエラーが返る_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //cwd
            cl.StringSend("CWD xxx");
            Assert.Equal("550 xxx: No such file or directory.\r\n", cl.StringRecv(1, this));
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
            Assert.Equal("550 xxx: No such file or directory.\r\n", cl.StringRecv(1, this));
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
            Assert.Equal("250 CWD command successful.\r\n", cl.StringRecv(1, this));
            cl.StringSend("CWD ..\\..");
            Assert.Equal("550 ..\\..: No such file or directory.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void CWDコマンドでルートより上に移動しようとするとエラーが返る_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);


            //cwd
            cl.StringSend("CWD home0");
            Assert.Equal("250 CWD command successful.\r\n", cl.StringRecv(1, this));
            cl.StringSend("CWD ..\\..");
            Assert.Equal("550 ..\\..: No such file or directory.\r\n", cl.StringRecv(1, this));

        }
        [Fact]
        public void CDUPコマンド_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //cwd
            cl.StringSend("CWD home0");
            Assert.Equal("250 CWD command successful.\r\n", cl.StringRecv(1, this));
            //cdup
            cl.StringSend("CDUP");
            Assert.Equal("250 CWD command successful.\r\n", cl.StringRecv(1, this));
            //pwd ルートに戻っていることを確認する
            cl.StringSend("PWD");
            Assert.Equal("257 \"/\" is current directory.\r\n", cl.StringRecv(1, this));

        }
        [Fact]
        public void CDUPコマンド_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            //cwd
            cl.StringSend("CWD home0");
            Assert.Equal("250 CWD command successful.\r\n", cl.StringRecv(1, this));
            //cdup
            cl.StringSend("CDUP");
            Assert.Equal("250 CWD command successful.\r\n", cl.StringRecv(1, this));
            //pwd ルートに戻っていることを確認する
            cl.StringSend("PWD");
            Assert.Equal("257 \"/\" is current directory.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void RNFR_RNTOコマンド_ファイル名変更_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RNFR 1.txt");
            Assert.Equal("350 File exists, ready for destination name.\r\n", cl.StringRecv(2, this));

            cl.StringSend("RNTO $$$.1.txt");
            Assert.Equal("250 RNTO command successful.\r\n", cl.StringRecv(2, this));

            cl.StringSend("RNFR $$$.1.txt");
            Assert.Equal("350 File exists, ready for destination name.\r\n", cl.StringRecv(2, this));

            cl.StringSend("RNTO 1.txt");
            Assert.Equal("250 RNTO command successful.\r\n", cl.StringRecv(2, this));
        }
        [Fact]
        public void RNFR_RNTOコマンド_ファイル名変更_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RNFR 1.txt");
            Assert.Equal("350 File exists, ready for destination name.\r\n", cl.StringRecv(1, this));

            cl.StringSend("RNTO $$$.1.txt");
            Assert.Equal("250 RNTO command successful.\r\n", cl.StringRecv(1, this));

            cl.StringSend("RNFR $$$.1.txt");
            Assert.Equal("350 File exists, ready for destination name.\r\n", cl.StringRecv(1, this));

            cl.StringSend("RNTO 1.txt");
            Assert.Equal("250 RNTO command successful.\r\n", cl.StringRecv(1, this));
        }


        [Fact]
        public void RNFR_RNTOコマンド_ディレクトリ名変更_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RNFR home0");
            Assert.Equal("350 File exists, ready for destination name.\r\n", cl.StringRecv(1, this));

            cl.StringSend("RNTO $$$.home0");
            Assert.Equal("250 RNTO command successful.\r\n", cl.StringRecv(1, this));

            cl.StringSend("RNFR $$$.home0");
            Assert.Equal("350 File exists, ready for destination name.\r\n", cl.StringRecv(1, this));

            cl.StringSend("RNTO home0");
            Assert.Equal("250 RNTO command successful.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void RNFR_RNTOコマンド_ディレクトリ名変更_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RNFR home0");
            Assert.Equal("350 File exists, ready for destination name.\r\n", cl.StringRecv(1, this));

            cl.StringSend("RNTO $$$.home0");
            Assert.Equal("250 RNTO command successful.\r\n", cl.StringRecv(1, this));

            cl.StringSend("RNFR $$$.home0");
            Assert.Equal("350 File exists, ready for destination name.\r\n", cl.StringRecv(1, this));

            cl.StringSend("RNTO home0");
            Assert.Equal("250 RNTO command successful.\r\n", cl.StringRecv(1, this));
        }

        [Fact]
        public void RMDコマンド_空でないディレクトリの削除は失敗する_V4()
        {
            var cl = _v4Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RMD home0");
            Assert.Equal("451 Rmd error.\r\n", cl.StringRecv(1, this));

        }

        [Fact]
        public void RMDコマンド_空でないディレクトリの削除は失敗する_V6()
        {
            var cl = _v6Cl;

            //共通処理(ログイン成功)
            Login("user1", cl);

            cl.StringSend("RMD home0");
            Assert.Equal("451 Rmd error.\r\n", cl.StringRecv(1, this));

        }

        public bool IsLife()
        {
            return true;
        }
    }
}