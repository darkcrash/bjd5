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
    public class ServerTest : ILife, IDisposable
    {

        public class SmtpTestServer : TestServer
        {
            public SmtpTestServer() : base(TestServerType.Smtp, "ServerTest.ini")
            {
                _service.CreateMailbox("user1");
            }
        }

        private SmtpTestServer _testServer;

        public ServerTest(ITestOutputHelper output)
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
            Assert.Equal(cl.StringRecv(5, this).Substring(0, 33), bannerStr);

            //HELO
            cl.StringSend("HELO 1");

            var str = string.Format("250 localhost Helo 127.0.0.1[127.0.0.1:{0}], Pleased to meet you.\r\n", localPort);
            if (cl.LocalAddress.Address.ToString() == "::1")
            {
                str = string.Format("250 localhost Helo ::1[[::1]:{0}], Pleased to meet you.\r\n", localPort);
            }
            Assert.Equal(cl.StringRecv(3, this), str);

        }

        private void Ehlo(ISocket cl)
        {
            var localPort = cl.LocalAddress.Port; //なぜかローカルのポートアドレスは１つ小さい

            //バナー
            const string bannerStr = "220 localhost SMTP BlackJumboDog ";
            Assert.Equal(cl.StringRecv(3, this).Substring(0, 33), bannerStr);

            //EHLO
            cl.StringSend("EHLO 1");
            //var lines = Inet.RecvLines(cl, 4, this);

            var str = string.Format("250-localhost Helo 127.0.0.1[127.0.0.1:{0}], Pleased to meet you.", localPort);
            if (cl.LocalAddress.Address.ToString() == "::1")
            {
                str = string.Format("250-localhost Helo ::1[[::1]:{0}], Pleased to meet you.", localPort);
            }

            //Assert.Equal(lines[0], str);
            //Assert.Equal(lines[1], "250-8BITMIME");
            //Assert.Equal(lines[2], "250-SIZE=5000");
            //Assert.Equal(lines[3], "250 HELP");

            var line1 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(1, this)));
            var line2 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(1, this)));
            var line3 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(1, this)));
            var line4 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(1, this)));

            Assert.Equal(line1, str);
            Assert.Equal("250-8BITMIME", line2);
            Assert.Equal("250-SIZE=5000", line3);
            Assert.Equal("250 HELP", line4);

        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V4()
        {

            //var sv = _v4Sv;
            var expected = $"+ サービス中 \t                Smtp\t[127.0.0.1\t:TCP {_testServer.port}]\tThread";

            //exercise
            var actual = _testServer.ToString(InetKind.V4).Substring(0, 58);
            //verify
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V6()
        {

            //var sv = _v6Sv;
            var expected = $"+ サービス中 \t                Smtp\t[::1\t:TCP {_testServer.port}]\tThread";

            //exercise
            var actual = _testServer.ToString(InetKind.V6).Substring(0, 52);
            //verify
            Assert.Equal(expected, actual);

        }



        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void HELOコマンド(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);

            //exercise verify
            Helo(cl);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void EHLOコマンド(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);

            //exercise verify
            Ehlo(cl);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void 無効コマンド(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);
            var expected = "500 command not understood: XXX\r\n";

            //exercise
            cl.StringSend("XXX");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void HELOのパラメータ不足(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var banner = cl.StringRecv(3, this);
            var expected = "501 HELO requires domain address\r\n";

            //exercise
            cl.StringSend("HELO");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void EHLOのパラメータ不足(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            var banner = cl.StringRecv(3, this);
            var expected = "501 EHLO requires domain address\r\n";

            //exercise
            cl.StringSend("EHLO");
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void 中継は拒否される(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Helo(cl);

            cl.StringSend("MAIL From:1@1");
            cl.StringRecv(3, this);

            cl.StringSend("RCPT To:user1@other.domain");

            var expected = "553 user1@other.domain... Relay operation rejected\r\n";

            //exercise
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            cl.Close();
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void 複数行にわたるReceived行に対応(InetKind inetKind)
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

            //cl.StringSend("Received: from mail.testdomain.co.jp (unknown [127.0.0.1]) by");
            cl.StringSend("Received: from localhost (unknown [127.0.0.1]) by");
            //cl.StringRecv(5, this);

            //cl.StringSend(" IMSVA (Postfix) with ESMTP id 1F5C5D0037 for <test@testdomain.co.jp>;");
            cl.StringSend(" IMSVA (Postfix) with ESMTP id 1F5C5D0037 for <test@localhost>;");
            //cl.StringRecv(5, this);

            cl.StringSend(" Tue, 24 Feb 2015 16:28:44 +0900 (JST)");
            //cl.StringRecv(5, this);

            cl.StringSend("");
            //cl.StringRecv(5, this);

            cl.StringSend("BODY");
            //cl.StringRecv(5, this);

            cl.StringSend(".");
            cl.StringRecv(5, this);

            var expected = 1; //本文は1行になる

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
        public void 複数行にわたるReceived行_TAB_に対応(InetKind inetKind)
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

            //cl.StringSend("Received: from mail.testdomain.co.jp (unknown [127.0.0.1]) by");
            cl.StringSend("Received: from localhost (unknown [127.0.0.1]) by");
            //cl.StringRecv(5, this);

            //cl.StringSend(" IMSVA (Postfix) with ESMTP id 1F5C5D0037 for <test@testdomain.co.jp>;");
            cl.StringSend(" IMSVA (Postfix) with ESMTP id 1F5C5D0037 for <test@localhost>;");
            //cl.StringRecv(5, this);

            cl.StringSend(" Tue, 24 Feb 2015 16:28:44 +0900 (JST)");
            //cl.StringRecv(5, this);

            cl.StringSend("");
            //cl.StringRecv(5, this);

            cl.StringSend("BODY");
            //cl.StringRecv(5, this);

            cl.StringSend(".");
            cl.StringRecv(5, this);

            var expected = 1; //本文は1行になる

            //exercise
            var mail = _testServer.GetMf("user1")[0];
            var lines = Inet.GetLines(mail.GetBody());
            var actual = lines.Count;//本分の行数を取得

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
