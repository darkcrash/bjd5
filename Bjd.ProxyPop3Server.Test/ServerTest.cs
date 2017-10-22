using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using Bjd.Initialization;
using Bjd.Threading;

namespace ProxyPop3ServerTest
{
    public class ServerTest : ILife, IDisposable, IClassFixture<ServerTest.ServerFixture>
    {
        private TestService _service; 
        private Bjd.ProxyPop3Server.Server _v6Sv; //サーバ
        private Bjd.ProxyPop3Server.Server _v4Sv; //サーバ

        private Bjd.Pop3Server.Server _v6UltimateSv; //最終到達サーバ
        private Bjd.Pop3Server.Server _v4UltimateSv; //最終到達サーバ

        public class ServerFixture : IDisposable
        {

            public ServerFixture()
            {
            }

            public void Dispose()
            {

            }


        }

        public ServerTest(ServerFixture fixture)
        {
            // ログイン失敗などで、しばらくサーバが使用できないため、TESTごとサーバを立ち上げて試験する必要がある

            //MailBoxは、Pop3ServerTest.iniの中で「c:\tmp2\bjd5\Pop3ServerTest\mailbox」に設定されている
            //また、上記のMaloBoxには、user1=0件　user2=2件　のメールが着信している

            _service = TestService.CreateTestService();
            _service.SetOption("ProxyPop3ServerTest.ini");

            var kernel = _service.Kernel;
            kernel.ListInitialize();

            var option = kernel.ListOption.Get("ProxyPop3");
            var conf = new Conf(option);

            //サーバ起動 Port:8110 => 127.0.0.1:9110
            _v4Sv = new Bjd.ProxyPop3Server.Server(kernel, conf, new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
            _v4Sv.Start();

            _v6Sv = new Bjd.ProxyPop3Server.Server(kernel, conf, new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
            _v6Sv.Start();

            //最終到達サーバ Port:9110
            option = kernel.ListOption.Get("Pop3");
            conf = new Conf(option);
            _v4UltimateSv = new Bjd.Pop3Server.Server(kernel, conf, new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
            _v4UltimateSv.Start();

            _v6UltimateSv = new Bjd.Pop3Server.Server(kernel, conf, new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
            _v6UltimateSv.Start();


            //メールボックスへのデータセット
            _service.CreateMailbox("user2");

            _service.AddMail("DF_00635026511425888292", "user2");
            _service.AddMail("DF_00635026511765086924", "user2");
            _service.AddMail("MF_00635026511425888292", "user2");
            _service.AddMail("MF_00635026511765086924", "user2");

        }

        public void Dispose()
        {
            //サーバ停止
            _v4Sv.Stop();
            _v6Sv.Stop();

            _v4Sv.Dispose();
            _v6Sv.Dispose();

            _v4UltimateSv.Stop();
            _v6UltimateSv.Stop();

            _v4UltimateSv.Dispose();
            _v6UltimateSv.Dispose();

            _service.Dispose();

        }

        //クライアントの生成
        ISocket CreateClient(InetKind inetKind)
        {
            int port = 8110;
            if (inetKind == InetKind.V4)
            {
                return Inet.Connect(_service.Kernel, new Ip(IpKind.V4Localhost), port, 10, null);
            }
            return Inet.Connect(_service.Kernel, new Ip(IpKind.V6Localhost), port, 10, null);

        }

        //共通処理(バナーチェック)  Resharperのバージョンを吸収
        //        private void CheckBanner(string str) {
        //            //テストの際は、バージョン番号はテストツール（ReSharper）のバージョンになる
        //            const string bannerStr1 = "xxx";
        //            const string bannerStr2 = "+OK BlackJumboDog (Version 7.1.2000.1478) ready <";
        //
        //
        //            //Assert.Equal(_v6cl.StringRecv(3, this), BannerStr);
        //
        //            if (str != bannerStr1 && str.IndexOf(bannerStr2) != 0) {
        //                Assert.Fail();
        //            }
        //        }
        //
        //共通処理(ログイン成功)
        //ユーザ名、メール蓄積数、蓄積サイズ
        void Login(string userName, string password, int n, int size, ISocket cl)
        {
            Assert.Equal("+OK \r\n", cl.StringRecv(3, this));
            cl.StringSend(string.Format("USER {0}", userName));
            Assert.Equal(cl.StringRecv(3, this), string.Format("+OK Password required for {0}.\r\n", userName));
            cl.StringSend(string.Format("PASS {0}", password));
            Assert.Equal(string.Format("+OK {0} has {1} message ({2} octets).\r\n", userName, n, size), cl.StringRecv(10, this));
        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V4()
        {
            //setUp
            var sv = _v4Sv;
            var expected = "+ サービス中 \t           ProxyPop3\t[127.0.0.1\t:TCP 8110]\tThread";

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
            var expected = "+ サービス中 \t           ProxyPop3\t[::1\t:TCP 8110]\tThread";

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
            Assert.Equal("+OK \r\n", cl.StringRecv(3, this));
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
        public void パスワード認証失敗_無効ユーザ(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);

            //exercise verify
            Assert.Equal("+OK \r\n", cl.StringRecv(3, this));
            cl.StringSend("user xxxx");
            Assert.Equal("+OK Password required for xxxx.\r\n", cl.StringRecv(3, this));
            cl.StringSend("PASS xxxx");
            Assert.Null(cl.StringRecv(3, this));

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void パスワード認証失敗_無効パスワード(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);

            //exercise verify
            Assert.Equal("+OK \r\n", cl.StringRecv(3, this));
            cl.StringSend("user user1");
            Assert.Equal("+OK Password required for user1.\r\n", cl.StringRecv(3, this));
            cl.StringSend("PASS xxxx");
            Assert.Null(cl.StringRecv(3, this));

            //tearDown
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
            var actual = Inet.RecvLines(cl, 3, this);

            //verify
            Assert.Equal(13, actual.Count);
            Assert.Equal("+OK 317 octets", actual[0]);

            //tearDown
            cl.StringSend("QUIT");
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        //[InlineData(InetKind.V6)]
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


        //        [TestCase(InetKind.V4)]
        //        //[TestCase(InetKind.V6)]
        //        public void 多重ログイン(InetKind inetKind) {
        //            //setUp
        //            var cl = CreateClient(inetKind);
        //            var cl2 = CreateClient(inetKind);
        //
        //            //exercise
        //            Login("user1", "user1", 0, 0, cl2);
        //            Login("user2", "user2", 2, 633, cl);
        //            cl.StringSend("RETR 1");
        //            var actual = Inet.RecvLines(cl, 3, this);
        //
        //            //verify
        //            Assert.Equal(actual.Count, 13);
        //            Assert.Equal(actual[0], "+OK 317 octets");
        //
        //            //tearDown
        //            cl.StringSend("QUIT");
        //            cl.Close();
        //
        //            cl2.StringSend("QUIT");
        //            cl2.Close();
        //
        //        }
        //


        //
        //        [TestCase(InetKind.V4)]
        //        [TestCase(InetKind.V6)]
        //        public void CHPSによるパスワード変更成功(InetKind inetKind) {
        //            //setUp
        //            var cl = CreateClient(inetKind);
        //            //exercise verify
        //            Login("user1", "user1", 0, 0, cl);
        //            cl.StringSend("CHPS ABCabc#123"); //パスワード変更
        //            Assert.Equal(cl.StringRecv(3, this), "+OK Password changed.\r\n");
        //            cl.StringSend("QUIT"); //コネクション終了
        //
        //            cl = CreateClient(inetKind); //再接続
        //            Login("user1", "ABCabc#123", 0, 0, cl); //変更後のパスワードでログインする
        //
        //            //tearDown
        //            cl.StringSend("QUIT");
        //            cl.Close();
        //        }


        public bool IsLife()
        {
            return true;
        }
    }
}