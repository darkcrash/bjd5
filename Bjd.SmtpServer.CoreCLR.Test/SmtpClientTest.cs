using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd;
using Bjd.Mails;
using Bjd.Net;
using Xunit;
using Bjd.Test;
using System.IO;
using Bjd.Services;
using Bjd.Threading;

namespace Bjd.SmtpServer.Test
{
    public class SmtpClientTest : ILife, IDisposable, IClassFixture<SmtpClientTest.ServerFixture>
    {

        public class ServerFixture : TestServer, IDisposable
        {
            public ServerFixture() : base(TestServerType.Smtp, "SmtpClientTest.ini")
            {
                //usrr2のメールボックスへの２通のメールをセット
                _service.CreateMailbox("user1");
                _service.CreateMailbox("user2");
            }

            public override void Dispose()
            {
                base.Dispose();
            }

        }

        private ServerFixture _testServer;
        private Kernel _kernel;


        public SmtpClientTest(ServerFixture fixture)
        {
            //_testServer = new TestServer(TestServerType.Smtp, "SmtpServerTest\\Agent", "SmtpClientTest.ini");

            _testServer = fixture;

            //usrr2のメールボックスへの２通のメールをセット
            _testServer._service.CleanMailbox("user1");
            _testServer._service.CleanMailbox("user2");

            _kernel = _testServer._service.Kernel;

        }

        public void Dispose()
        {
        }

        private SmtpClient CreateSmtpClient(InetKind inetKind)
        {
            var kernel = _testServer._service.Kernel;
            if (inetKind == InetKind.V4)
            {
                return new SmtpClient(kernel, new Ip(IpKind.V4Localhost), _testServer.port, 3, this);
            }
            return new SmtpClient(kernel, new Ip(IpKind.V6Localhost), _testServer.port, 3, this);
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
            Assert.Equal(sut.Data(new Mail(_kernel)), true);

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
            sut.Data(new Mail(_kernel));
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

            var mail1 = new Mail(_kernel);
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

            var mail1 = new Mail(_kernel);
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

            var mail1 = new Mail(_kernel);
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
