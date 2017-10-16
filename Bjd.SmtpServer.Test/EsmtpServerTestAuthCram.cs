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

namespace Bjd.SmtpServer.Test
{
    public class EsmtpServerTestAuthCram : ILife, IDisposable
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
                _service.SetOption("EsmtpServerTestAuthCram.ini");

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


        public EsmtpServerTestAuthCram(Xunit.Abstractions.ITestOutputHelper output)
        {
            _server = new ServerFixture();
            _server._service.AddOutput(output);
            _service = _server._service;
            port = _server.port;
        }

        // ログイン失敗などで、しばらくサーバが使用できないため、TESTごとサーバを立ち上げて試験する必要がある
        public void Dispose()
        {
            _server.Dispose();
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
        ISocket CreateClient(InetKind inetKind)
        {
            if (inetKind == InetKind.V4)
            {
                return Inet.Connect(_service.Kernel, new Ip(IpKind.V4Localhost), port, 10, null);
            }
            return Inet.Connect(_service.Kernel, new Ip(IpKind.V6Localhost), port, 10, null);

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
        public void AUTH_CRAM_MD5認証_正常(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Ehlo(cl);


            var expected = "235 Authentication successful.\r\n";

            //exercise
            cl.StringSend("AUTH CRAM-MD5");
            var recvStr = cl.StringRecv(3, this);
            recvStr = Inet.TrimCrlf(recvStr);
            Assert.Equal(recvStr.Substring(0, 3), "334");
            var hash = Md5.Hash("user1", Base64.Decode(recvStr.Substring(4)));
            cl.StringSend(Base64.Encode(string.Format("user1 {0}", hash)));
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(InetKind.V4)]
        [InlineData(InetKind.V6)]
        public void AUTH_CRAM_MD5認証_異常(InetKind inetKind)
        {
            //setUp
            var cl = CreateClient(inetKind);
            Ehlo(cl);

            String expected = null;

            //exercise
            cl.StringSend("AUTH CRAM-MD5");
            var recvStr = cl.StringRecv(3, this);
            recvStr = Inet.TrimCrlf(recvStr);
            Assert.Equal(recvStr.Substring(0, 3), "334");
            var hash = Md5.Hash("user1", Base64.Decode(recvStr.Substring(4)));
            cl.StringSend(Base64.Encode(string.Format("user1 {0}", hash)) + "XXX"); //<=ゴミデータ追加
            var actual = cl.StringRecv(3, this);

            //verify
            Assert.Equal(expected, actual);
        }


        public bool IsLife()
        {
            return true;
        }
    }
}
