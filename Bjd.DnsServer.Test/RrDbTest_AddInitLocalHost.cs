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
            Assert.Equal(DnsType.A, o.DnsType);
            Assert.Equal("localhost.", o.Name);
            Assert.Equal("127.0.0.1", o.Ip.ToString());
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
            Assert.Equal(DnsType.Ptr, o.DnsType);
            Assert.Equal("1.0.0.127.in-addr.arpa.", o.Name);
            Assert.Equal("localhost.", o.Ptr);
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
            Assert.Equal("::1", o.Ip.ToString());
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
            Assert.Equal(DnsType.Ptr, o.DnsType);
            Assert.Equal("1.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.IP6.ARPA.", o.Name);
            Assert.Equal("localhost.", o.Ptr);
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
            Assert.Equal(DnsType.Ns, o.DnsType);
            Assert.Equal("localhost.", o.Name);
            Assert.Equal("localhost.", o.NsName);
        }
    }
}
