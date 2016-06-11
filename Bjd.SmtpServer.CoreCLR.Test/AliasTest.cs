using System;
using System.Collections.Generic;
using System.IO;
using Bjd.Controls;
using Bjd.Logs;
using Bjd.Mails;
using Bjd.Options;
using Xunit;
using Bjd.SmtpServer;
using Bjd.Services;

namespace Bjd.SmtpServer.Test
{
    public class AliasTest : IDisposable
    {
        private TestService _service;
        private MailBox _mailBox;
        private List<String> _domainList;

        public AliasTest()
        {
            _service = TestService.CreateTestService();


            _domainList = new List<string>();
            _domainList.Add("example.com");


            var datUser = new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox });
            datUser.Add(true, "user1\t3OuFXZzV8+iY6TC747UpCA==");
            datUser.Add(true, "user2\tNKfF4/Tw/WMhHZvTilAuJQ==");
            datUser.Add(true, "user3\tjNBu6GHNV633O4jMz1GJiQ==");
            //_mailBox = new MailBox(new Logger(), datUser, "c:\\tmp2\\bjd5\\SmtpServerTest\\mailbox");
            _mailBox = new MailBox(new Logger(), datUser, _service.MailboxPath);

        }
        public void Dispose()
        {
            _service.Dispose();
        }

        [Fact]
        public void Reflectionによる宛先の変換_ヒットあり()
        {
            //setUp
            var sut = new Alias(_domainList, _mailBox);
            sut.Add("user1", "user2,user3", new Logger());

            var rcptList = new List<MailAddress>();
            rcptList.Add(new MailAddress("user1@example.com"));

            //exercise
            var actual = sut.Reflection(rcptList, new Logger());
            //verify
            Assert.Equal(actual.Count, 2);
            Assert.Equal(actual[0].ToString(), "user2@example.com");
            Assert.Equal(actual[1].ToString(), "user3@example.com");

        }
        [Fact]
        public void Reflectionによる宛先の変換_ヒットなし()
        {
            //setUp
            var sut = new Alias(_domainList, _mailBox);
            sut.Add("user1", "user2,user3", new Logger());

            var rcptList = new List<MailAddress>();
            rcptList.Add(new MailAddress("user2@example.com"));

            //exercise
            var actual = sut.Reflection(rcptList, new Logger());
            //verify
            Assert.Equal(actual.Count, 1);
            Assert.Equal(actual[0].ToString(), "user2@example.com");

        }
        [Fact]
        public void Reflectionによる宛先の変換_ALL()
        {
            //setUp
            var sut = new Alias(_domainList, _mailBox);
            sut.Add("user1", "$ALL", new Logger());

            var rcptList = new List<MailAddress>();
            rcptList.Add(new MailAddress("user1@example.com"));

            //exercise
            var actual = sut.Reflection(rcptList, new Logger());
            //verify
            Assert.Equal(actual.Count, 3);
            Assert.Equal(actual[0].ToString(), "user1@example.com");
            Assert.Equal(actual[1].ToString(), "user2@example.com");
            Assert.Equal(actual[2].ToString(), "user3@example.com");

        }

        [Fact]
        public void Reflectionによる宛先の変換_USER()
        {
            //setUp
            var sut = new Alias(_domainList, _mailBox);
            sut.Add("user1", "$USER,user2", new Logger());

            var rcptList = new List<MailAddress>();
            rcptList.Add(new MailAddress("user1@example.com"));

            //exercise
            var actual = sut.Reflection(rcptList, new Logger());
            //verify
            Assert.Equal(actual.Count, 2);
            Assert.Equal(actual[0].ToString(), "user1@example.com");
            Assert.Equal(actual[1].ToString(), "user2@example.com");

        }

        [Fact]
        public void Reflectionによる宛先の変換_仮想ユーザ()
        {
            //setUp
            var sut = new Alias(_domainList, _mailBox);
            sut.Add("dmy", "user1,user2", new Logger());
            var rcptList = new List<MailAddress>();
            rcptList.Add(new MailAddress("dmy@example.com"));

            //exercise
            var actual = sut.Reflection(rcptList, new Logger());
            //verify
            Assert.Equal(actual.Count, 2);
            Assert.Equal(actual[0].ToString(), "user1@example.com");
            Assert.Equal(actual[1].ToString(), "user2@example.com");

        }

        [Theory]
        [InlineData("dmy", true)]
        [InlineData("xxx", false)]
        [InlineData("user1", true)]
        [InlineData("user2", false)]
        public void IsUserによる登録ユーザの確認(String user, bool expected)
        {
            //setUp
            var sut = new Alias(_domainList, _mailBox);
            sut.Add("dmy", "user1,user2", new Logger());
            sut.Add("user1", "user3,user4", new Logger());

            var rcptList = new List<MailAddress>();
            rcptList.Add(new MailAddress("dmy@example.com"));

            //exercise
            var actual = sut.IsUser(user);
            //verify
            Assert.Equal(expected, actual);

        }

    }

}
