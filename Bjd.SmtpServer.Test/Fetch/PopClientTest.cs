﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Bjd;
using Bjd.Mailbox;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Test;
using Xunit;
using Bjd.SmtpServer;
using Bjd.Threading;
using Xunit.Abstractions;
using Bjd.Test.Logs;

namespace Bjd.SmtpServer.Test
{
    public class PopClientTest : ILife, IDisposable
    {

        public class ServerFixture : TestServer, IDisposable
        {
            public ServerFixture() : base(TestServerType.Pop, "PopClientTest.ini")
            {

            }

        }

        private ServerFixture _testServer;
        private Kernel _kernel;

        // ログイン失敗などで、しばらくサーバが使用できないため、TESTごとサーバを立ち上げて試験する必要がある
        public PopClientTest(ITestOutputHelper helper)
        {
            _testServer = new ServerFixture();
            _testServer._service.AddOutput(helper);
            //_output = new TestOutputService(helper);

            //usrr2のメールボックスへの２通のメールをセット
            _testServer._service.CleanMailbox("user2");
            _testServer.SetMail("user2", "00635026511425888292");
            _testServer.SetMail("user2", "00635026511765086924");

            _kernel = _testServer._service.Kernel;

        }


        public void Dispose()
        {
            //_output.Dispose();
            _testServer.Dispose();
        }

        private PopClient CreatePopClient(InetKind inetKind)
        {
            var kernel = _testServer._service.Kernel;
            if (inetKind == InetKind.V4)
            {
                return new PopClient(kernel, new Ip(IpKind.V4Localhost), _testServer.port, 3, this);
            }
            return new PopClient(kernel, new Ip(IpKind.V6Localhost), _testServer.port, 3, this);
        }


        [Theory]
        [InlineData("127.0.0.1", 1)]
        [InlineData("::1", 1)]
        public void 接続失敗_ポート間違い(string addr, int port)
        {
            var kernel = _testServer._service.Kernel;
            //setUp
            using (var sut = new PopClient(kernel, new Ip(addr), port, 3, this))
            {
                var expected = false;

                //exercise
                var actual = sut.Connect();

                //verify
                Assert.Equal(expected, actual);
                Assert.Equal("Faild in PopClient Connect()", sut.GetLastError());

                //tearDown
                //sut.Dispose();
            }
        }

        [Theory]
        [InlineData("127.0.0.2")]
        [InlineData("::2")]
        public void 接続失敗_アドレス間違い(string addr)
        {
            var kernel = _testServer._service.Kernel;
            var port = _testServer.port;
            //setUp
            using (var sut = new PopClient(kernel, new Ip(addr), port, 3, this))
            {
                var expected = false;

                //exercise
                var actual = sut.Connect();

                //verify
                Assert.Equal(expected, actual);
                Assert.Equal("Faild in PopClient Connect()", sut.GetLastError());

                //tearDown
                //sut.Dispose();
            }
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void ログイン成功(InetKind inetKind)
        {
            //setUp
            using (var sut = CreatePopClient(inetKind))
            {
                var expected = true;

                //exercise
                sut.Connect();
                var actual = sut.Login("user1", "user1");

                //verify
                Assert.Equal(expected, actual);

                //tearDown
                //sut.Dispose();
            }
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void ログイン失敗_パスワードの間違い(InetKind inetKind)
        {
            //setUp
            using (var sut = CreatePopClient(inetKind))
            {
                var expected = false;

                //exercise
                sut.Connect();
                var actual = sut.Login("user1", "xxx");

                //verify
                Assert.Equal(expected, actual);
                Assert.Equal("Timeout in PopClient RecvStatus()", sut.GetLastError());

                //tearDown
                //sut.Dispose();
            }
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void user1のUidl取得(InetKind inetKind)
        {
            //setUp
            using (var sut = CreatePopClient(inetKind))
            {
                var expected = true;

                //exercise
                sut.Connect();
                sut.Login("user1", "user1");

                var lines = new List<string>();
                var actual = sut.Uidl(lines);

                //verify
                Assert.Equal(expected, actual);
                Assert.Empty(lines);

                //tearDown
                //sut.Dispose();
            }
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void user2のUidl取得(InetKind inetKind)
        {
            //setUp
            using (var sut = CreatePopClient(inetKind))
            {
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
                //sut.Dispose();
            }
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RETRによるメール取得(InetKind inetKind)
        {
            //setUp
            using (var sut = CreatePopClient(inetKind))
            {
                var expected = true;

                //exercise
                sut.Connect();
                sut.Login("user2", "user2");

                var mail = new Mail(_kernel);
                var actual = sut.Retr(0, mail);

                //verify
                Assert.Equal(expected, actual);

                var enc = mail.GetEncoding();
                _kernel.Logger.TraceInformation(enc.GetString(mail.GetBytes()));

                var mfList = _testServer.GetMf("user2");
                var mf1 = mfList.First();

                //Assert.Equal(308, mail.GetBytes().Length);
                Assert.Equal(mf1.Length, mail.GetBytes().Length);

                //tearDown
                //sut.Dispose();
            }

        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RETRによるメール取得_失敗(InetKind inetKind)
        {
            //setUp
            using (var sut = CreatePopClient(inetKind))
            {
                var expected = false;

                //exercise
                sut.Connect();
                sut.Login("user1", "user1");

                var mail = new Mail(_kernel);
                var actual = sut.Retr(0, mail); //user1は滞留が0通なので、存在しないメールをリクエストしている

                //verify
                Assert.Equal(expected, actual);
                Assert.Equal("Not Found +OK in PopClient RecvStatus()", sut.GetLastError());
                //tearDown
                //sut.Dispose();
            }
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DELEによるメール削除(InetKind inetKind)
        {
            //setUp
            using (var sut = CreatePopClient(inetKind))
            {
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
                //sut.Dispose();
            }
        }


        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DELEによるメール削除_失敗(InetKind inetKind)
        {
            //setUp
            using (var sut = CreatePopClient(inetKind))
            {
                var expected = false;

                //exercise
                sut.Connect();
                sut.Login("user1", "user1");

                //verify
                var actual = sut.Dele(0);
                Assert.Equal(expected, actual);
                Assert.Equal("Not Found +OK in PopClient RecvStatus()", sut.GetLastError());


                //tearDown
                sut.Quit();
                //sut.Dispose();
            }
        }

        //メール通数の確認
        int CountMail(String user)
        {
            //メールボックス内に蓄積されたファイル数を検証する
            //var path = String.Format("c:\\tmp2\\bjd5\\SmtpServerTest\\mailbox\\{0}", user);
            var path = Path.Combine(_testServer._service.MailboxPath, user);
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
