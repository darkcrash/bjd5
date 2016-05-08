using Bjd;
using Bjd.Option;
using Bjd.Utils;
using Bjd.DnsServer;
using Xunit;

namespace DnsServerTest
{


    public class RrDbTest_addOneDat
    {
        private readonly bool[] _isSecret = new[] { false, false, false, false, false };
        private const string DomainName = "aaa.com.";

        //共通メソッド
        //リソースレコードのtostring()
        private string print(OneRr o)
        {
            switch (o.DnsType)
            {
                case DnsType.A:
                    return o.ToString();
                case DnsType.AAAA:
                    return o.ToString();
                case DnsType.Ns:
                    return o.ToString();
                case DnsType.Mx:
                    return o.ToString();
                case DnsType.Ptr:
                    return o.ToString();
                case DnsType.Soa:
                    return o.ToString();
                case DnsType.Cname:
                    return o.ToString();
                default:
                    Util.RuntimeException("not implement.");
                    break;
            }
            return "";
        }

        [Fact]
        public void Aレコードを読み込んだ時_A及びPTRが保存される()
        {

            //setUp
            var sut = new RrDb();
            var oneDat = new OneDat(true, new[] { "0", "www", "alias", "192.168.0.1", "10" }, _isSecret);
            //exercise
            RrDbTest.AddOneDat(sut, DomainName, oneDat);

            //verify
            Assert.Equal(RrDbTest.Size(sut), 2); //A,PTR
            Assert.Equal(print(RrDbTest.Get(sut, 0)), "A www.aaa.com. TTL=0 192.168.0.1");
            Assert.Equal(print(RrDbTest.Get(sut, 1)), "Ptr 1.0.168.192.in-addr.arpa. TTL=0 www.aaa.com.");

        }

        [Fact]
        public void AAAAレコードを読み込んだ時_AAAA及びPTRが保存される()
        {
            //setUp
            var sut = new RrDb();
            var oneDat = new OneDat(true, new[] { "4", "www", "alias", "fe80::f509:c5be:437b:3bc5", "10" }, _isSecret);
            //exercise
            RrDbTest.AddOneDat(sut, DomainName, oneDat);

            //verify
            Assert.Equal(RrDbTest.Size(sut), 2); //AAAA,PTR
            Assert.Equal(print(RrDbTest.Get(sut, 0)), "AAAA www.aaa.com. TTL=0 fe80::f509:c5be:437b:3bc5");
            Assert.Equal(print(RrDbTest.Get(sut, 1)), "Ptr 5.c.b.3.b.7.3.4.e.b.5.c.9.0.5.f.0.0.0.0.0.0.0.0.0.0.0.0.0.8.e.f.ip6.arpa. TTL=0 www.aaa.com.");
        }

        [Fact]
        public void MXレコードを読み込んだ時_MX_A及びPTRが保存される()
        {
            //setUp
            var sut = new RrDb();
            var oneDat = new OneDat(true, new[] { "2", "smtp", "alias", "210.10.2.250", "15" }, _isSecret);
            //exercise
            RrDbTest.AddOneDat(sut, DomainName, oneDat);

            //verify
            Assert.Equal(RrDbTest.Size(sut), 3); //MX,A,PTR
            Assert.Equal(print(RrDbTest.Get(sut, 0)), "Mx aaa.com. TTL=0 15 smtp.aaa.com.");
            Assert.Equal(print(RrDbTest.Get(sut, 1)), "A smtp.aaa.com. TTL=0 210.10.2.250");
            Assert.Equal(print(RrDbTest.Get(sut, 2)), "Ptr 250.2.10.210.in-addr.arpa. TTL=0 smtp.aaa.com.");
        }

        [Fact]
        public void NSレコードを読み込んだ時_NS_A及びPTRが保存される()
        {
            //setUp
            var sut = new RrDb();
            var oneDat = new OneDat(true, new[] { "1", "ns", "alias", "111.3.255.0", "0" }, _isSecret);
            //exercise
            RrDbTest.AddOneDat(sut, DomainName, oneDat);

            //verify count
            Assert.Equal(RrDbTest.Size(sut), 3); //NS,A,PTR
            Assert.Equal(print(RrDbTest.Get(sut, 0)), "Ns aaa.com. TTL=0 ns.aaa.com.");
            Assert.Equal(print(RrDbTest.Get(sut, 1)), "A ns.aaa.com. TTL=0 111.3.255.0");
            Assert.Equal(print(RrDbTest.Get(sut, 2)), "Ptr 0.255.3.111.in-addr.arpa. TTL=0 ns.aaa.com.");
        }

        [Fact]
        public void CNAMEレコードを読み込んだ時_CNAMEが保存される()
        {
            //setUp
            var sut = new RrDb();
            var oneDat = new OneDat(true, new[] { "3", "cname", "alias", "255.254.253.252", "0" }, _isSecret);
            //exercise
            RrDbTest.AddOneDat(sut, DomainName, oneDat);

            //verify
            Assert.Equal(RrDbTest.Size(sut), 1); //Cname
            Assert.Equal(print(RrDbTest.Get(sut, 0)), "Cname alias.aaa.com. TTL=0 cname.aaa.com.");
        }

        [Fact]
        //[ExpectedException(typeof (ValidObjException))]
        public void enable_falseのデータを追加すると例外が発生する()
        {
            //実際に発生するのはValidObjExceptionだが、privateメソッドの制約のためExceptionの発生をテストする

            //setUp
            var sut = new RrDb();
            var oneDat = new OneDat(false, new[] { "0", "www", "alias", "192.168.0.1", "10" }, _isSecret);
            //exercise
            Assert.Throws<ValidObjException>(() =>
               RrDbTest.AddOneDat(sut, DomainName, oneDat)
                );

            //verify
            //Assert.Fail("ここが実行されたらテスト失敗");
        }

        [Fact]
        //[ExpectedException(typeof (ValidObjException))]
        public void 無効なAレコードを読み込むと例外が発生する()
        {
            //実際に発生するのはValidObjExceptionだが、privateメソッドの制約のためExceptionの発生をテストする

            //setUp
            var sut = new RrDb();
            //IPv6のAレコード
            var oneDat = new OneDat(true, new[] { "0", "www", "alias", "::1", "0" }, _isSecret);
            //exercise
            Assert.Throws<ValidObjException>(() =>
                RrDbTest.AddOneDat(sut, DomainName, oneDat)
                );

            //verify
            //Assert.Fail("ここが実行されたらテスト失敗");

        }

        [Fact]
        //[ExpectedException(typeof(ValidObjException))]
        public void 無効なAAAAレコードを読み込むと例外が発生する()
        {
            //実際に発生するのはValidObjExceptionだが、privateメソッドの制約のためExceptionの発生をテストする

            //setUp
            var sut = new RrDb();
            //IPv4のAAAAレコード
            var oneDat = new OneDat(true, new[] { "4", "www", "alias", "127.0.0.1", "0" }, _isSecret);
            //exercise
            Assert.Throws<ValidObjException>(() =>
                    RrDbTest.AddOneDat(sut, DomainName, oneDat)
                    );

            //verify
            //Assert.Fail("ここが実行されたらテスト失敗");

        }

        [Fact]
        //[ExpectedException(typeof(ValidObjException))]
        public void 無効なタイプのレコードを読み込むと例外が発生する()
        {
            //実際に発生するのはValidObjExceptionだが、privateメソッドの制約のためExceptionの発生をテストする

            //setUp
            var sut = new RrDb();
            //タイプは0~4まで
            var oneDat = new OneDat(true, new[] { "5", "www", "alias", "127.0.0.1", "0" }, _isSecret);
            //exercise
            Assert.Throws<ValidObjException>(() =>
                    RrDbTest.AddOneDat(sut, DomainName, oneDat)
                    );

            //verify
            //Assert.Fail("ここが実行されたらテスト失敗");

        }
    }
}