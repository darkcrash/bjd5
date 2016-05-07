using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Bjd;
using Bjd.mail;
using Bjd.net;
using Bjd.option;
using Bjd.Common.Test;
using Xunit;
using Bjd.SmtpServer;

namespace Bjd.SmtpServer.Test
{
    public class PopClientTest : ILife, IDisposable
    {

        private TestServer _testServer;

        // ログイン失敗などで、しばらくサーバが使用できないため、TESTごとサーバを立ち上げて試験する必要がある
        public PopClientTest()
        {

            //MailBoxは、Pop3ServerTest.iniの中で「c:\tmp2\bjd5\SmtpServerTest\mailbox」に設定されている
            //また、上記のMaloBoxには、user1=0件　user2=2件　のメールが着信している
            _testServer = new TestServer(TestServerType.Pop, "SmtpServerTest\\Fetch", "PopClientTest.ini");

            //usrr2のメールボックスへの２通のメールをセット
            _testServer.SetMail("user2", "00635026511425888292");
            _testServer.SetMail("user2", "00635026511765086924");

        }


        public void Dispose()
        {
            _testServer.Dispose();
        }

        private PopClient CreatePopClient(InetKind inetKind)
        {
            if (inetKind == InetKind.V4)
            {
                return new PopClient(new Kernel(), new Ip(IpKind.V4Localhost), 9110, 3, this);
            }
            return new PopClient(new Kernel(), new Ip(IpKind.V6Localhost), 9110, 3, this);
        }




        [Theory]
        [InlineData(InetKind.V4, "127.0.0.1", 9112)]
        [InlineData(InetKind.V6, "::1", 9112)]
        public void 接続失敗_ポート間違い(InetKind inetKind, String addr, int port)
        {
            //setUp
            var sut = new PopClient(new Kernel(), new Ip(addr), port, 3, this);
            var expected = false;

            //exercise
            var actual = sut.Connect();

            //verify
            Assert.Equal(actual, expected);
            Assert.Equal(sut.GetLastError(), "Faild in PopClient Connect()");

            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4, "127.0.0.2")]
        [InlineData(InetKind.V6, "::2")]
        public void 接続失敗_アドレス間違い(InetKind inetKind, String addr)
        {
            //setUp
            var sut = new PopClient(new Kernel(), new Ip(addr), 9110, 3, this);
            var expected = false;

            //exercise
            var actual = sut.Connect();

            //verify
            Assert.Equal(actual, expected);
            Assert.Equal(sut.GetLastError(), "Faild in PopClient Connect()");

            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void ログイン成功(InetKind inetKind)
        {
            //setUp
            var sut = CreatePopClient(inetKind);
            var expected = true;

            //exercise
            sut.Connect();
            var actual = sut.Login("user1", "user1");

            //verify
            Assert.Equal(actual, expected);

            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void ログイン失敗_パスワードの間違い(InetKind inetKind)
        {
            //setUp
            var sut = CreatePopClient(inetKind);
            var expected = false;

            //exercise
            sut.Connect();
            var actual = sut.Login("user1", "xxx");

            //verify
            Assert.Equal(actual, expected);
            Assert.Equal(sut.GetLastError(), "Timeout in PopClient RecvStatus()");

            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void user1のUidl取得(InetKind inetKind)
        {
            //setUp
            var sut = CreatePopClient(inetKind);
            var expected = true;

            //exercise
            sut.Connect();
            sut.Login("user1", "user1");

            var lines = new List<string>();
            var actual = sut.Uidl(lines);

            //verify
            Assert.Equal(actual, expected);
            Assert.Equal(lines.Count, 0);

            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void user2のUidl取得(InetKind inetKind)
        {
            //setUp
            var sut = CreatePopClient(inetKind);
            var expected = true;

            //exercise
            sut.Connect();
            sut.Login("user2", "user2");

            var lines = new List<string>();
            var actual = sut.Uidl(lines);

            //verify
            Assert.Equal(actual, expected);
            Assert.Equal(lines[0], "1 bjd.00635026511425808252.000");
            Assert.Equal(lines[1], "2 bjd.00635026511765066907.001");

            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RETRによるメール取得(InetKind inetKind)
        {
            //setUp
            var sut = CreatePopClient(inetKind);
            var expected = true;

            //exercise
            sut.Connect();
            sut.Login("user2", "user2");

            var mail = new Mail();
            var actual = sut.Retr(0, mail);

            //verify
            Assert.Equal(actual, expected);
            Assert.Equal(mail.GetBytes().Length, 308);
            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RETRによるメール取得_失敗(InetKind inetKind)
        {
            //setUp
            var sut = CreatePopClient(inetKind);
            var expected = false;

            //exercise
            sut.Connect();
            sut.Login("user1", "user1");

            var mail = new Mail();
            var actual = sut.Retr(0, mail); //user1は滞留が0通なので、存在しないメールをリクエストしている

            //verify
            Assert.Equal(actual, expected);
            Assert.Equal(sut.GetLastError(), "Not Found +OK in PopClient RecvStatus()");
            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DELEによるメール削除(InetKind inetKind)
        {
            //setUp
            var sut = CreatePopClient(inetKind);
            //var expected = true;

            //exercise
            sut.Connect();
            sut.Login("user2", "user2");

            //verify
            sut.Dele(0);//1通削除
            Assert.Equal(CountMail("user2"), 2);//QUIT前は２通

            sut.Quit();
            Assert.Equal(CountMail("user2"), 1);//QUIT後は１通



            //tearDown
            sut.Dispose();
        }


        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DELEによるメール削除_失敗(InetKind inetKind)
        {
            //setUp
            var sut = CreatePopClient(inetKind);
            var expected = false;

            //exercise
            sut.Connect();
            sut.Login("user1", "user1");

            //verify
            var actual = sut.Dele(0);
            Assert.Equal(actual, expected);
            Assert.Equal(sut.GetLastError(), "Not Found +OK in PopClient RecvStatus()");


            //tearDown
            sut.Quit();
            sut.Dispose();
        }

        //メール通数の確認
        int CountMail(String user)
        {
            //メールボックス内に蓄積されたファイル数を検証する
            var path = String.Format("c:\\tmp2\\bjd5\\SmtpServerTest\\mailbox\\{0}", user);
            var di = new DirectoryInfo(path);

            //DF_*がn個存在する
            var files = di.GetFiles("DF_*");
            return files.Count();
        }
        public bool IsLife()
        {
            return true;
        }

    }
}
