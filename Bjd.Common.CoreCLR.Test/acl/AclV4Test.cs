using Bjd.acl;
using Bjd.net;
using Bjd;
using Xunit;

namespace Bjd.Common.Test.acl
{

    public class AclV4Test
    {

        //指定の要領
        //192.168.0.1
        //192.168.0.1-200
        //192.168.0.1-192.168.10.254
        //192.168.10.254-192.168.0.1（開始と終了が逆転してもＯＫ）
        //192.168.0.1/24
        //192.168.*.* 
        //*.*.*,*
        //*

        [Theory]
        [InlineData("192.168.0.1-192.168.10.254", "192.168.0.1", "192.168.10.254")]
        [InlineData("192.168.0.1-200", "192.168.0.1", "192.168.0.200")]
        [InlineData("*", "0.0.0.0", "255.255.255.255")]
        [InlineData("192.168.*.*", "192.168.0.0", "192.168.255.255")]
        [InlineData("192.168.0.*", "192.168.0.0", "192.168.0.255")]
        [InlineData("192.168.0.1/24", "192.168.0.0", "192.168.0.255")]
        [InlineData("192.168.10.254-192.168.0.1", "192.168.0.1", "192.168.10.254")]
        [InlineData("192.168.0.1", "192.168.0.1", "192.168.0.1")]
        public void Startの検証(string aclStr, string startStr, string endStr)
        {
            //setUp
            var sut = new AclV4("TAG", aclStr);
            var expected = startStr;
            //exercise
            var actual = sut.Start.ToString();
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("192.168.0.1-192.168.10.254", "192.168.0.1", "192.168.10.254")]
        [InlineData("192.168.0.1-200", "192.168.0.1", "192.168.0.200")]
        [InlineData("*", "0.0.0.0", "255.255.255.255")]
        [InlineData("192.168.*.*", "192.168.0.0", "192.168.255.255")]
        [InlineData("192.168.0.*", "192.168.0.0", "192.168.0.255")]
        [InlineData("192.168.0.1/24", "192.168.0.0", "192.168.0.255")]
        [InlineData("192.168.10.254-192.168.0.1", "192.168.0.1", "192.168.10.254")]
        [InlineData("192.168.0.1", "192.168.0.1", "192.168.0.1")]
        public void Endの検証(string aclStr, string startStr, string endStr)
        {
            //setUp
            var sut = new AclV4("TAG", aclStr);
            var expected = endStr;
            //exercise
            var actual = sut.End.ToString();
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("192.168.1.0/24", "192.168.1.0", true)]
        [InlineData("192.168.1.0/24", "192.168.1.255", true)]
        [InlineData("192.168.1.0/24", "192.168.0.255", false)]
        [InlineData("192.168.1.0/24", "192.168.2.0", false)]
        [InlineData("*", "192.168.2.0", true)]
        public void IsHitの検証(string aclStr, string ipStr, bool expected)
        {
            //setUp
            var sut = new AclV4("TAG", aclStr);
            //exercise
            var actual = sut.IsHit(new Ip(ipStr));
            //verify
            Assert.Equal(expected, actual);
        }


        [Theory]
        [InlineData("192.168.1.0.0")]
        [InlineData("::1")]
        [InlineData("x")]
        [InlineData("192.168.1.0-267")]
        [InlineData("192.168.1.0/200")]
        //[ExpectedException(typeof(ValidObjException))]
        public void 無効な文字列による初期化の例外テスト(string aclStr)
        {
            //exercise
            //new AclV4("TAG", aclStr);
            Assert.Throws<ValidObjException>(() => new AclV4("TAG", aclStr));
        }

        //public void 無効な文字列で初期化するとStatusがFalseとなる(string aclStr) {
        //    //setUp
        //    var sut = new AclV4("TAG", aclStr);
        //    //exercise
        //    var expected = false;
        //    var actual = sut.Status;
        //    //verify
        //    Assert.That(actual, Is.EqualTo(expected));
        //}




    }
}
