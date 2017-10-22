using System;
using System.IO;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using Bjd.SmtpServer;
using System.Collections.Generic;
using Bjd.Threading;
using System.Text;
using Xunit.Abstractions;

namespace Bjd.SmtpServer.Test
{
    public class ServerTestRcpt : ILife, IDisposable
    {

        public class SmtpTestServer : TestServer
        {
            public SmtpTestServer() : base(TestServerType.Smtp, "ServerTestRcpt.ini")
            {
                _service.CreateMailbox("user1");
            }
        }

        private SmtpTestServer _testServer;

        public ServerTestRcpt(ITestOutputHelper output)
        {
            _testServer = new SmtpTestServer();
            _testServer._service.AddOutput(output);
            _testServer._service.CleanMailbox("user1");

        }

        public void Dispose()
        {
            _testServer.Dispose();
        }

        //クライアントの生成
        ISocket CreateClient(InetKind inetKind)
        {
            int port = _testServer.port; //ウイルススキャンにかかるため25を避ける
            var kernel = _testServer._service.Kernel;
            if (inetKind == InetKind.V4)
            {
                return Inet.Connect(kernel, new Ip(IpKind.V4Localhost), port, 10, null);
            }
            return Inet.Connect(kernel, new Ip(IpKind.V6Localhost), port, 10, null);

        }

        private void Helo(ISocket cl)
        {
            var localPort = cl.LocalAddress.Port; //なぜかローカルのポートアドレスは１つ小さい

            //バナー
            const string bannerStr = "220 localhost SMTP BlackJumboDog ";
            Assert.Equal(cl.StringRecv(3, this).Substring(0, 33), bannerStr);

            //HELO
            cl.StringSend("HELO 1");

            var str = string.Format("250 localhost Helo 127.0.0.1[127.0.0.1:{0}], Pleased to meet you.\r\n", localPort);
            if (cl.LocalAddress.Address.ToString() == "::1")
            {
                str = string.Format("250 localhost Helo ::1[[::1]:{0}], Pleased to meet you.\r\n", localPort);
            }
            Assert.Equal(cl.StringRecv(3, this), str);

        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RCPTコマンド_正常(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);
            //MAIL
            cl.StringSend("MAIL From:1@1");
            cl.StringRecv(3, this);

            var expected = "250 user1@example.com... Recipient ok\r\n";

            //exercise
            cl.StringSend("RCPT To:user1@example.com");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RCPTコマンド_正常_ドメイン無し(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);
            //MAIL
            cl.StringSend("MAIL From:1@1");
            cl.StringRecv(3, this);

            var expected = "250 user1@example.com... Recipient ok\r\n";

            //exercise
            cl.StringSend("RCPT To:user1");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RCPTコマンド_異常_無効ユーザ(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);
            //MAIL
            cl.StringSend("MAIL From:1@1");
            cl.StringRecv(3, this);

            var expected = "550 xxx... User unknown\r\n";

            //exercise
            cl.StringSend("RCPT To:xxx@example.com");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RCPTコマンド_異常_無効ユーザ_ドメインなし(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);
            //MAIL
            cl.StringSend("MAIL From:1@1");
            cl.StringRecv(3, this);

            var expected = "550 xxx... User unknown\r\n";

            //exercise
            cl.StringSend("RCPT To:xxx");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }


        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RCPTコマンド_異常_MAILの前(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);

            var expected = "503 Need MAIL before RCPT\r\n";

            //exercise
            cl.StringSend("RCPT To:user1@example.com");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RCPTコマンド_異常_パラメータなし(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);
            //MAIL
            cl.StringSend("MAIL From:1@1");
            cl.StringRecv(3, this);

            var expected = "501 Syntax error in parameters scanning \"\"\r\n";

            //exercise
            cl.StringSend("RCPT");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void RCPTコマンド_異常_メールアドレスなし(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);
            //MAIL
            cl.StringSend("MAIL From:1@1");
            cl.StringRecv(3, this);

            var expected = "501 Syntax error in parameters scanning \"\"\r\n";

            //exercise
            cl.StringSend("RCPT To:");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }


        public bool IsLife()
        {
            return true;
        }
    }
}
