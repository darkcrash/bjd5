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

namespace Bjd.SmtpServer.Test
{
    public class ServerTestData : ILife, IDisposable, IClassFixture<ServerTestData.SmtpTestServer>
    {

        public class SmtpTestServer : TestServer
        {
            public SmtpTestServer() : base(TestServerType.Smtp, "ServerTest.ini")
            {
                _service.CreateMailbox("user1");
            }
        }

        private SmtpTestServer _testServer;

        public ServerTestData(SmtpTestServer server)
        {
            _testServer = server;
            _testServer._service.CleanMailbox("user1");

        }

        public void Dispose()
        {

        }

        //クライアントの生成
        SockTcp CreateClient(InetKind inetKind)
        {
            int port = _testServer.port; //ウイルススキャンにかかるため25を避ける
            var kernel = _testServer._service.Kernel;
            if (inetKind == InetKind.V4)
            {
                return Inet.Connect(kernel, new Ip(IpKind.V4Localhost), port, 10, null);
            }
            return Inet.Connect(kernel, new Ip(IpKind.V6Localhost), port, 10, null);

        }

        private void Helo(SockTcp cl)
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
        public void DATAコマンド_正常(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);
            //MAIL
            cl.StringSend("MAIL From:1@1");
            cl.StringRecv(3, this);
            //RCPT
            cl.StringSend("RCPT To:user1@example.com");
            cl.StringRecv(3, this);

            var expected = "354 Enter mail,end with \".\" on a line by ltself\r\n";

            //exercise
            cl.StringSend("DATA");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }


        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DATAコマンド_正常_送信(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);
            //MAIL
            cl.StringSend("MAIL From:1@1");
            cl.StringRecv(3, this);
            //RCPT
            cl.StringSend("RCPT To:user1@example.com");
            cl.StringRecv(3, this);

            cl.StringSend("DATA");
            cl.StringRecv(3, this);


            var expected = "250 OK\r\n";

            //exercise
            cl.StringSend(".");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DATAコマンド_正常_メールボックス確認(InetKind inetKind)
        {
            var expected = _testServer.GetDf("user1").Length + 1;

            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);

            cl.StringSend("MAIL From:1@1");
            var l0 = cl.StringRecv(5, this);
            cl.StringSend("RCPT To:user1@example.com");
            var l1 = cl.StringRecv(5, this);

            cl.StringSend("DATA");
            var l2 = cl.StringRecv(5, this);

            cl.StringSend(".");
            var l3 = cl.StringRecv(5, this);

            //exercise
            var actual = _testServer.GetDf("user1").Length;

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }


        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DATAコマンド_空行あり_正常_メールボックス確認(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);

            cl.StringSend("MAIL From:1@1");
            var l0 = cl.StringRecv(5, this);
            cl.StringSend("RCPT To:user1@example.com");
            var l1 = cl.StringRecv(5, this);

            cl.StringSend("DATA");
            var l2 = cl.StringRecv(5, this);

            cl.StringSend("Subject:TEST");
            //var l3 = cl.StringRecv(5, this);

            cl.StringSend("");
            //var l4 = cl.StringRecv(5, this);

            cl.StringSend("body-1");//本文１行目
            //var l5 = cl.StringRecv(5, this);

            cl.StringSend("body-2");//本文２行目
            //var l6 = cl.StringRecv(5, this);

            cl.StringSend(".");
            var l7 = cl.StringRecv(5, this);

            var expected = 2; //本文は２行になる

            //exercise
            var mail = _testServer.GetMf("user1")[0];
            var lines = Inet.GetLines(mail.GetBody());

            var actual = lines.Count;//本分の行数を取得

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }


        //ヘッダとボディの間に空白行が無い場合に対応
        [Theory]
        [InlineData(InetKind.V4)]
        //[InlineData(InetKind.V6)]
        public void DATAコマンド_空行なし_正常_メールボックス確認(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);

            cl.StringSend("MAIL From:1@1");
            var l0 = cl.StringRecv(5, this);
            cl.StringSend("RCPT To:user1@example.com");
            var l1 = cl.StringRecv(5, this);

            cl.StringSend("DATA");
            var l2 = cl.StringRecv(5, this);

            cl.StringSend("Subject:TEST");
            //var l3 = cl.StringRecv(5, this);

            //cl.StringSend("");
            //var l4 = cl.StringRecv(5, this);

            cl.StringSend("body-1");//本文１行目
            //var l5 = cl.StringRecv(5, this);

            cl.StringSend("body-2");//本文２行目
            //var l6 = cl.StringRecv(5, this);

            cl.StringSend(".");
            var l7 = cl.StringRecv(5, this);

            var expected = 2; //本文は２行になる

            //exercise
            var mail = _testServer.GetMf("user1")[0];
            var lines = Inet.GetLines(mail.GetBody());
            var actual = lines.Count;//本分の行数を取得

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }


        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DATAコマンド_正常_連続２通(InetKind inetKind)
        {
            var expected = _testServer.GetDf("user1").Length + 2;

            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);

            cl.StringSend("MAIL From:1@1");
            cl.StringRecv(3, this);

            cl.StringSend("RCPT To:user1@example.com");
            cl.StringRecv(3, this);

            cl.StringSend("DATA");
            cl.StringRecv(3, this);

            cl.StringSend(".");
            cl.StringRecv(3, this);

            cl.StringSend("RCPT To:user1@example.com");
            cl.StringRecv(3, this);

            cl.StringSend("DATA");
            cl.StringRecv(3, this);

            cl.StringSend(".");
            cl.StringRecv(3, this);


            //exercise
            var actual = _testServer.GetDf("user1").Length;

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DATAコマンド_異常_MAILの前(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);

            var expected = "503 Need MAIL command\r\n";

            //exercise
            cl.StringSend("DATA");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void DATAコマンド_異常_RCPTの前(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);
            //MAIL
            cl.StringSend("MAIL From:1@1");
            cl.StringRecv(3, this);

            var expected = "503 Need RCPT (recipient)\r\n";

            //exercise
            cl.StringSend("DATA");
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
