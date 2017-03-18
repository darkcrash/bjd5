using System;
using System.Collections.Generic;
using Bjd.Mailbox;
using Xunit;
using Bjd.SmtpServer;
using Bjd;

namespace Bjd.SmtpServer.Test
{

    public class MlAddrTest : IDisposable
    {

        MlAddr _mlAddr;//テスト対象クラス

        public MlAddrTest()
        {
            _mlAddr = new MlAddr("1ban", new List<string> { "example.com" });
        }

        public void Dispose()
        {

        }

        [Theory]
        [InlineData("1ban-admin@example.com", MlAddrKind.Admin)]
        [InlineData("1ban-ctl@example.com", MlAddrKind.Ctrl)]
        [InlineData("1ban@example.com", MlAddrKind.Post)]
        public void MlAddressTest(string mailAddress, MlAddrKind mlAddrKind)
        {
            switch (mlAddrKind)
            {
                case MlAddrKind.Admin:
                    Assert.Equal(mailAddress, _mlAddr.Admin.ToString());
                    break;
                case MlAddrKind.Ctrl:
                    Assert.Equal(mailAddress, _mlAddr.Ctrl.ToString());
                    break;
                case MlAddrKind.Post:
                    Assert.Equal(mailAddress, _mlAddr.Post.ToString());
                    break;
            }
        }


        [Theory]
        [InlineData("1ban-admin@example.com", MlAddrKind.Admin)]
        [InlineData("1ban-ctl@example.com", MlAddrKind.Ctrl)]
        [InlineData("1ban@example.com", MlAddrKind.Post)]
        [InlineData("1@1", MlAddrKind.None)]
        [InlineData("admin@example.com", MlAddrKind.None)]
        [InlineData("ctl-1ban@example.com", MlAddrKind.None)]
        public void GetKindTest(string mailAddress, MlAddrKind kind)
        {
            Assert.Equal(_mlAddr.GetKind(new MailAddress(mailAddress)), kind);
        }

        [Theory]
        [InlineData("1ban-admin@example.com", true)]
        [InlineData("1ban-ctl@example.com", true)]
        [InlineData("admin@example.com", false)]
        [InlineData("ctl-1ban@example.com", false)]
        [InlineData("1ban@example.com", true)]
        [InlineData("1@1", false)]
        public void IsUserTest(string mailAddress, bool isUser)
        {
            Assert.Equal(_mlAddr.IsUser(new MailAddress(mailAddress)), isUser);
        }
    }
}
