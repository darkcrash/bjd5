using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Bjd;
using Bjd.Mails;
using Bjd.Net;
using Bjd.Options;
using Bjd.Common.Test;
using Xunit;
using Bjd.SmtpServer;

namespace Bjd.SmtpServer.Test.Agent
{
    public class SmtpClientTest_Auth : ILife, IDisposable, IClassFixture<SmtpClientTest_Auth.ServerFixture>
    {
        public class ServerFixture : TestServer, IDisposable
        {
            public ServerFixture() : base(TestServerType.Smtp, "Bjd.SmtpServer.CoreCLR.Test\\Agent", "SmtpClientTest_Auth.ini")
            {
                //usrr2のメールボックスへの２通のメールをセット
                SetMail("user1", "00635026511425888292");
                //SetMail("user1", "00635026511765086924");

            }

        }

        private ServerFixture _testServer;

        public SmtpClientTest_Auth(ServerFixture fixture)
        {
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
        [InlineData(InetKind.V4, SmtpClientAuthKind.Login)]
        [InlineData(InetKind.V4, SmtpClientAuthKind.CramMd5)]
        [InlineData(InetKind.V4, SmtpClientAuthKind.Plain)]
        public void 正常系(InetKind inetKind, SmtpClientAuthKind kind)
        {
            //setUp
            var sut = CreateSmtpClient(inetKind);

            //exercise
            Assert.Equal(sut.Connect(), true);
            Assert.Equal(sut.Helo(), true);
            Assert.Equal(sut.Auth(kind, "user1", "user1"), true);
            Assert.Equal(sut.Mail("1@1"), true);
            Assert.Equal(sut.Rcpt("user1@example.com"), true);
            Assert.Equal(sut.Data(new Mail()), true);

            Assert.Equal(sut.Quit(), true);

            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        public void 認証前にMAILコマンド(InetKind inetKind)
        {
            //setUp
            var sut = CreateSmtpClient(inetKind);

            //exercise
            Assert.Equal(sut.Connect(), true);
            Assert.Equal(sut.Helo(), true);
            Assert.Equal(sut.Mail("1@1"), false);

            var expected = "530 Authentication required.\r\n";
            var actual = sut.GetLastError();
            Assert.Equal(expected, actual);

            //tearDown
            sut.Dispose();
        }

        [InlineData(InetKind.V4)]
        public void HELOの前にMAILコマンド(InetKind inetKind)
        {
            //setUp
            var sut = CreateSmtpClient(inetKind);

            //exercise
            Assert.Equal(sut.Connect(), true);
            //Assert.Equal(sut.Helo(), true);
            Assert.Equal(sut.Mail("1@1"), false);

            var expected = "Mail() Status != Transaction";
            var actual = sut.GetLastError();
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
