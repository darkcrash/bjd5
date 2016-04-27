using Bjd.DnsServer;
using Xunit;

namespace DnsServerTest{

    public class RrQueryTest{

        [Fact]
        public void GetDnsTypeの確認(){
            //setUp
            var expected = DnsType.A;
            var sut = new RrQuery("aaa.com", expected);
            //exercise
            var actual = sut.DnsType;
            //verify
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void ToStringの確認(){
            //setUp
            var expected = "Query A aaa.com";
            var sut = new RrQuery("aaa.com", DnsType.A);
            //exercise
            var actual = sut.ToString();
            //verify
            Assert.Equal(actual, expected);
        }
    }
}