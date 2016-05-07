using Bjd.net;
using Xunit;
using Bjd;

namespace BjdTest.net
{
    public class IpTest
    {

        [Theory]
        [InlineData("192.168.0.1", "192.168.0.1")]
        [InlineData("255.255.0.254", "255.255.0.254")]
        [InlineData("INADDR_ANY", "INADDR_ANY")]//DOTO
        [InlineData("0.0.0.0", "0.0.0.0")]
        [InlineData("IN6ADDR_ANY_INIT", "IN6ADDR_ANY_INIT")]
        [InlineData("::", "::0")]//DOTO
        [InlineData("::1", "::1")]
        [InlineData("::809f", "::809f")]
        [InlineData("ff34::809f", "ff34::809f")]
        [InlineData("1234:56::1234:5678:90ab", "1234:56::1234:5678:90ab")]
        [InlineData("fe80::7090:40f5:96f7:17db%13", "fe80::7090:40f5:96f7:17db%13")]//Ver5.4.9
        [InlineData("12::78:90ab", "12::78:90ab")]
        [InlineData("[12::78:90ab]", "12::78:90ab")]//[括弧付きで指定された場合]
        [InlineData("fff::", "fff::")]
        [InlineData("::192.168.0.1", "::c0a8:1")] //Ver6.1.2 IPv4互換アドレス対応
        [InlineData("::ffbf:192.168.0.1", "::ffbf:c0a8:1")] //Ver6.1.2 IPv4射影アドレス対応
        public void 文字列のコンストラクタで生成してToStringで確認する(string ipStr, string expected)
        {
            //setUp
            var sut = new Ip(ipStr);
            //exercise
            var actual = sut.ToString();
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("192.168.0.1", "192.168.0.1")]
        [InlineData("255.255.0.254", "255.255.0.254")]
        [InlineData("INADDR_ANY", "0.0.0.0")]
        [InlineData("0.0.0.0", "0.0.0.0")]
        [InlineData("IN6ADDR_ANY_INIT", "::")]
        [InlineData("::", "::")]
        [InlineData("::1", "::1")]
        [InlineData("::809f", "::809f")]
        [InlineData("ff34::809f", "ff34::809f")]
        [InlineData("1234:56::1234:5678:90ab", "1234:56::1234:5678:90ab")]
        [InlineData("fe80::7090:40f5:96f7:17db%13", "fe80::7090:40f5:96f7:17db%13")]
        [InlineData("12::78:90ab", "12::78:90ab")]
        [InlineData("[12::78:90ab]", "12::78:90ab")]//[括弧付きで指定された場合]
        public void 文字列のコンストラクタで生成してIPAddress_ToStringで確認する(string ipStr, string expected)
        {
            //setUp
            var sut = new Ip(ipStr);
            //exercise
            var actual = sut.IPAddress.ToString();
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(192, 168, 0, 1)]
        [InlineData(127, 0, 0, 1)]
        [InlineData(0, 0, 0, 0)]
        [InlineData(255, 255, 255, 255)]
        [InlineData(255, 255, 0, 254)]
        public void プロパティIpV4の確認(int n1, int n2, int n3, int n4)
        {
            //setUp
            var ipStr = string.Format("{0}.{1}.{2}.{3}", n1, n2, n3, n4);
            var sut = new Ip(ipStr);
            //exercise
            var p = sut.IpV4;
            //verify
            Assert.Equal(p[0], n1);
            Assert.Equal(p[1], n2);
            Assert.Equal(p[2], n3);
            Assert.Equal(p[3], n4);
        }

        [Theory]
        [InlineData("1234:56::1234:5678:90ab", 0x12, 0x34, 0x00, 0x56, 0, 0, 0, 0, 0, 0, 0x12, 0x34, 0x56, 0x78, 0x90, 0xab)]
        [InlineData("1::1", 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1)]
        [InlineData("ff04::f234", 0xff, 0x04, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xf2, 0x34)]
        [InlineData("1::1%16", 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1)]
        [InlineData("[1::1]", 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1)]
        public void プロパティIpV6の確認(string ipStr, int n1, int n2, int n3, int n4, int n5, int n6, int n7, int n8, int n9, int n10, int n11, int n12, int n13, int n14, int n15, int n16)
        {
            //setUp
            var sut = new Ip(ipStr);
            //exercise
            var p = sut.IpV6;
            //verify
            Assert.Equal(p[0], n1);
            Assert.Equal(p[1], n2);
            Assert.Equal(p[2], n3);
            Assert.Equal(p[3], n4);
            Assert.Equal(p[4], n5);
            Assert.Equal(p[5], n6);
            Assert.Equal(p[6], n7);
            Assert.Equal(p[7], n8);
            Assert.Equal(p[8], n9);
            Assert.Equal(p[9], n10);
            Assert.Equal(p[10], n11);
            Assert.Equal(p[11], n12);
            Assert.Equal(p[12], n13);
            Assert.Equal(p[13], n14);
            Assert.Equal(p[14], n15);
            Assert.Equal(p[15], n16);
        }

        [Theory]
        [InlineData("192.168.0.1", "192.168.0.1", true)]
        [InlineData("192.168.0.1", "192.168.0.2", false)]
        [InlineData("192.168.0.1", null, false)]
        [InlineData("::1", "::1", true)]
        [InlineData("::1%1", "::1%1", true)]
        [InlineData("::1%1", "::1", false)]
        [InlineData("ff01::1", "::1", false)]
        [InlineData("::1", null, false)]
        public void 演算子イコールの判定_null判定(string ipStr, string targetStr, bool expected)
        {
            //setUp
            var sut = new Ip(ipStr);
            Ip target = null;
            if (targetStr != null)
            {
                target = new Ip(targetStr);
            }
            //exercise
            var actual = (sut == target);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("1.2.3.4")]
        [InlineData("192.168.0.1")]
        [InlineData("255.255.255.255")]
        [InlineData("INADDR_ANY")]
        public void AddrV4で取得した値からIpオブジェクトを再構築する(string ipStr)
        {
            //setUp
            var sut = new Ip((new Ip(ipStr)).AddrV4);
            var expected = ipStr;
            //exercise
            var actual = sut.ToString();
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("102:304:506:708:90a:b0c:d0e:f01")]
        [InlineData("ff83::e:f01")]
        [InlineData("::1")]
        [InlineData("fff::")]
        public void AddrV6HとAddrV6Lで取得した値からIpオブジェクトを再構築する(string ipStr)
        {
            //setUp
            var ip = new Ip(ipStr);
            var sut = new Ip(ip.AddrV6H, ip.AddrV6L);
            var expected = ipStr;
            //exercise
            var actual = sut.ToString();
            //verify
            Assert.Equal(expected, actual);
        }


        [Theory]
        [InlineData("")]
        [InlineData("IN_ADDR_ANY")]
        [InlineData("xxx")]
        [InlineData("192.168.0.1.2")]
        [InlineData(null)]
        [InlineData("11111::")]
        //[ExpectedException(typeof(ValidObjException))]
        public void 無効な文字列による初期化の例外テスト(string ipStr)
        {
            //exercise
            //new Ip(ipStr);
            Assert.Throws<ValidObjException>(() => new Ip(ipStr));
        }

        [Theory]
        [InlineData("192.168.0.1", 0xc0a80001)]
        public void AddrV4の検証(string ipStr, uint ip)
        {
            //setUp
            var sut = new Ip(ipStr);
            var expected = ip;
            //exercise
            var actual = sut.AddrV4;
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("1234:56::1234:5678:90ab", 0x1234005600000000uL, 0x00001234567890abuL)]
        public void AddrV6の検証(string ipStr, ulong v6h, ulong v6l)
        {
            //setUp
            var sut = new Ip(ipStr);
            //exercise
            ulong h = sut.AddrV6H;
            ulong l = sut.AddrV6L;
            //verify
            Assert.Equal(h, v6h);
            Assert.Equal(l, v6l);
        }

    }
}
