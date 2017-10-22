using System;
using System.Collections.Generic;
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
    public class AliasTest : IDisposable
    {
        private TestService _service;
        private Kernel _kernel;
        private MailBox _mailBox;
        private List<String> _domainList;

        public AliasTest(ITestOutputHelper output)
        {
            _service = TestService.CreateTestService();
            _service.AddOutput(output);
            _kernel = _service.Kernel;
            _kernel.ListInitialize();

            _domainList = new List<string>();
            _domainList.Add("example.com");

            var datUser = new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox });
            datUser.Add(true, "user1\t3OuFXZzV8+iY6TC747UpCA==");
            datUser.Add(true, "user2\tNKfF4/Tw/WMhHZvTilAuJQ==");
            datUser.Add(true, "user3\tjNBu6GHNV633O4jMz1GJiQ==");

            var opt = _kernel.ListOption.Get("MailBox") as Bjd.Mailbox.Configurations.ConfigurationMailBox;
            //opt.user.Add(new Mailbox.Configurations.ConfigurationMailBox.userClass() {userName = "user1", password = "3OuFXZzV8+iY6TC747UpCA==" });
            //opt.user.Add(new Mailbox.Configurations.ConfigurationMailBox.userClass() {userName = "user2", password = "NKfF4/Tw/WMhHZvTilAuJQ==" });
            //opt.user.Add(new Mailbox.Configurations.ConfigurationMailBox.userClass() {userName = "user3", password = "jNBu6GHNV633O4jMz1GJiQ==" });

            var conf = new Conf(opt);
            conf.Add("user", datUser);


            //_mailBox = new MailBox(new Logger(), datUser, "c:\\tmp2\\bjd5\\SmtpServerTest\\mailbox");
            //_mailBox = new MailBox(new Logger(_kernel), datUser, _service.MailboxPath);
            _mailBox = new MailBox(_kernel, conf);

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
            sut.Add("user1", "user2,user3", new Logger(_kernel));

            var rcptList = new List<MailAddress>();
            rcptList.Add(new MailAddress("user1@example.com"));

            //exercise
            var actual = sut.Reflection(rcptList, new Logger(_kernel));
            //verify
            Assert.Equal(2, actual.Count);
            Assert.Equal("user2@example.com", actual[0].ToString());
            Assert.Equal("user3@example.com", actual[1].ToString());

        }
        [Fact]
        public void Reflectionによる宛先の変換_ヒットなし()
        {
            //setUp
            var sut = new Alias(_domainList, _mailBox);
            sut.Add("user1", "user2,user3", new Logger(_kernel));

            var rcptList = new List<MailAddress>();
            rcptList.Add(new MailAddress("user2@example.com"));

            //exercise
            var actual = sut.Reflection(rcptList, new Logger(_kernel));
            //verify
            Assert.Single(actual);
            Assert.Equal("user2@example.com", actual[0].ToString());

        }
        [Fact]
        public void Reflectionによる宛先の変換_ALL()
        {
            //setUp
            var sut = new Alias(_domainList, _mailBox);
            sut.Add("user1", "$ALL", new Logger(_kernel));

            var rcptList = new List<MailAddress>();
            rcptList.Add(new MailAddress("user1@example.com"));

            //exercise
            var actual = sut.Reflection(rcptList, new Logger(_kernel));
            //verify
            Assert.Equal(3, actual.Count);
            Assert.Equal("user1@example.com", actual[0].ToString());
            Assert.Equal("user2@example.com", actual[1].ToString());
            Assert.Equal("user3@example.com", actual[2].ToString());

        }

        [Fact]
        public void Reflectionによる宛先の変換_USER()
        {
            //setUp
            var sut = new Alias(_domainList, _mailBox);
            sut.Add("user1", "$USER,user2", new Logger(_kernel));

            var rcptList = new List<MailAddress>();
            rcptList.Add(new MailAddress("user1@example.com"));

            //exercise
            var actual = sut.Reflection(rcptList, new Logger(_kernel));
            //verify
            Assert.Equal(2, actual.Count);
            Assert.Equal("user1@example.com", actual[0].ToString());
            Assert.Equal("user2@example.com", actual[1].ToString());

        }

        [Fact]
        public void Reflectionによる宛先の変換_仮想ユーザ()
        {
            //setUp
            var sut = new Alias(_domainList, _mailBox);
            sut.Add("dmy", "user1,user2", new Logger(_kernel));
            var rcptList = new List<MailAddress>();
            rcptList.Add(new MailAddress("dmy@example.com"));

            //exercise
            var actual = sut.Reflection(rcptList, new Logger(_kernel));
            //verify
            Assert.Equal(2, actual.Count);
            Assert.Equal("user1@example.com", actual[0].ToString());
            Assert.Equal("user2@example.com", actual[1].ToString());

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
            sut.Add("dmy", "user1,user2", new Logger(_kernel));
            sut.Add("user1", "user3,user4", new Logger(_kernel));

            var rcptList = new List<MailAddress>();
            rcptList.Add(new MailAddress("dmy@example.com"));

            //exercise
            var actual = sut.IsUser(user);
            //verify
            Assert.Equal(expected, actual);

        }

    }

}
