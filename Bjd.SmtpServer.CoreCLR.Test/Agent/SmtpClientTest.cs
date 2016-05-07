﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd;
using Bjd.mail;
using Bjd.net;
using Xunit;
using Bjd.Common.Test;

namespace Bjd.SmtpServer.Test.Agent
{
    public class SmtpClientTest : ILife, IDisposable
    {
        private TestServer _testServer;


        public SmtpClientTest()
        {
            _testServer = new TestServer(TestServerType.Smtp, "SmtpServerTest\\Agent", "SmtpClientTest.ini");
        }

        public void Dispose()
        {
            _testServer.Dispose();
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
            var expected = 1;
            var actual = _testServer.GetDf("user1").Count();
            Assert.Equal(actual, expected);

            actual = _testServer.GetDf("user1").Count();
            Assert.Equal(actual, expected);

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
            var mail2 = _testServer.GetMf("user1")[0];
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
            var mail2 = _testServer.GetMf("user1")[0];
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
            Assert.Equal(actual, expected);

            //tearDown
            sut.Dispose();
        }

        public bool IsLife()
        {
            return true;
        }
    }
}
