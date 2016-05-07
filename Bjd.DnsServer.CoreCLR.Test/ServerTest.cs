using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Bjd;
using Bjd.net;
using Bjd.option;
using Bjd.sock;
using Bjd.util;
using Bjd.Common.Test;
using Bjd.DnsServer;
using Xunit;

namespace DnsServerTest
{


    //このテストを成功させるには、c:\dev\bjd5\BJD\outにDnsServer.dllが必要
    public class ServerTest : IDisposable
    {

        private static TmpOption _op; //設定ファイルの上書きと退避
        private static Server _sv; //サーバ

        //[TestFixtureSetUp]
        public ServerTest()
        {
            TestUtil.CopyLangTxt();//BJD.Lang.txt

            //named.caのコピー
            var src = string.Format("{0}\\Bjd.DnsServer.CoreCLR.Test\\named.ca", TestUtil.ProjectDirectory());
            var dst = string.Format("{0}\\Bjd.CoreCLR\\named.ca", TestUtil.ProjectDirectory());
            File.Copy(src, dst, true);

            //設定ファイルの退避と上書き
            _op = new TmpOption("Bjd.DnsServer.CoreCLR.Test", "DnsServerTest.ini");
            OneBind oneBind = new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Udp);
            Kernel kernel = new Kernel();
            var option = kernel.ListOption.Get("Dns");
            Conf conf = new Conf(option);

            //サーバ起動
            _sv = new Server(kernel, conf, oneBind);
            _sv.Start();


        }

        //[TestFixtureTearDown]
        public void Dispose()
        {

            //サーバ停止
            _sv.Stop();
            _sv.Dispose();

            //設定ファイルのリストア
            _op.Dispose();

        }

        // 共通メソッド
        // リクエスト送信して、サーバから返ったデータをDNSパケットとしてデコードする
        // レスポンスが無い場合は、1秒でタイムアウトしてnullを返す
        // rd = 再帰要求
        private PacketDns lookup(DnsType dnsType, string name, bool rd = false)
        {
            //乱数で識別子生成
            var id = (ushort)(new Random()).Next(100);
            //送信パケット生成
            var sp = new PacketDns(id, false, false, rd, false);
            //質問フィールド追加
            sp.AddRr(RrKind.QD, new RrQuery(name, dnsType));
            //クライアントソケット生成、及び送信
            var cl = new SockUdp(new Kernel(), new Ip(IpKind.V4Localhost), 53, null, sp.GetBytes());
            //受信
            //byte[] recvBuf = cl.Recv(1000);
            var recvBuf = cl.Recv(3);

            if (recvBuf.Length == 0)
            {
                //受信データが無い場合
                return null;
            }
            //System.out.println(string.Format("lookup(%s,\"%s\") recv().Length=%d", dnsType, name, recvBuf.Length));
            //デコード
            var p = new PacketDns(recvBuf);
            //System.out.println(print(p));
            return p;
        }

        //共通メソッド
        //リソースレコードの数を表示する
        private static string Print(PacketDns p)
        {
            return string.Format("QD={0} AN={1} NS={2} AR={3}", p.GetCount(RrKind.QD), p.GetCount(RrKind.AN), p.GetCount(RrKind.NS), p.GetCount(RrKind.AR));
        }

        // 共通メソッド
        //リソースレコードのToString()
        private string Print(PacketDns p, RrKind rrKind, int n)
        {
            var o = p.GetRr(rrKind, n);
            if (rrKind == RrKind.QD)
            {
                return o.ToString();
            }
            return Print(o);
        }

        // 共通メソッド
        // リソースレコードのToString()
        private string Print(OneRr o)
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
        public void ステータス情報_ToString_の出力確認()
        {

            var expected = "+ サービス中 \t                 Dns\t[127.0.0.1\t:UDP 53]\tThread";

            //exercise
            var actual = _sv.ToString().Substring(0, 56);
            //verify
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void localhostの検索_タイプA()
        {

            //exercise
            var p = lookup(DnsType.A, "localhost");
            //verify
            Assert.Equal(Print(p), "QD=1 AN=1 NS=1 AR=0");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query A localhost.");
            Assert.Equal(Print(p, RrKind.AN, 0), "A localhost. TTL=2400 127.0.0.1");
            Assert.Equal(Print(p, RrKind.NS, 0), "Ns localhost. TTL=2400 localhost.");
        }

        [Fact]
        public void localhostの検索_タイプAAAA()
        {
            //exercise
            var p = lookup(DnsType.AAAA, "localhost");

            //verify
            Assert.Equal(Print(p), "QD=1 AN=1 NS=1 AR=0");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query Aaaa localhost.");
            Assert.Equal(Print(p, RrKind.AN, 0), "Aaaa localhost. TTL=2400 ::1");
            Assert.Equal(Print(p, RrKind.NS, 0), "Ns localhost. TTL=2400 localhost.");
        }

        [Fact]
        public void localhostの検索_タイプPTR()
        {
            //exercise
            var p = lookup(DnsType.Ptr, "localhost");
            //verify
            Assert.Equal(Print(p), "QD=1 AN=0 NS=0 AR=0");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query Ptr localhost.");
        }

        [Fact]
        public void localhost_V4の検索_タイプPTR()
        {
            //exercise
            var p = lookup(DnsType.Ptr, "1.0.0.127.in-addr.arpa");
            //verify
            Assert.Equal(Print(p), "QD=1 AN=1 NS=0 AR=0");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query Ptr 1.0.0.127.in-addr.arpa.");
            Assert.Equal(Print(p, RrKind.AN, 0), "Ptr 1.0.0.127.in-addr.arpa. TTL=2400 localhost.");

        }

        [Fact]
        public void localhost_V6の検索_タイプPTR()
        {
            //exercise
            var p = lookup(DnsType.Ptr, "1.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.ip6.arpa");
            //verify
            Assert.Equal(Print(p), "QD=1 AN=1 NS=0 AR=0");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query Ptr 1.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.ip6.arpa.");
            Assert.Equal(Print(p, RrKind.AN, 0), "Ptr 1.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.ip6.arpa. TTL=2400 localhost.");
        }

        [Fact]
        public void 自ドメインの検索_タイプA_www_aaa_com()
        {
            //exercise
            var p = lookup(DnsType.A, "www.aaa.com");

            //verify
            Assert.Equal(Print(p), "QD=1 AN=1 NS=1 AR=1");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query A www.aaa.com.");
            Assert.Equal(Print(p, RrKind.AN, 0), "A www.aaa.com. TTL=2400 192.168.0.10");
            Assert.Equal(Print(p, RrKind.NS, 0), "Ns aaa.com. TTL=2400 ns.aaa.com.");
            Assert.Equal(Print(p, RrKind.AR, 0), "A ns.aaa.com. TTL=2400 192.168.0.1");
        }

        [Fact]
        public void 自ドメインの検索_タイプA_xxx_aaa_com_存在しない()
        {
            //exercise
            var p = lookup(DnsType.A, "xxx.aaa.com");

            //verify
            Assert.Equal(Print(p), "QD=1 AN=0 NS=1 AR=1");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query A xxx.aaa.com.");
            Assert.Equal(Print(p, RrKind.NS, 0), "Ns aaa.com. TTL=2400 ns.aaa.com.");
            Assert.Equal(Print(p, RrKind.AR, 0), "A ns.aaa.com. TTL=2400 192.168.0.1");
        }

        [Fact]
        public void 自ドメインの検索_タイプNS_aaa_com()
        {
            //exercise
            var p = lookup(DnsType.Ns, "aaa.com");

            //verify
            Assert.Equal(Print(p), "QD=1 AN=1 NS=0 AR=1");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query Ns aaa.com.");
            Assert.Equal(Print(p, RrKind.AN, 0), "Ns aaa.com. TTL=2400 ns.aaa.com.");
            Assert.Equal(Print(p, RrKind.AR, 0), "A ns.aaa.com. TTL=2400 192.168.0.1");
        }

        [Fact]
        public void 自ドメインの検索_タイプMX_aaa_com()
        {
            //exercise
            var p = lookup(DnsType.Mx, "aaa.com");

            //verify
            Assert.Equal(Print(p), "QD=1 AN=1 NS=0 AR=1");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query Mx aaa.com.");
            Assert.Equal(Print(p, RrKind.AN, 0), "Mx aaa.com. TTL=2400 15 smtp.aaa.com.");
            Assert.Equal(Print(p, RrKind.AR, 0), "A smtp.aaa.com. TTL=2400 192.168.0.2");
        }

        [Fact]
        public void 自ドメインの検索_タイプAAAA_www_aaa_com()
        {
            //exercise
            var p = lookup(DnsType.AAAA, "www.aaa.com");

            //verify
            Assert.Equal(Print(p), "QD=1 AN=1 NS=1 AR=1");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query Aaaa www.aaa.com.");
            Assert.Equal(Print(p, RrKind.AN, 0), "Aaaa www.aaa.com. TTL=2400 fe80::3882:6dac:af18:cba6");
            Assert.Equal(Print(p, RrKind.NS, 0), "Ns aaa.com. TTL=2400 ns.aaa.com.");
            Assert.Equal(Print(p, RrKind.AR, 0), "A ns.aaa.com. TTL=2400 192.168.0.1");
        }

        [Fact]
        public void 自ドメインの検索_タイプAAAA_xxx_aaa_com_存在しない()
        {
            //exercise
            var p = lookup(DnsType.AAAA, "xxx.aaa.com");

            //verify
            Assert.Equal(Print(p), "QD=1 AN=0 NS=1 AR=1");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query Aaaa xxx.aaa.com.");
            Assert.Equal(Print(p, RrKind.NS, 0), "Ns aaa.com. TTL=2400 ns.aaa.com.");
            Assert.Equal(Print(p, RrKind.AR, 0), "A ns.aaa.com. TTL=2400 192.168.0.1");
        }

        [Fact]
        public void 自ドメインの検索_タイプCNAME_www2_aaa_com()
        {
            //exercise
            var p = lookup(DnsType.Cname, "www2.aaa.com");

            //verify
            Assert.Equal(Print(p), "QD=1 AN=1 NS=1 AR=3");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query Cname www2.aaa.com.");
            Assert.Equal(Print(p, RrKind.AN, 0), "Cname www2.aaa.com. TTL=2400 www.aaa.com.");
            Assert.Equal(Print(p, RrKind.NS, 0), "Ns aaa.com. TTL=2400 ns.aaa.com.");
            Assert.Equal(Print(p, RrKind.AR, 0), "A www.aaa.com. TTL=2400 192.168.0.10");
            Assert.Equal(Print(p, RrKind.AR, 1), "Aaaa www.aaa.com. TTL=2400 fe80::3882:6dac:af18:cba6");
            Assert.Equal(Print(p, RrKind.AR, 2), "A ns.aaa.com. TTL=2400 192.168.0.1");
        }

        [Fact]
        public void 自ドメインの検索_タイプCNAME_www_aaa_com_逆検索()
        {
            //exercise
            var p = lookup(DnsType.Cname, "www.aaa.com");

            //verify
            Assert.Equal(Print(p), "QD=1 AN=0 NS=1 AR=1");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query Cname www.aaa.com.");
            Assert.Equal(Print(p, RrKind.NS, 0), "Ns aaa.com. TTL=2400 ns.aaa.com.");
            Assert.Equal(Print(p, RrKind.AR, 0), "A ns.aaa.com. TTL=2400 192.168.0.1");
        }

        [Fact]
        public void 自ドメインの検索_タイプPTR_192_168_0_1()
        {
            //exercise
            var p = lookup(DnsType.Ptr, "1.0.168.192.in-addr.arpa");

            //verify
            Assert.Equal(Print(p), "QD=1 AN=2 NS=0 AR=0");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query Ptr 1.0.168.192.in-addr.arpa.");
            Assert.Equal(Print(p, RrKind.AN, 0), "Ptr 1.0.168.192.in-addr.arpa. TTL=2400 ns.aaa.com.");
            Assert.Equal(Print(p, RrKind.AN, 1), "Ptr 1.0.168.192.in-addr.arpa. TTL=2400 ws0.aaa.com.");
        }

        [Fact]
        public void 自ドメインの検索_タイプPTR_192_168_0_222_存在しない()
        {
            //exercise
            var p = lookup(DnsType.Ptr, "222.0.168.192.in-addr.arpa");

            //verify
            //Assert.Is.assertNull(p); //レスポンスが無いことを確認する
            Assert.Null(p); //レスポンスが無いことを確認する

        }

        [Fact]
        public void 他ドメインの検索_タイプA()
        {
            //exercise
            var p = lookup(DnsType.A, "www.sapporoworks.ne.jp", true);

            //verify
            Assert.Equal(Print(p), "QD=1 AN=2 NS=2 AR=1");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query A www.sapporoworks.ne.jp.");

            var ar = new List<string>();
            for (int i = 0; i < 2; i++)
                ar.Add(Print(p, RrKind.AN, i));
            ar.Sort();
            Assert.Equal(ar[0], "A spw02.sakura.ne.jp. TTL=3600 59.106.27.208");
            Assert.Equal(ar[1], "Cname www.sapporoworks.ne.jp. TTL=3600 spw02.sakura.ne.jp.");


            ar.Clear();
            for (int i = 0; i < 2; i++)
                ar.Add(Print(p, RrKind.NS, i));
            ar.Sort();
            Assert.Equal(ar[0], "Ns sapporoworks.ne.jp. TTL=86400 gdns1.interlink.or.jp.");
            Assert.Equal(ar[1], "Ns sapporoworks.ne.jp. TTL=86400 gdns2.interlink.or.jp.");

            ar.Clear();
            for (int i = 0; i < 1; i++)
                ar.Add(Print(p, RrKind.AR, i));
            ar.Sort();
            if (ar[0] != "A gdns1.interlink.or.jp. TTL=86400 203.141.128.80" &&
                ar[0] != "A gdns2.interlink.or.jp. TTL=86400 203.141.142.56")
            {
                Assert.False(true, "bad AR");
            }
        }

        [Fact]
        public void 他ドメインの検索_タイプMX()
        {
            //exercise
            var p = lookup(DnsType.Mx, "sapporoworks.ne.jp", true);

            //verify
            Assert.Equal(Print(p), "QD=1 AN=1 NS=0 AR=0");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query Mx sapporoworks.ne.jp.");

            Assert.Equal(Print(p, RrKind.AN, 0), "Mx sapporoworks.ne.jp. TTL=3600 10 spw02.sakura.ne.jp.");
            //Assert.That(Print(p, RrKind.AR, 0), "A sapporoworks.ne.jp. TTL=3600 59.106.27.208");


        }

        [Fact]
        public void 他ドメインの検索_タイプCNAME()
        {
            //exercise
            var p = lookup(DnsType.A, "www.yahoo.com", true);

            //verify
            //Assert.Equal(Print(p), Is.EqualTo("QD=1 AN=2 NS=5 AR=5"));
            Assert.Equal(Print(p), "QD=1 AN=2 NS=5 AR=6");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query A www.yahoo.com.");

            Assert.Equal(Print(p, RrKind.AN, 0), "Cname www.yahoo.com. TTL=300 fd-fp3.wg1.b.yahoo.com.");
            Assert.Equal(Print(p, RrKind.AN, 1), "A fd-fp3.wg1.b.yahoo.com. TTL=60 106.10.139.246");
            //Assert.Equal(Print(p, RrKind.AN, 2), "A fd-fp3.wg1.b.yahoo.com. TTL=60 206.190.36.105");
            Assert.Equal(Print(p, RrKind.AR, 0), "A ns1.yahoo.com. TTL=172800 68.180.131.16");
            Assert.Equal(Print(p, RrKind.AR, 1), "A ns5.yahoo.com. TTL=172800 119.160.247.124");
            Assert.Equal(Print(p, RrKind.AR, 2), "A ns2.yahoo.com. TTL=172800 68.142.255.16");
            Assert.Equal(Print(p, RrKind.AR, 3), "A ns3.yahoo.com. TTL=172800 203.84.221.53");
            Assert.Equal(Print(p, RrKind.AR, 4), "Aaaa ns3.yahoo.com. TTL=172800 2406:8600:b8:fe03::1003");


        }

        [Fact]
        public void 他ドメインの検索_yahooo_co_jp()
        {
            //exercise
            var p = lookup(DnsType.A, "www.yahoo.co.jp", true);

            //verify
            Assert.Equal(Print(p), "QD=1 AN=5 NS=4 AR=4");
            Assert.Equal(Print(p, RrKind.AN, 0), "Cname www.yahoo.co.jp. TTL=900 www.g.yahoo.co.jp.");
            //AN.1のAレコードは、ダイナミックにIPが変わるので、Testの対象外とする
            //Assert.Equal(Print(p, RrKind.AN, 1), "A www.g.yahoo.co.jp. TTL=60 203.216.235.189");
        }

        [Fact]
        public void 他ドメインの検索_www_asahi_co_jp()
        {
            //exercise
            var p = lookup(DnsType.A, "www.asahi.co.jp", true);

            //verify
            Assert.Equal(Print(p), "QD=1 AN=1 NS=2 AR=2");
            Assert.Equal(Print(p, RrKind.AN, 0), "A www.asahi.co.jp. TTL=600 202.242.245.10");
        }

        [Fact]
        public void 他ドメインの検索_www_ip_com()
        {
            //exercise
            var p = lookup(DnsType.A, "www.ip.com", true);

            //verify
            Assert.Equal(Print(p), "QD=1 AN=2 NS=5 AR=7");
            var ar = new List<String>();
            //ar.Add("Cname www.ip.com. TTL=3600 ip.com.");
            //ar.Add("A ip.com. TTL=3600 192.155.83.7");
            ar.Add("Cname www.ip.com. TTL=300 ip.com.");
            ar.Add("A ip.com. TTL=300 64.111.96.203");

            //ar.Add("A www.ip.com. TTL=1800 96.45.82.133");
            //ar.Add("A www.ip.com. TTL=1800 96.45.82.69");
            //ar.Add("A www.ip.com. TTL=1800 96.45.82.5");
            //ar.Add("A www.ip.com. TTL=1800 96.45.82.197");

            for (int i = 0; i < ar.Count; i++)
            {
                var str = Print(p, RrKind.AN, i);
                if (ar.IndexOf(str) < 0)
                {
                    Assert.False(true, str);
                }
            }
        }
    }
}