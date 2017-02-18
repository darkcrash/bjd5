using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using Bjd.Pop3Server;
using System.Collections.Generic;
using Bjd.Services;
using Bjd.Threading;
using Xunit.Abstractions;
using Bjd.Test.Logs;

namespace Pop3ServerTest
{
    public class ServerTest : ILife, IDisposable
    {
        internal TestService _service;
        internal Server _v6Sv; //サーバ
        internal Server _v4Sv; //サーバ
        internal TestOutputService _output;
        public readonly int port;

        public ServerTest(ITestOutputHelper helper)
        {

            _service = TestService.CreateTestService();
            _service.SetOption("Pop3ServerTest.ini");

            var kernel = _service.Kernel;
            var option = kernel.ListOption.Get("Pop3");
            var conf = new Conf(option);
            port = _service.GetAvailablePort(IpKind.V4Localhost, conf);

            //サーバ起動
            _v4Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
            _v4Sv.Start();

            _v6Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
            _v6Sv.Start();

            //メールボックスへのデータセット
            _service.CreateMailbox("user1");
            _service.CreateMailbox("user2");

            _service.AddMail("DF_00635026511425888292", "user2");
            _service.AddMail("DF_00635026511765086924", "user2");
            _service.AddMail("MF_00635026511425888292", "user2");
            _service.AddMail("MF_00635026511765086924", "user2");

            //Thread.Sleep(100);//少し余裕がないと多重でテストした場合に、サーバが起動しきらないうちにクライアントからの接続が始まってしまう。
            _output = new TestOutputService(helper);

        }

        // ログイン失敗などで、しばらくサーバが使用できないため、TESTごとサーバを立ち上げて試験する必要がある
        public void Dispose()
        {
            _output.Dispose();

            //サーバ停止
            _v4Sv.Stop();
            _v6Sv.Stop();

            _v4Sv.Dispose();
            _v6Sv.Dispose();

            _service.Dispose();

        }

        //クライアントの生成
        SockTcp CreateClient(InetKind inetKind)
        {
            var kernel = _service.Kernel;
            if (inetKind == InetKind.V4)
            {
                return Inet.Connect(kernel, new Ip(IpKind.V4Localhost), port, 10, null);
            }
            return Inet.Connect(kernel, new Ip(IpKind.V6Localhost), port, 10, null);

        }

        //共通処理(バナーチェック)  Resharperのバージョンを吸収
        private void CheckBanner(string str)
        {
            //テストの際は、バージョン番号はテストツール（ReSharper）のバージョンになる
            //const string bannerStr1 = "+OK BlackJumboDog (Version 9.0.0.0) ready <";
            const string bannerStr = "+OK BlackJumboDog .NET Core (Version ";


            //Assert.That(_v6cl.StringRecv(3, this), Is.EqualTo(BannerStr));

            //if (str.IndexOf(bannerStr1) != 0 && str.IndexOf(bannerStr2) != 0 && str.IndexOf(bannerStr3) != 0 && str.IndexOf(bannerStr4) != 0)
            if (str.IndexOf(bannerStr) != 0)
            {
                Assert.False(true);
            }
        }

        //共通処理(ログイン成功)
        //ユーザ名、メール蓄積数、蓄積サイズ
        void Login(string userName, string password, int n, int size, SockTcp cl)
        {
            CheckBanner(cl.StringRecv(3, this));//バナーチェック

            cl.StringSend($"USER {userName}");
            Assert.Equal($"+OK Password required for {userName}.\r\n", cl.StringRecv(3, this));
            cl.StringSend($"PASS {password}");
            Assert.Equal($"+OK {userName} has {n} message ({size} octets).\r\n", cl.StringRecv(10, this));
        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V4()
        {
            //setUp
            var sv = _v4Sv;
            var expected = "+ サービス中 \t                Pop3\t[127.0.0.1\t:TCP 9210]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 58);
            //verify
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V6()
        {

            //setUp
            var sv = _v6Sv;
            var expected = "+ サービス中 \t                Pop3\t[::1\t:TCP 9210]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 52);
            //verify
            Assert.Equal(expected, actual);

        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void パスワード認証成功(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);

            //exercise verify
            CheckBanner(cl.StringRecv(10, this));//バナーチェック
            cl.StringSend("user user1");
            Assert.Equal("+OK Password required for user1.\r\n", cl.StringRecv(3, this));
            cl.StringSend("PASS user1");
            Assert.Equal("+OK user1 has 0 message (0 octets).\r\n", cl.StringRecv(3, this));
            cl.StringSend("QUIT");
            Assert.Equal("+OK Pop Server at localhost signing off.\r\n", cl.StringRecv(3, this));

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void 複数ログイン(InetKind inetKind)
        {
            //setUp
            var cl1 = CreateClient(inetKind);
            var cl2 = CreateClient(inetKind);
            var cl3 = CreateClient(inetKind);
            var expected = "+OK 2 message (633 octets)\r\n";

            //exercise
            Login("user1", "user1", 0, 0, cl1);
            Login("user2", "user2", 2, 633, cl2);
            Login("user3", "user3", 0, 0, cl3);
            cl2.StringSend("UIDL");
            var actual = cl2.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl1.StringSend("QUIT");
            cl1.Close();
            cl2.StringSend("QUIT");
            cl2.Close();
            cl3.StringSend("QUIT");
            cl3.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void 多重ログイン(InetKind inetKind)
        {
            //setUp
            var clDmy = CreateClient(inetKind);
            Login("user1", "user1", 0, 0, clDmy);
            var cl = CreateClient(inetKind);
            var expected = "-ERR Double login\r\n";

            //exercise
            CheckBanner(cl.StringRecv(3, this));//バナーチェック
            cl.StringSend("user user1");
            Assert.Equal(cl.StringRecv(3, this), "+OK Password required for user1.\r\n");
            cl.StringSend("PASS user1");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            clDmy.StringSend("QUIT");
            clDmy.Close();
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void CHPSによるパスワード変更_文字数不足による失敗(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "-ERR The number of letter is not enough.\r\n";

            //exercise
            Login("user1", "user1", 0, 0, cl);
            cl.StringSend("CHPS abc");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void CHPSによるパスワード変更成功(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            //exercise verify
            Login("user1", "user1", 0, 0, cl);
            cl.StringSend("CHPS ABCabc#123"); //パスワード変更
            Assert.Equal(cl.StringRecv(5, this), "+OK Password changed.\r\n");
            cl.StringSend("QUIT"); //コネクション終了

            cl = CreateClient(inetKind); //再接続
            Login("user1", "ABCabc#123", 0, 0, cl); //変更後のパスワードでログインする

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void パスワード認証失敗(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            string expected = null;

            //exercise
            CheckBanner(cl.StringRecv(3, this));//バナーチェック
            cl.StringSend("user user1");
            Assert.Equal(cl.StringRecv(3, this), "+OK Password required for user1.\r\n");
            cl.StringSend("PASS xxx");
            var actual = cl.StringRecv(3, this);

            //verify
            //パスワードに誤りがある場合、ブルートフォース対策のためしばらくレスポンスが無い
            Assert.Equal(expected, actual);



            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void USERの前にPASSコマンドを送るとエラーが返る(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "-ERR Invalid command.\r\n";

            //exercise
            CheckBanner(cl.StringRecv(3, this));//バナーチェック
            cl.StringSend("PASS user1");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);
            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void APOP認証成功(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "+OK user1 has 0 message (0 octets).\r\n";

            //exercise
            var rescStr = Inet.TrimCrlf(cl.StringRecv(3, this));
            var challengeStr = rescStr.Split(' ')[7];

            //var result = (new MD5CryptoServiceProvider()).ComputeHash(Encoding.ASCII.GetBytes(challengeStr + "user1"));
            var md5 = System.Security.Cryptography.MD5.Create();
            md5.Initialize();
            var result = md5.ComputeHash(Encoding.ASCII.GetBytes(challengeStr + "user1"));
            var sb = new StringBuilder();
            for (int i = 0; i < 16; i++)
            {
                sb.Append(string.Format("{0:x2}", result[i]));
            }
            cl.StringSend("APOP user1 " + sb.ToString());
            var actual = cl.StringRecv(5, this);

            //verify
            Assert.Equal(expected, actual);



            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void APOP認証失敗(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            string expected = null;

            //exercise
            var challengeStr = Inet.TrimCrlf(cl.StringRecv(3, this)).Split(' ')[5];

            //var result = (new MD5CryptoServiceProvider()).ComputeHash(Encoding.ASCII.GetBytes(challengeStr + "user1"));
            var md5 = System.Security.Cryptography.MD5.Create();
            md5.Initialize();
            var result = md5.ComputeHash(Encoding.ASCII.GetBytes(challengeStr + "user1"));
            var sb = new StringBuilder();
            for (int i = 0; i < 16; i++)
            {
                sb.Append(string.Format("{0:x2}", result[i]));
            }
            cl.StringSend("APOP user2 " + sb.ToString());
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }


        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void 無効なコマンドでエラーが返る(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "-ERR Invalid command.\r\n";

            //exercise
            CheckBanner(cl.StringRecv(3, this));//バナーチェック
            cl.StringSend("xxx");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void 空行を送るとエラーが返る(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "-ERR Invalid command.\r\n";

            //exercise
            Login("user1", "user1", 0, 0, cl);
            cl.StringSend("");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);


            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void LISTコマンドの応答_メール蓄積が無い場合(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);

            //exercise
            Login("user1", "user1", 0, 0, cl);
            cl.StringSend("LIST");
            //var actual = Inet.RecvLines(cl, 5, this);

            var actual1 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(3, this)));
            var actual2 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(3, this)));

            //verify
            //Assert.Equal(actual.Count, 2);
            //Assert.Equal(actual[0], "+OK 0 message (0 octets)");
            //Assert.Equal(actual[1], ".");
            Assert.Equal("+OK 0 message (0 octets)", actual1);
            Assert.Equal(".", actual2);


            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void LISTコマンドの応答_メール蓄積がある場合(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("LIST");
            //var actual = Inet.RecvLines(cl, 3, this);
            var actual1 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(3, this)));
            var actual2 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(3, this)));
            var actual3 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(3, this)));
            var actual4 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(3, this)));

            //verify
            //Assert.Equal(actual.Count, 4);
            //Assert.Equal(actual[0], "+OK 2 message (633 octets)");
            //Assert.Equal(actual[1], "1 317");
            //Assert.Equal(actual[2], "2 316");
            //Assert.Equal(actual[3], ".");

            Assert.Equal("+OK 2 message (633 octets)", actual1);
            Assert.Equal("1 317", actual2);
            Assert.Equal("2 316", actual3);
            Assert.Equal(".", actual4);


            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void LISTコマンドの有効パラメータの場合のレスポンス確認(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "+OK 1 317\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("LIST 1");
            var actual = cl.StringRecv(3, this);
            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void LISTコマンドの無効パラメータの場合のレスポンス確認(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "-ERR Message 3 does not exist.\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("LIST 3");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RETRによるメール受信(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("RETR 1");

            var actual1 = cl.StringRecv(3, this);
            Assert.Equal("+OK 317 octets\r\n", actual1);

            //var actual = Inet.RecvLines(cl, 3, this);
            var actual = new List<String>();

            for (var i = 0; i < 12; i++)
            {
                var recvLine = cl.StringRecv(1, this);
                if (recvLine == null) Assert.False(true);
                actual.Add(recvLine);
            }

            //verify
            Assert.Equal(actual.Count, 12);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RETR_パラメータ無しによるエラー(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "-ERR Too few arguments for the RETR command.\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("RETR");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RETR_無効パラメータによるエラー(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "-ERR Message 3 does not exist.\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("RETR 3"); //存在しないメール
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void NOOPコマンド(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "+OK\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("NOOP");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void STATコマンド(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "+OK 2 633\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("STAT");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void STAT_パラメータ有りの場合の確認(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "+OK 2 633\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("STAT 2");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void TOP_パラメータ無しによるエラー(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "-ERR Too few arguments for the TOP command.\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("TOP");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void TOP_無効パラメータの確認(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "-ERR Too few arguments for the TOP 1 command.\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("TOP 1");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void TOP_有効パラメータによるデータ取得(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("TOP 1 2");
            var actual = Inet.RecvLines(cl, 2, this);

            //verify
            Assert.Equal(13, actual.Count);
            Assert.Equal("+OK 317 octets", actual[0]);
            Assert.Equal("Message-ID: <bjd.00635026511425808252.000@example.com>", actual[5]);
            Assert.Equal("From: <1@1>", actual[6]);
            Assert.Equal(".", actual[12]);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void TOP_無効パラメータ_存在しないデータ(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "-ERR Message 3 does not exist.\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("TOP 3 2");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void UIDLコマンドの確認(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "+OK 2 message (633 octets)\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("UIDL");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void UIDL_パラメータ有り(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "+OK 1 bjd.00635026511425808252.000\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("UIDL 1");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void UIDL_無効パラメータ_無効データ(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "-ERR Message 3 does not exist.\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("UIDL 3");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DELEコマンドによるデータ削除成功(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "+OK 317 octets\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("DELE 1");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DELEコマンド_無効パラメータによる失敗(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "-ERR Message 3 does not exist.\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("DELE 3");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DELEコマンド_無効パラメータによる失敗2(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "-ERR Invalid message number.\r\n";

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("DELE ABC");
            var actual = cl.StringRecv(5, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DELEコマンドによるデータ削除成功後のメール数(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "+OK 1 message (316 octets)\r\n"; //１通に減少する

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("DELE 1");
            cl.StringRecv(3, this);
            cl.StringSend("LIST");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DELEコマンドによるデータ削除後のRSETによる復活(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var expected = "+OK 2 message (633 octets)\r\n"; // 最初の状態に戻る

            //exercise
            Login("user2", "user2", 2, 633, cl);
            cl.StringSend("DELE 1");
            cl.StringRecv(3, this);
            cl.StringSend("RSET");
            cl.StringRecv(3, this);
            cl.StringSend("LIST");
            var actual = cl.StringRecv(5, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }


        public bool IsLife()
        {
            return true;
        }
    }
}