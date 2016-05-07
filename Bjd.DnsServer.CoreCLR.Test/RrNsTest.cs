using BjdTest.test;
using Bjd.DnsServer;
using Xunit;

namespace DnsServerTest
{


    public class RrNsTest
    {

        //NS class=1 ttl=0x000002b25 ns2.google.com
        private const string Str0 = "0002000100002b250010036e733206676f6f676c6503636f6d00";

        [Fact]
        public void GetNsNameの確認()
        {
            //setUp
            var expected = "ns.google.com.";
            var sut = new RrNs("aaa.com", 0, expected);
            //exercise
            var actual = sut.NsName;
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void バイナリ初期化との比較()
        {
            //setUp
            var sut = new RrNs("aaa.com", 64800, "1.");
            var expected = (new RrNs("aaa.com", 64800, new byte[] { 01, 49, 0 })).ToString();
            //exercise
            var actual = sut.ToString();
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 実パケット生成したオブジェクトとの比較()
        {
            //setUp
            var sut = new RrNs("aaa.com", 0x00002b25, "ns2.google.com");
            var rr = new PacketRr(TestUtil.HexStream2Bytes(Str0), 0);
            var expected = (new RrNs("aaa.com", rr.Ttl, rr.Data)).ToString();
            //exercise
            var actual = sut.ToString();
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToStringの確認()
        {
            //setUp
            var expected = "Ns aaa.com TTL=0 ns.google.com.";
            var sut = new RrNs("aaa.com", 0, "ns.google.com.");
            //exercise
            var actual = sut.ToString();
            //verify
            Assert.Equal(expected, actual);
        }
    }
}
