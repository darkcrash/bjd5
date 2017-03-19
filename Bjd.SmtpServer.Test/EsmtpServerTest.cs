using Bjd;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.Configurations;
using Bjd.Initialization;
using Bjd.SmtpServer;
using Bjd.Test;
using Bjd.Threading;
using Bjd.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Bjd.SmtpServer.Test
{
    public class EsmtpServerTest : ILife, IDisposable, IClassFixture<EsmtpServerTest.ServerFixture>
    {
        public class ServerFixture : IDisposable
        {
            public TestService _service;
            public Server _v4Sv; //サーバ
            public Server _v6Sv; //サーバ
            public int port;

            public ServerFixture()
            {

                //MailBoxは、Smtp3ServerTest.iniの中で「c:\tmp2\bjd5\SmtpServerTest\mailbox」に設定されている
                //また、上記のMaloBoxには、user1=0件　user2=2件　のメールが着信している

                _service = TestService.CreateTestService();
                _service.SetOption("EsmtpServerTest.ini");

                var kernel = _service.Kernel;
                kernel.ListInitialize();

                var option = kernel.ListOption.Get("Smtp");
                var conf = new Conf(option);
                port = _service.GetAvailablePort(IpKind.V4Localhost, conf);


                //サーバ起動
                _v4Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
                _v4Sv.Start();
                _v6Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
                _v6Sv.Start();


                //Thread.Sleep(100);//少し余裕がないと多重でテストした場合に、サーバが起動しきらないうちにクライアントからの接続が始まってしまう。

            }

            public void Dispose()
            {
                //サーバ停止
                _v4Sv.Stop();
                _v6Sv.Stop();

                _v4Sv.Dispose();
                _v6Sv.Dispose();

                _service.Dispose();

            }

        }

        private ServerFixture _server;
        public TestService _service;
        public int port;


        public EsmtpServerTest(ServerFixture fixture)
        {
            _server = fixture;
            _service = fixture._service;
            port = fixture.port;
        }

        // ログイン失敗などで、しばらくサーバが使用できないため、TESTごとサーバを立ち上げて試験する必要がある
        public void Dispose()
        {
        }


        //DFファイルの一覧を取得する
        private string[] GetDf(string user)
        {
            //var dir = string.Format("c:\\tmp2\\bjd5\\SmtpServerTest\\mailbox\\{0}", user);
            //var dir = Path.Combine(TestDefine.Instance.TestMailboxPath, user);
            var dir = Path.Combine(_service.MailboxPath, user);
            var files = Directory.GetFiles(dir, "DF*");
            return files;
        }

        //クライアントの生成
        SockTcp CreateClient(InetKind inetKind)
        {
            if (inetKind == InetKind.V4)
            {
                return Inet.Connect(_service.Kernel, new Ip(IpKind.V4Localhost), port, 10, null);
            }
            return Inet.Connect(_service.Kernel, new Ip(IpKind.V6Localhost), port, 10, null);

        }

        private void Ehlo(SockTcp cl)
        {
            var localPort = cl.LocalAddress.Port; //なぜかローカルのポートアドレスは１つ小さい

            //バナー
            const string bannerStr = "220 localhost SMTP BlackJumboDog ";
            Assert.Equal(cl.StringRecv(5, this).Substring(0, 33), bannerStr);

            //EHLO
            cl.StringSend("EHLO 1");
            //var lines = Inet.RecvLines(cl, 4, this);

            var str = string.Format("250-localhost Helo 127.0.0.1[127.0.0.1:{0}], Pleased to meet you.", localPort);
            if (cl.LocalAddress.Address.ToString() == "::1")
            {
                str = string.Format("250-localhost Helo ::1[[::1]:{0}], Pleased to meet you.", localPort);
            }

            var line1 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(1, this)));
            var line2 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(1, this)));
            var line3 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(1, this)));
            var line4 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(1, this)));
            var line5 = Encoding.ASCII.GetString(Inet.TrimCrlf(cl.LineRecv(1, this)));

            Assert.Equal(line1, str);
            Assert.Equal(line2, "250-8BITMIME");
            Assert.Equal(line3, "250-SIZE=5000");
            Assert.Equal(line4, "250-AUTH LOGIN PLAIN CRAM-MD5");
            Assert.Equal(line5, "250 HELP");

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
        public void 正常系メール送信(InetKind inetKind)
        {
            var expected = GetDf("user1").Length + 1;

            //setUp
            var cl = CreateClient(inetKind);
            Ehlo(cl);


            //exercise
            cl.StringSend("AUTH PLAIN");
            Assert.Equal(cl.StringRecv(3, this), "334 \r\n");
            cl.StringSend(Base64.Encode("user1\0user1\0user1"));
            Assert.Equal(cl.StringRecv(3, this), "235 Authentication successful.\r\n");


            cl.StringSend("MAIL From:1@1");
            var s = cl.StringRecv(3, this);

            cl.StringSend("RCPT To:user1@example.com");
            s = cl.StringRecv(3, this);

            cl.StringSend("DATA");
            s = cl.StringRecv(3, this);

            cl.StringSend(".");
            s = cl.StringRecv(3, this);


            //exercise
            var actual = GetDf("user1").Length;

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
