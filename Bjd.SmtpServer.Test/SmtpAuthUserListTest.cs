﻿using System;
using System.IO;
using Bjd.Controls;
using Bjd.Logs;
using Bjd.Mailbox;
using Bjd.Configurations;
using Xunit;
using Bjd.SmtpServer;
using Bjd.Initialization;
using Xunit.Abstractions;

namespace Bjd.SmtpServer.Test
{
    public class SmtpAuthUserListTest : IDisposable
    {
        private TestService _service;
        private MailBox _mailBox;
        private Dat _esmtpUserList;

        public SmtpAuthUserListTest(ITestOutputHelper output)
        {
            _service = TestService.CreateTestService();
            _service.AddOutput(output);
            _service.Kernel.ListInitialize();

            ////mailBoxに"user1"を登録
            var datUser = new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox });
            datUser.Add(true, "user1\t3OuFXZzV8+iY6TC747UpCA==");
            ////_mailBox = new MailBox(new Logger(), datUser, "c:\\tmp2\\bjd5\\SmtpServerTest\\mailbox");
            //_mailBox = new MailBox(new Logger(_service.Kernel), datUser, _service.MailboxPath);


            var opt = _service.Kernel.ListOption.Get("MailBox") as Bjd.Mailbox.Configurations.ConfigurationMailBox;
            var conf = new Conf(opt);
            conf.Add("user", datUser);
            _mailBox = new MailBox(_service.Kernel, conf);


            //esmtpUserListに"user2"を登録
            _esmtpUserList = new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox });
            _esmtpUserList.Add(true, "user2\tNKfF4/Tw/WMhHZvTilAuJQ==");
        }

        public void Dispose()
        {
            _service.Dispose();
        }


        [Theory]
        [InlineData("user1", "user1", true)] //mailBoxのユーザは成功
        [InlineData("user2", "user2", false)] //esmtpUserListのユーザは失敗
        [InlineData("user1", "user2", false)]//mailBoxのユーザのパスワード間違い
        public void MailBoxが有効な場合の認証(String user, String pass, bool expected)
        {
            //setUp
            var sut = new SmtpAuthUserList(true, _mailBox, _esmtpUserList);
            //exercise
            var actual = sut.Auth(user, pass);
            //verify
            Assert.Equal(expected, actual);

        }

        [Theory]
        [InlineData("user1", "user1", false)]//mailBoxのユーザは失敗
        [InlineData("user2", "user2", true)]//esmtpUserListのユーザは成功
        [InlineData("user2", "user1", false)]//esmtpUserListのユーザのパスワード間違い
        public void EsmtpUserListが有効な場合の認証(String user, String pass, bool expected)
        {
            //setUp
            var sut = new SmtpAuthUserList(false, _mailBox, _esmtpUserList);
            //exercise
            var actual = sut.Auth(user, pass);
            //verify
            Assert.Equal(expected, actual);

        }

        [Theory]
        [InlineData("user1", "user1", true)]//mailBoxのユーザ成功する
        [InlineData("user2", "user2", false)] //mailBoxが有効な場合、_esmtpUserListは無効になる
        [InlineData("user1", "user2", false)]//mailBoxのユーザのパスワード間違い
        public void 両方有効な場合の認証(String user, String pass, bool expected)
        {
            //setUp
            var sut = new SmtpAuthUserList(true, _mailBox, _esmtpUserList);
            //exercise
            var actual = sut.Auth(user, pass);
            //verify
            Assert.Equal(expected, actual);

        }

        [Theory]
        [InlineData("user1", "user1")]//mailBoxのユーザは成功する
        [InlineData("user2", null)]//esmtpUserListのユーザは失敗する
        public void GetPass_MailBoxが有効な場合(String user, String pass)
        {
            //setUp
            var sut = new SmtpAuthUserList(true, _mailBox, _esmtpUserList);
            var expected = pass;
            //exercise
            var actual = sut.GetPass(user);
            //verify
            Assert.Equal(expected, actual);

        }

        [Theory]
        [InlineData("user1", null)]//mailBoxのユーザは失敗する
        [InlineData("user2", "user2")]//esmtpUserListのユーザは成功する
        public void GetPass_esmtpUserListが有効な場合(String user, String pass)
        {
            //setUp
            var sut = new SmtpAuthUserList(false, _mailBox, _esmtpUserList);
            var expected = pass;
            //exercise
            var actual = sut.GetPass(user);
            //verify
            Assert.Equal(expected, actual);

        }

        [Theory]
        [InlineData("user1", "user1")]//mailBoxのユーザは成功する
        [InlineData("user2", null)] //mailBoxが有効な場合esmtpUserListは無効になる
        public void GetPass_両方有効な場合(String user, String pass)
        {
            //setUp
            var sut = new SmtpAuthUserList(true, _mailBox, _esmtpUserList);
            var expected = pass;
            //exercise
            var actual = sut.GetPass(user);
            //verify
            Assert.Equal(expected, actual);

        }

    }
}
