using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd;
using Bjd.Mails;
using Bjd.Net;
using Xunit;
using Bjd.Common.Test;
using System.IO;

namespace Bjd.SmtpServer.Test.Agent
{
    public class SmtpClientTest : ILife, IDisposable, IClassFixture<SmtpClientTest.ServerFixture>
    {

        public class ServerFixture : TestServer, IDisposable
        {
            private string mailboxUser1;
            private string mailboxUser2;

            public ServerFixture() : base(TestServerType.Smtp, "Bjd.SmtpServer.CoreCLR.Test\\Agent", "SmtpClientTest.ini")
            {
                //usrr2のメールボックスへの２通のメールをセット
                //SetMail("user1", "00635026511425888292");
                //SetMail("user1", "00635026511765086924");
                mailboxUser1 = Path.Combine(TestDefine.Instance.TestMailboxPath, "user1");
                mailboxUser2 = Path.Combine(TestDefine.Instance.TestMailboxPath, "user2");
                Directory.CreateDirectory(mailboxUser1);
                Directory.CreateDirectory(mailboxUser2);


            }

            public override void Dispose()
            {
                try
                {
                    Directory.Delete(mailboxUser1);
                }
                catch { }
                try
                {
                    Directory.Delete(mailboxUser2);
                }
                catch { }


                base.Dispose();
            }

        }

        private ServerFixture _testServer;


        public SmtpClientTest(ServerFixture fixture)
        {
            //_testServer = new TestServer(TestServerType.Smtp, "SmtpServerTest\\Agent", "SmtpClientTest.ini");

            _testServer = fixture;

        }

        public void Dispose()
        {
        }

        private SmtpClient CreateSmtpClient(InetKind inetKind)
        {
            if (inetKind == InetKind.V4)
            {
                return new SmtpClient(new Ip(IpKind.V4Localhost), 9025, 3, this);
            }
            return new SmtpClient(new Ip(IpKind.V6Localhost), 9025, 3, this);
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void 正常系(InetKind inetKind)
        {
            //setUp
            var sut = CreateSmtpClient(inetKind);

            //exercise
            Assert.Equal(sut.Connect(), true);
            Assert.Equal(sut.Helo(), true);
            Assert.Equal(sut.Mail("1@1"), true);
            Assert.Equal(sut.Rcpt("user1@example.com"), true);
            Assert.Equal(sut.Data(new Mail()), true);

            Assert.Equal(sut.Quit(), true);

            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        public void 宛先にメールが到着していることを確認する(InetKind inetKind)
        {
            //setUp
            var sut = CreateSmtpClient(inetKind);

            var expectedUser1 = _testServer.GetDf("user1").Count();
            var expectedUser2 = _testServer.GetDf("user2").Count();

            //exercise
            sut.Connect();
            sut.Helo();
            sut.Mail("1@1");
            sut.Rcpt("user1@example.com");
            sut.Rcpt("user2@example.com");
            sut.Data(new Mail());
            sut.Quit();

            //verify
            //user1及びuser2に１通づつメールが到着していることを確認する
            var actual = _testServer.GetDf("user1").Count();
            Assert.Equal(expectedUser1 + 1, actual);

            actual = _testServer.GetDf("user2").Count();
            Assert.Equal(expectedUser2 + 1, actual);

            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        public void Dataの動作確認_Bodyの確認(InetKind inetKind)
        {
            //setUp
            var sut = CreateSmtpClient(inetKind);

            var mail1 = new Mail();
            mail1.Init2(Encoding.ASCII.GetBytes("1:1\r\n\r\nbody1\r\nbody2\r\n"));

            //exercise
            sut.Connect();
            sut.Helo();
            sut.Mail("1@1");
            sut.Rcpt("user1@example.com");
            sut.Data(mail1);
            sut.Quit();

            //verify
            var mailList = _testServer.GetMf("user1");
            var mail2 = mailList[mailList.Count - 1];
            Assert.Equal(mail2.GetBody(), mail1.GetBody());

            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        public void Dataの動作確認_Bodyの確認_ドットのみの行を含む(InetKind inetKind)
        {
            //setUp
            var sut = CreateSmtpClient(inetKind);

            var mail1 = new Mail();
            mail1.Init2(Encoding.ASCII.GetBytes("1:1\r\n\r\nbody1\r\nbody2\r\n.\r\n"));

            //exercise
            sut.Connect();
            sut.Helo();
            sut.Mail("1@1");
            sut.Rcpt("user1@example.com");
            sut.Data(mail1);
            sut.Quit();

            //verify
            var mailList = _testServer.GetMf("user1");
            var mail2 = mailList[mailList.Count - 1];
            Assert.Equal(mail2.GetBody(), mail1.GetBody());

            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        public void Dataの動作確認_Bodyの確認_最後が改行で終わらない(InetKind inetKind)
        {
            //setUp
            var sut = CreateSmtpClient(inetKind);

            var mail1 = new Mail();
            mail1.Init2(Encoding.ASCII.GetBytes("1:1\r\n\r\nbody1\r\nbody2\r\n123"));

            //exercise
            sut.Connect();
            sut.Helo();
            sut.Mail("1@1");
            sut.Rcpt("user1@example.com");
            sut.Data(mail1);
            sut.Quit();

            //verify
            var mail2 = _testServer.GetMf("user1")[0];
            var actual = mail2.GetBody().Length;
            var expected = mail1.GetBody().Length + 2;//\r\nが追加される
            Assert.Equal(expected, actual);

            //tearDown
            sut.Dispose();
        }

        public bool IsLife()
        {
            return true;
        }
    }
}
