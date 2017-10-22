using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Bjd;
using Bjd.Mailbox;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Test;
using Xunit;
using Bjd.SmtpServer;
using Bjd.Threading;

namespace Bjd.SmtpServer.Test
{
    public class SmtpClientAuthTest : ILife, IDisposable
    {
        public class ServerFixture : TestServer, IDisposable
        {
            public ServerFixture() : base(TestServerType.Smtp, "SmtpClientAuthTest.ini")
            {
                //usrr2のメールボックスへの２通のメールをセット
                SetMail("user1", "00635026511425888292");
                //SetMail("user1", "00635026511765086924");

            }

        }

        private ServerFixture _testServer;
        private Kernel _kernel;

        public SmtpClientAuthTest(Xunit.Abstractions.ITestOutputHelper output)
        {
            _testServer = new ServerFixture();
            _testServer._service.AddOutput(output);
            _kernel = _testServer._service.Kernel;
        }

        public void Dispose()
        {
            _testServer.Dispose();
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
        [InlineData(InetKind.V4, SmtpClientAuthKind.Login)]
        [InlineData(InetKind.V4, SmtpClientAuthKind.CramMd5)]
        [InlineData(InetKind.V4, SmtpClientAuthKind.Plain)]
        public void 正常系(InetKind inetKind, SmtpClientAuthKind kind)
        {
            //setUp
            var sut = CreateSmtpClient(inetKind);

            //exercise
            Assert.True(sut.Connect());
            Assert.True(sut.Helo());
            Assert.True(sut.Auth(kind, "user1", "user1"));
            Assert.True(sut.Mail("1@1"));
            Assert.True(sut.Rcpt("user1@example.com"));
            Assert.True(sut.Data(new Mail(_kernel)));

            Assert.True(sut.Quit());

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
            Assert.True(sut.Connect());
            Assert.True(sut.Helo());
            Assert.False(sut.Mail("1@1"));

            var expected = "530 Authentication required.\r\n";
            var actual = sut.GetLastError();
            Assert.Equal(expected, actual);

            //tearDown
            sut.Dispose();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        public void HELOの前にMAILコマンド(InetKind inetKind)
        {
            //setUp
            var sut = CreateSmtpClient(inetKind);

            //exercise
            Assert.True(sut.Connect());
            //Assert.Equal(sut.Helo(), true);
            Assert.False(sut.Mail("1@1"));

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
