using Bjd.DnsServer;
using Xunit;

namespace DnsServerTest
{


    public class RrDbTest_initLocalHost
    {

        [Fact]
        public void 件数は４件になる()
        {
            //setUp
            var sut = new RrDb();
            var expected = 5;
            //exercise
            RrDbTest.InitLocalHost(sut);
            var actual = RrDbTest.Size(sut);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void リソース確認_1番目はAレコードとなる()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            RrDbTest.InitLocalHost(sut);
            var o = (RrA)RrDbTest.Get(sut, 0);
            //verify
            Assert.Equal(o.DnsType, DnsType.A);
            Assert.Equal(o.Name, "localhost.");
            Assert.Equal(o.Ip.ToString(), "127.0.0.1");
        }

        [Fact]
        public void リソース確認_2番目はPTRレコードとなる()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            RrDbTest.InitLocalHost(sut);
            var o = (RrPtr)RrDbTest.Get(sut, 1);
            //verify
            Assert.Equal(o.DnsType, DnsType.Ptr);
            Assert.Equal(o.Name, "1.0.0.127.in-addr.arpa.");
            Assert.Equal(o.Ptr, "localhost.");
        }

        [Fact]
        public void リソース確認_3番目はAAAAレコードとなる()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            RrDbTest.InitLocalHost(sut);
            var o = (RrAaaa)RrDbTest.Get(sut, 2);
            //verify
            //Assert.That(o.getDnsType(), Is.EqualTo(DnsType.Aaaa));
            //Assert.That(o.getName(), Is.EqualTo("localhost."));
            Assert.Equal(o.Ip.ToString(), "::1");
        }

        [Fact]
        public void リソース確認_4番目はPTRレコードとなる()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            RrDbTest.InitLocalHost(sut);
            var o = (RrPtr)RrDbTest.Get(sut, 3);
            //verify
            Assert.Equal(o.DnsType, DnsType.Ptr);
            Assert.Equal(o.Name, "1.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.IP6.ARPA.");
            Assert.Equal(o.Ptr, "localhost.");
        }

        [Fact]
        public void リソース確認_5番目はNSレコードとなる()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            RrDbTest.InitLocalHost(sut);
            var o = (RrNs)RrDbTest.Get(sut, 4);
            //verify
            Assert.Equal(o.DnsType, DnsType.Ns);
            Assert.Equal(o.Name, "localhost.");
            Assert.Equal(o.NsName, "localhost.");
        }
    }
}
