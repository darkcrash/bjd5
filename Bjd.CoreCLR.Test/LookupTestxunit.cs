using System.Linq;
using Xunit;
using Bjd;

namespace BjdTest {
    
    //[TestFixture(TestName = "LookupTest", Category = "Bjd.Common")]
    public class LookupTestxunit {
        
        
        //[SetUp]
        public void SetUp() {
        }
        
        //[TearDown]
        public void TearDown() {
        }


        [Fact]
        public void DnsServerTest() {
            var o = Lookup.DnsServer();
            Assert.NotEqual(o.Count,0);
            
            //デフォルトゲートウエイ確認 ※環境の違いを吸収
            if (o[0] != "192.168.0.254" && o[0] != "10.0.0.1" && o[0] != "192.168.1.1" && o[0]!="192.168.113.2" && o[0] != "192.168.9.26" && o[0] != "192.168.43.1")
            {
                //Assert.Fail();
                throw new System.Exception();
            }
            //Assert.AreEqual(o[0],"192.168.0.254");//デフォルトゲートウエイ確認
        }

        [Theory]
        [InlineData("www.sapporoworks.ne.jp", "59.106.27.208")]
        [InlineData("yahoo.co.jp", "182.22.59.229")]
        [InlineData("yahoo.co.jp", "183.79.135.206")]
        public void QueryATest(string target, string ipStr) {
            var o = Lookup.QueryA(target);
            Assert.NotEqual(o.Count,0);
            if (o.Any(s => s == ipStr)){
                return;
            }
            Assert.Contains(ipStr,o);
        }

        [Theory]
        [InlineData("google.com", "aspmx.l.google.com.")]
        [InlineData("google.com", "alt1.aspmx.l.google.com.")]
        [InlineData("google.com", "alt2.aspmx.l.google.com.")]
        [InlineData("google.com", "alt3.aspmx.l.google.com.")]
        [InlineData("google.com", "alt4.aspmx.l.google.com.")]
        [InlineData("sapporoworks.ne.jp", "spw02.sakura.ne.jp.")]
        public void QueryMxTest(string target,string answer) {
            var d = Lookup.DnsServer();
            var dnsServer = d[0];
            var o = Lookup.QueryMx(target,dnsServer);
            Assert.NotEqual(o.Count, 0);

            if (o.Any(s => s == answer)){
                return;
            }
            Assert.Contains(answer, o);
        }
    }
}
