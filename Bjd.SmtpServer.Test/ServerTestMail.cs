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
    public class ServerTestMail : ILife, IDisposable
    {

        public class SmtpTestServer : TestServer
        {
            public SmtpTestServer() : base(TestServerType.Smtp, "ServerTestMail.ini")
            {
                _service.CreateMailbox("user1");
            }
        }

        private SmtpTestServer _testServer;

        public ServerTestMail(ITestOutputHelper output)
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
        public void MAILコマンド_正常(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);
            var expected = "250 1@1... Sender ok\r\n";

            //exercise
            cl.StringSend("MAIL From:1@1");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void MAILコマンド_異常ー_Fromなし(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);
            var expected = "501 5.5.2 Syntax error in parameters scanning 1@1\r\n";

            //exercise
            cl.StringSend("MAIL 1@1");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void MAILコマンド_異常_メールアドレスなし(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);
            var expected = "501 Syntax error in parameters scanning \"\"\r\n";

            //exercise
            cl.StringSend("MAIL From:");
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
