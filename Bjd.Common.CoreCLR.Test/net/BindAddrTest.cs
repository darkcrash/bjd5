using Bjd;
using Bjd.Net;
using Xunit;

namespace Bjd.Test.Net
{

    public class BindAddrTest
    {

        [Fact]
        public void Totostringによる確認()
        {
            //setUp
            var expected = "V4Only,INADDR_ANY,IN6ADDR_ANY_INIT";
            var sut = new BindAddr();
            //exercise
            var actual = sut.ToString();
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(BindStyle.V4Only, "192.168.0.1", "::1", "V4Only,192.168.0.1,::1")]
        [InlineData(BindStyle.V46Dual, "0.0.0.0", "ffe0::1", "V46Dual,0.0.0.0,ffe0::1")]
        public void パラメータBindStyle_ipv4_ipv6でnewしたBindAddrオブジェクトをToStringで確認する(
            BindStyle bindStyle, string ipV4, string ipV6, string expected)
        {

            //setUp
            var sut = new BindAddr(bindStyle, new Ip(ipV4), new Ip(ipV6));
            //exercise
            var actual = sut.ToString();
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("V4ONLY,192.168.0.1,::1", "V4Only,192.168.0.1,::1")]
        public void 文字列でnewしたBindAddrオブジェクトをToStringで確認する(string bindStr, string expected)
        {
            //setUp
            var sut = new BindAddr(bindStr);
            //exercise
            var actual = sut.ToString();
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("XXX,INADDR_ANY,IN6ADDR_ANY_INIT")] // 無効な列挙名
        [InlineData("V4ONLY,INADDR_ANY,192.168.0.1")] // IpV6にV4のアドレスを指定
        [InlineData("V4ONLY,::1,IN6ADDR_ANY_INIT")] // IpV4にV6のアドレスを指定
        public void 効文字列を指定してBindAddresをnewすると発生する例外テスト(string bindStr)
        {
            try
            {
                new BindAddr(bindStr);
                Assert.False(true, "この行が実行されたらエラー");
            }
            catch (ValidObjException) { }
        }

        [Theory]
        [InlineData("V4Only,INADDR_ANY,IN6ADDR_ANY_INIT", "V4Only,INADDR_ANY,IN6ADDR_ANY_INIT", true)]
        [InlineData("V4Only,INADDR_ANY,::1", "V4Only,INADDR_ANY,::2", false)]
        [InlineData("V4Only,INADDR_ANY,::1", null, false)]
        [InlineData("V4Only,INADDR_ANY,::1", "V4Only,0.0.0.1,::1", false)]
        [InlineData("V6Only,0.0.0.1,::1", "V4Only,0.0.0.1,::1", false)]
        public void Equalsによる同一確認(string bindStr1, string bindStr2, bool expected)
        {

            //setUp
            var sut = new BindAddr(bindStr1);
            var target = (bindStr2 == null) ? null : new BindAddr(bindStr2);

            //exercise
            var actual = sut.Equals(target);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("V4ONLY,INADDR_ANY,IN6ADDR_ANY_INIT", "V4ONLY,INADDR_ANY,IN6ADDR_ANY_INIT", true)]
        [InlineData("V4ONLY,INADDR_ANY,::1", "V4ONLY,INADDR_ANY,::2", true)]
        [InlineData("V4ONLY,INADDR_ANY,::1", "V4ONLY,0.0.0.1,::1", true)]
        [InlineData("V6ONLY,0.0.0.1,::1", "V4ONLY,0.0.0.1,::1", false)]
        [InlineData("V46DUAL,0.0.0.1,::1", "V4ONLY,0.0.0.1,::1", true)]
        public void CheckCompetitionによる競合があるかどうかの確認(string bindStr1, string bindStr2, bool expected)
        {

            //setUp
            var sut = new BindAddr(bindStr1);
            var target = (bindStr2 == null) ? null : new BindAddr(bindStr2);

            //exercise
            var actual = sut.CheckCompetition(target);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("V4ONLY,INADDR_ANY,::1", ProtocolKind.Tcp, 1, "INADDR_ANY-Tcp")]
        [InlineData("V4ONLY,INADDR_ANY,::1", ProtocolKind.Udp, 1, "INADDR_ANY-Udp")]
        [InlineData("V4ONLY,0.0.0.1,::1", ProtocolKind.Tcp, 1, "0.0.0.1-Tcp")]
        [InlineData("V6ONLY,0.0.0.1,::1", ProtocolKind.Tcp, 1, "::1-Tcp")]
        [InlineData("V46DUAL,0.0.0.1,::1", ProtocolKind.Tcp, 2, "::1-Tcp")]
        public void CreateOneBindで生成される数の確認(string bindStr, ProtocolKind protocolKind, int count, string firstOneBind)
        {
            //stUp
            var sut = new BindAddr(bindStr);
            var expected = count;

            //exercise
            var ar = sut.CreateOneBind(protocolKind);
            var actual = ar.Length;
            //verify
            Assert.Equal(expected, actual);

        }

        [Theory]
        [InlineData("V4ONLY,INADDR_ANY,::1", ProtocolKind.Tcp, 1, "INADDR_ANY-Tcp")]
        [InlineData("V4ONLY,INADDR_ANY,::1", ProtocolKind.Udp, 1, "INADDR_ANY-Udp")]
        [InlineData("V4ONLY,0.0.0.1,::1", ProtocolKind.Tcp, 1, "0.0.0.1-Tcp")]
        [InlineData("V6ONLY,0.0.0.1,::1", ProtocolKind.Tcp, 1, "::1-Tcp")]
        [InlineData("V46DUAL,0.0.0.1,::1", ProtocolKind.Tcp, 2, "::1-Tcp")]
        public void CreateOneBindで生成される最初のOneBindの確認(string bindStr, ProtocolKind protocolKind, int count,
                                                     string firstOneBind)
        {
            //stUp
            var sut = new BindAddr(bindStr);
            var expected = firstOneBind;

            //exercise
            var ar = sut.CreateOneBind(protocolKind);
            var actual = ar[0].ToString();

            //verify
            Assert.Equal(expected, actual);

        }

    }
}
