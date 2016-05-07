﻿using System;
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
    public class PopClientTest : ILife, IDisposable, IClassFixture<PopClientTest.ServerFixture>
    {

        public class ServerFixture : TestServer, IDisposable
        {
            public ServerFixture() : base(TestServerType.Pop, "Bjd.SmtpServer.CoreCLR.Test\\Fetch", "PopClientTest.ini")
            {
                ////MailBoxは、Pop3ServerTest.iniの中で「c:\tmp2\bjd5\SmtpServerTest\mailbox」に設定されている
                ////また、上記のMaloBoxには、user1=0件　user2=2件　のメールが着信している
                //_testServer = new TestServer(TestServerType.Pop, "SmtpServerTest\\Fetch", "PopClientTest.ini");

                //usrr2のメールボックスへの２通のメールをセット
                SetMail("user2", "00635026511425888292");
                SetMail("user2", "00635026511765086924");

            }
            public override void Dispose()
            {
                //fetchDbの削除
                //File.Delete(@"c:\tmp2\bjd5\BJD\out\fetch.127.0.0.1.9110.user2.localuser.db");
                //File.Delete(@"c:\tmp2\bjd5\BJD\out\fetch.127.0.0.1.9110.user1.localuser.db");
                File.Delete(Path.Combine(TestDefine.Instance.TestDirectory, "fetch.127.0.0.1.9110.user2.localuser.db"));
                File.Delete(Path.Combine(TestDefine.Instance.TestDirectory, "fetch.127.0.0.1.9110.user1.localuser.db"));

                base.Dispose();
            }

        }

        private ServerFixture _testServer;

        // ログイン失敗などで、しばらくサーバが使用できないため、TESTごとサーバを立ち上げて試験する必要がある
        public PopClientTest(ServerFixture fixture)
        {
            _testServer = fixture;


        }


        public void Dispose()
        {
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
            Assert.Equal(expected, actual);
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
            Assert.Equal(expected, actual);
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
            Assert.Equal(expected, actual);

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
            Assert.Equal(expected, actual);
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
            Assert.Equal(expected, actual);
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
            Assert.Equal(expected, actual);
            Assert.Equal("1 bjd.00635026511425808252.000", lines[0]);
            Assert.Equal("2 bjd.00635026511765066907.001", lines[1]);

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
            Assert.Equal(expected, actual);
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
            Assert.Equal(expected, actual);
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
            var expected = CountMail("user2");

            //exercise
            sut.Connect();
            sut.Login("user2", "user2");

            //verify
            sut.Dele(0);//1通削除
            Assert.Equal(expected, CountMail("user2"));//QUIT前は２通

            sut.Quit();
            Assert.Equal(expected - 1, CountMail("user2"));//QUIT後は１通



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
            Assert.Equal(expected, actual);
            Assert.Equal(sut.GetLastError(), "Not Found +OK in PopClient RecvStatus()");


            //tearDown
            sut.Quit();
            sut.Dispose();
        }

        //メール通数の確認
        int CountMail(String user)
        {
            //メールボックス内に蓄積されたファイル数を検証する
            //var path = String.Format("c:\\tmp2\\bjd5\\SmtpServerTest\\mailbox\\{0}", user);
            var path = Path.Combine(TestDefine.Instance.TestMailboxPath, user);
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
