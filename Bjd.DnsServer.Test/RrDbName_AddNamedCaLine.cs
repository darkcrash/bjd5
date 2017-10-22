using System.IO;
using Bjd.Utils;
using Bjd.DnsServer;
using Xunit;

namespace DnsServerTest
{


    public class RrDbTest_addNamedCaLine
    {

        // 共通メソッド
        // リソースレコードのtostring()
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
        public void コメント行は処理されない()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            var expected = 0;
            RrDbTest.AddNamedCaLine(sut, "", "; formerly NS.INTERNIC.NET");
            var actual = RrDbTest.Size(sut);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 空白行は処理されない()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            var expected = 0;
            RrDbTest.AddNamedCaLine(sut, "", "");
            var actual = RrDbTest.Size(sut);
            //verify
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void Aレコードの処理()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            var retName = RrDbTest.AddNamedCaLine(sut, "", "A.ROOT-SERVERS.NET.      3600000      A     198.41.0.4");
            //verify
            Assert.Equal("A.ROOT-SERVERS.NET.", retName);
            Assert.Equal(1, RrDbTest.Size(sut)); //A
            Assert.Equal("A A.ROOT-SERVERS.NET. TTL=0 198.41.0.4", print(RrDbTest.Get(sut, 0))); //TTLは強制的に0になる
        }

        [Fact]
        public void AAAAレコードの処理()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            var retName = RrDbTest.AddNamedCaLine(sut, "", "A.ROOT-SERVERS.NET.      3600000      AAAA  2001:503:BA3E::2:30");
            //verify
            Assert.Equal("A.ROOT-SERVERS.NET.", retName);
            Assert.Equal(1, RrDbTest.Size(sut)); //Aaaa
            Assert.Equal("AAAA A.ROOT-SERVERS.NET. TTL=0 2001:503:ba3e::2:30", print(RrDbTest.Get(sut, 0))); //TTLは強制的に0になる
        }

        [Fact]
        public void NSレコードの処理()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            string retName = RrDbTest.AddNamedCaLine(sut, "", ".                        3600000  IN  NS    A.ROOT-SERVERS.NET.");
            //verify
            Assert.Equal(".", retName);
            Assert.Equal(1, RrDbTest.Size(sut)); //Ns
            Assert.Equal("Ns . TTL=0 A.ROOT-SERVERS.NET.", print(RrDbTest.Get(sut, 0))); //TTLは強制的に0になる
        }

        [Fact]
        //[ExpectedException(typeof (IOException))]
        public void DnsTypeが無い場合例外が発生する()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            Assert.Throws<IOException>(() =>
                  RrDbTest.AddNamedCaLine(sut, "", ".                        3600000  IN      A.ROOT-SERVERS.NET.")
                );
        }

        [Fact]
        //[ExpectedException(typeof(IOException))]
        public void DnsTypeの次のカラムのDataが無い場合例外が発生する()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            Assert.Throws<IOException>(() =>
                RrDbTest.AddNamedCaLine(sut, "", ".                        3600000  IN  NS")
            );
        }

        [Fact]
        //[ExpectedException(typeof(IOException))]
        public void Aタイプでアドレスに矛盾があると例外が発生する()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            Assert.Throws<IOException>(() =>
                RrDbTest.AddNamedCaLine(sut, "", "A.ROOT-SERVERS.NET.      3600000      A     ::1")
                );
        }


        [Fact]
        //[ExpectedException(typeof(IOException))]
        public void AAAAタイプでアドレスに矛盾があると例外が発生する()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            Assert.Throws<IOException>(() =>
                RrDbTest.AddNamedCaLine(sut, "", "A.ROOT-SERVERS.NET.      3600000      AAAA     192.168.0.1")
                );
        }

        [Fact]
        //[ExpectedException(typeof(IOException))]
        public void A_AAAA_NS以外タイプは例外が発生する()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            Assert.Throws<IOException>(() =>
                RrDbTest.AddNamedCaLine(sut, "", ".                        3600000  IN  MX    A.ROOT-SERVERS.NET.")
                );
        }

        [Fact]
        //[ExpectedException(typeof(IOException))]
        public void Aタイプで不正なアドレスを指定すると例外が発生する()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            Assert.Throws<IOException>(() =>
                RrDbTest.AddNamedCaLine(sut, "", "A.ROOT-SERVERS.NET.      3600000      A     1.1.1.1.1")
                );
        }

        [Fact]
        //[ExpectedException(typeof(IOException))]
        public void AAAAタイプで不正なアドレスを指定すると例外が発生する()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            Assert.Throws<IOException>(() =>
                RrDbTest.AddNamedCaLine(sut, "", "A.ROOT-SERVERS.NET.      3600000      AAAA     xxx")
                );
        }

        [Fact]
        public void 名前補完_アットマークの場合ドメイン名になる()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            var expected = "example.com.";
            var actual = RrDbTest.AddNamedCaLine(sut, "", "@      3600000      A     198.41.0.4");
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 名前補完_最後にドットが無い場合_ドメイン名が補完される()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            var expected = "www.example.com.";
            var actual = RrDbTest.AddNamedCaLine(sut, "", "www      3600000      A     198.41.0.4");
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 名前補完_指定されない場合_前行と同じになる()
        {
            //setUp
            var sut = new RrDb();
            //exercise
            var expected = "before.aaa.com.";
            var actual = RrDbTest.AddNamedCaLine(sut, "before.aaa.com.", "     3600000      A     198.41.0.4");
            //verify
            Assert.Equal(expected, actual);
        }
    }
}