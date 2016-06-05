using System;
using System.IO;
using Bjd.Controls;
using Bjd.Logs;
using Bjd.Mails;
using Bjd.Options;
using Xunit;
using Bjd.Pop3Server;
using Bjd.Services;

namespace Pop3ServerTest
{
    public class ChpsTest : IDisposable
    {
        private Conf _conf;
        private MailBox _mailBox;
        private TestService _service;

        public ChpsTest()
        {
            _service = TestService.CreateTestService();

            var datUser = new Dat(new CtrlType[2] { CtrlType.TextBox, CtrlType.TextBox });
            datUser.Add(true, "user1\t3OuFXZzV8+iY6TC747UpCA==");
            datUser.Add(true, "user2\tNKfF4/Tw/WMhHZvTilAuJQ==");
            datUser.Add(true, "user3\tXXX");

            _conf = new Conf();
            _conf.Add("user", datUser);

            //_mailBox = new MailBox(new Logger(), datUser, "c:\\tmp2\\bjd5\\Pop3Server\\mailbox");
            _mailBox = new MailBox(new Logger(), datUser, _service.MailboxPath);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_mailBox.Dir);
            }
            catch (Exception)
            {
                try
                {
                    Directory.Delete(_mailBox.Dir, true);
                }
                catch (Exception)
                {

                }
            }
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
