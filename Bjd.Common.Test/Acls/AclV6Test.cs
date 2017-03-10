using Bjd.Acls;
using Bjd.Net;
using Xunit;
using Bjd;

namespace Bjd.Test.Acls
{

    public class AclV6Test
    {

        [Theory]
        [InlineData("1122:3344::/32", "1122:3344::", "1122:3344:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("1122:3344::/64", "1122:3344::", "1122:3344::ffff:ffff:ffff:ffff")]
        [InlineData("1122:3344::-1122:3355::", "1122:3344::", "1122:3355::")]
        [InlineData("1122:3355::-1122:3344::", "1122:3344::", "1122:3355::")]
        [InlineData("1122:3344::2", "1122:3344::2", "1122:3344::2")]
        [InlineData("*", "::0", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("*:*:*:*:*:*:*:*", "::0", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void Startの検証(string aclStr, string startStr, string endStr)
        {
            //setUp
            var sut = new AclV6("TAG", aclStr);
            var expected = startStr;
            //exercise
            var actual = sut.Start.ToString();
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("1122:3344::/32", "1122:3344::", "1122:3344:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("1122:3344::/64", "1122:3344::", "1122:3344::ffff:ffff:ffff:ffff")]
        [InlineData("1122:3344::-1122:3355::", "1122:3344::", "1122:3355::")]
        [InlineData("1122:3355::-1122:3344::", "1122:3344::", "1122:3355::")]
        [InlineData("1122:3344::2", "1122:3344::2", "1122:3344::2")]
        [InlineData("*", "::0", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [InlineData("*:*:*:*:*:*:*:*", "::0", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void Endの検証(string aclStr, string startStr, string endStr)
        {
            //setUp
            var sut = new AclV6("TAG", aclStr);
            var expected = endStr;
            //exercise
            var actual = sut.End.ToString();
            //verify
            Assert.Equal(expected, actual);
        }


        [Theory]
        [InlineData("1122:3344::/64", "1122:3343::", false)]
        [InlineData("1122:3344::/64", "1122:3344::1", true)]
        [InlineData("1122:3344::/64", "1122:3345::", false)]
        public void IsHitの検証(string aclStr, string ipStr, bool expected)
        {
            //setUp
            var sut = new AclV6("TAG", aclStr);
            //exercise
            var actual = sut.IsHit(new Ip(ipStr));
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("192.168.0.1")]
        [InlineData("x")]
        [InlineData("::1-234")]
        [InlineData("::1/200")]
        //[ExpectedException(typeof(ValidObjException))]
        public void 無効な文字列による初期化の例外テスト(string aclStr)
        {
            //exercise
            //new AclV6("TAG", aclStr);
            Assert.Throws<ValidObjException>(() => new AclV6("TAG", aclStr));

        }
    }

}
