﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Bjd.Controls;
using Bjd.Logs;
using Bjd.Mails;
using Bjd.Net;
using Bjd.Options;
using Xunit;
using Bjd.SmtpServer;
using Bjd.Services;

namespace Bjd.SmtpServer.Test
{
    public class PopBeforeSmtpTest : IDisposable
    {
        private MailBox _mailBox;
        private TestService _service;

        public PopBeforeSmtpTest()
        {
            _service = TestService.CreateTestService();
            

            var datUser = new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox });
            datUser.Add(true, "user1\t3OuFXZzV8+iY6TC747UpCA==");
            //_mailBox = new MailBox(new Logger(), datUser, "c:\\tmp2\\bjd5\\SmtpServerTest\\mailbox");
            _mailBox = new MailBox(new Logger(), datUser, _service.MailboxPath);
        }

        public void Dispose()
        {
            _service.Dispose();
        }

        [Fact]
        public void 事前にログインが無い場合_許可されない()
        {
            //setUp
            var sut = new PopBeforeSmtp(true, 10, _mailBox);
            var expected = false;

            //exercise
            var actual = sut.Auth(new Ip("127.0.0.1"));
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 事前にログインが有る場合_許可される()
        {
            //setUp
            var sut = new PopBeforeSmtp(true, 10, _mailBox);
            var ip = new Ip("192.168.0.1");
            var expected = true;

            _mailBox.Login("user1", ip);
            _mailBox.Logout("user1");

            //exercise
            var actual = sut.Auth(ip);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 事前にログインが有るが時間が経過してる場合_許可されない()
        {
            //setUp
            var sut = new PopBeforeSmtp(true, 1, _mailBox);//１秒以内にログインが必要
            var ip = new Ip("192.168.0.1");
            var expected = false;

            _mailBox.Login("user1", ip);
            _mailBox.Logout("user1");
            Thread.Sleep(1100);//ログアウトしてから１.1秒経過
            //exercise
            var actual = sut.Auth(ip);
            //verify
            Assert.Equal(expected, actual);
        }
    }
}
