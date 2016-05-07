using System.Linq;
using Bjd.mail;
using Xunit;

namespace Bjd.Common.Test.mail
{

    public class MailAddressTest
    {

        [Theory]
        [InlineData("", "", "")]
        [InlineData("user1", "user1", "")]
        [InlineData("user1@example.com", "user1", "example.com")]
        [InlineData("user1@example,jp\b\b\b.jp", "user1", "example.jp")] //バックスペースで修正されて入力
        public void コンストラクタによる名前とドメイン名の初期化(string mailaddress, string user, string domain)
        {
            //setUp
            var sut = new MailAddress(mailaddress);
            //verify
            Assert.Equal(sut.User, user);
            Assert.Equal(sut.Domain, domain);
        }

        [Theory]
        [InlineData("1@aaa.com", new[] { "aaa.com" }, true)]
        [InlineData("1@aaa.com", new[] { "bbb.com" }, false)]
        [InlineData("1@aaa.com", new[] { "aaa.com", "bbb.com" }, true)]
        [InlineData("1@bbb.com", new[] { "aaa.com", "bbb.com" }, true)]
        [InlineData("1", new[] { "aaa.com", "bbb.com" }, false)]
        public void IsLocalによるドメインに属するかどうかの確認(string mailaddress, string[] domainList, bool expected)
        {
            //setUp
            var sut = new MailAddress(mailaddress);
            //exercise
            var actual = sut.IsLocal(domainList.ToList());
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("user@aaa.com", "user", "aaa.com")]
        [InlineData("<user@aaa.com>", "user", "aaa.com")]
        [InlineData("user", "user", "")]
        [InlineData("", "", "")]
        [InlineData("\"<user@aaa.com>\"", "user", "aaa.com")]
        [InlineData("\"名前<user@aaa.com>\"", "user", "aaa.com")]
        [InlineData("\" 名前 <user@aaa.com> \"", "user", "aaa.com")]
        public void コンストラクタによる初期化(string str, string user, string domain)
        {
            //setUp
            var sut = new MailAddress(str);
            //exercise
            Assert.Equal(sut.User, user);
            Assert.Equal(sut.Domain, domain);
        }

    }

}
