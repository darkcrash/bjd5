using System;
using System.IO;
using Bjd.Controls;
using Bjd.Logs;
using Bjd.Mailbox;
using Bjd.Configurations;
using Xunit;
using Bjd.Pop3Server;
using Bjd.Initialization;
using Bjd;
using Xunit.Abstractions;

namespace Pop3ServerTest
{
    public class ChpsTest : IDisposable
    {
        private Conf _conf;
        private MailBox _mailBox;
        private TestService _service;
        private Kernel _kernel;

        public ChpsTest(ITestOutputHelper helper)
        {
            _service = TestService.CreateTestService();
            _service.SetOption("Pop3ServerTest.ini");
            _service.AddOutput(helper);
            _service.Kernel.ListInitialize();

            var datUser = new Dat(new CtrlType[2] { CtrlType.TextBox, CtrlType.TextBox });
            datUser.Add(true, "user1\t3OuFXZzV8+iY6TC747UpCA==");
            datUser.Add(true, "user2\tNKfF4/Tw/WMhHZvTilAuJQ==");
            datUser.Add(true, "user3\tXXX");

            var option = _service.Kernel.ListOption.Get("Pop3");
            _conf = new Conf(option);
            _conf.Add("user", datUser);

            _kernel = _service.Kernel;

            //_mailBox = new MailBox(new Logger(), datUser, "c:\\tmp2\\bjd5\\Pop3Server\\mailbox");
            //_mailBox = new MailBox(new Logger(_kernel), datUser, _service.MailboxPath);
            _mailBox = _kernel.GetMailBox();
        }

        public void Dispose()
        {
            _service.Dispose();
        }

        [Theory]
        [InlineData("user1", "123")]//user1のパスワードを123に変更する
        [InlineData("user3", "123")]//user3のパスワードを123に変更する
        public void Changeによるパスワード変更_成功(string user, string pass)
        {
            //setUp
            bool expected = true;
            //exercise
            var actual = Chps.Change(user, pass, _mailBox, _conf);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("user1", "123")]//user1のパスワードを123に変更する
        [InlineData("user3", "123")]//user3のパスワードを123に変更する
        public void Changeによるパスワード変更_変更確認(string user, string pass)
        {
            //setUp
            var expected = true;
            Chps.Change(user, pass, _mailBox, _conf);
            //exercise
            var actual = _mailBox.Auth(user, pass);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("user1", null)]//無効パスワードの指定は失敗する
        [InlineData("xxx", "123")]//無効ユーザのパスワード変更は失敗する
        [InlineData(null, "123")]//無効ユーザのパスワード変更は失敗する
        public void Changeによるパスワード変更_失敗(string user, string pass)
        {
            //setUp
            bool expected = false;

            //exercise
            var actual = Chps.Change(user, pass, _mailBox, _conf);
            //verify
            Assert.Equal(expected, actual);
        }


    }
}
