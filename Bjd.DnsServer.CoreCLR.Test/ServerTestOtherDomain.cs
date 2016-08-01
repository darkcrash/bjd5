using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Test;
using Bjd.DnsServer;
using Xunit;
using Bjd.Services;

namespace DnsServerTest
{


    //このテストを成功させるには、c:\dev\bjd5\BJD\outにDnsServer.dllが必要
    public class ServerTestOtherDomain : IDisposable, IClassFixture<ServerTestOtherDomain.Fixture>
    {
        public class Fixture : ServerTestFixture
        {
        }

        private TestService _service;
        private Server _sv; //サーバ
        private int port;
        private ServerTestOtherDomain.Fixture _server;

        public ServerTestOtherDomain(ServerTestOtherDomain.Fixture fixture)
        {
            _server = fixture;
            _service = _server._service;
            _sv = _server._sv;
            port = _server.port;

        }

        public void Dispose()
        {

        }

        // 共通メソッド
        // リクエスト送信して、サーバから返ったデータをDNSパケットとしてデコードする
        // レスポンスが無い場合は、1秒でタイムアウトしてnullを返す
        // rd = 再帰要求
        private PacketDns lookup(DnsType dnsType, string name, bool rd = false)
        {
            var kernel = _service.Kernel;

            //乱数で識別子生成
            var id = (ushort)(new Random()).Next(100);
            //送信パケット生成
            var sp = new PacketDns(id, false, false, rd, false);
            //質問フィールド追加
            sp.AddRr(RrKind.QD, new RrQuery(name, dnsType));
            //クライアントソケット生成、及び送信
            var cl = new SockUdp(kernel, new Ip(IpKind.V4Localhost), port, null, sp.GetBytes());
            //受信
            //byte[] recvBuf = cl.Recv(1000);
            var recvBuf = cl.Recv(6);

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
        public void 他ドメインの検索_タイプA()
        {
            //exercise
            PacketDns p = null;
            for (var i = 0; i < 5; i++)
            {
                p = lookup(DnsType.A, "www.sapporoworks.ne.jp", true);
                if (p == null) continue;
                if (p.GetCount(RrKind.AN) < 2) continue;
                if (p.GetCount(RrKind.NS) < 2) continue;
                if (p.GetCount(RrKind.AR) < 2) continue;
                break;
            }

            //verify
            //Assert.Equal(Print(p), "QD=1 AN=2 NS=2 AR=1");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query A www.sapporoworks.ne.jp.");

            var ar = new List<string>();
            for (int i = 0; i < 2; i++)
                ar.Add(Print(p, RrKind.AN, i));
            ar.Sort();
            Assert.Equal("A spw02.sakura.ne.jp. TTL=3600 59.106.27.208", ar[0]);
            Assert.Equal("Cname www.sapporoworks.ne.jp. TTL=3600 spw02.sakura.ne.jp.", ar[1]);


            ar.Clear();
            for (int i = 0; i < 2; i++)
                ar.Add(Print(p, RrKind.NS, i));
            ar.Sort();
            var NSList = new List<string>();
            NSList.Add("Ns sapporoworks.ne.jp. TTL=3600 gdns1.interlink.or.jp.");
            NSList.Add("Ns sapporoworks.ne.jp. TTL=3600 gdns2.interlink.or.jp.");
            NSList.Add("Ns sapporoworks.ne.jp. TTL=86400 gdns1.interlink.or.jp.");
            NSList.Add("Ns sapporoworks.ne.jp. TTL=86400 gdns2.interlink.or.jp.");
            Assert.Contains<string>(ar[0], NSList);
            Assert.Contains<string>(ar[1], NSList);

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
            PacketDns p = null;
            for (var i = 0; i < 5; i++)
            {
                p = lookup(DnsType.Mx, "sapporoworks.ne.jp", true);
                if (p == null) continue;
                if (p.GetCount(RrKind.QD) < 1) continue;
                if (p.GetCount(RrKind.AN) < 1) continue;
                break;
            }


            //verify
            //Assert.Equal(Print(p), "QD=1 AN=1 NS=0 AR=0");
            Assert.Equal(Print(p, RrKind.QD, 0), "Query Mx sapporoworks.ne.jp.");

            Assert.Equal(Print(p, RrKind.AN, 0), "Mx sapporoworks.ne.jp. TTL=3600 10 spw02.sakura.ne.jp.");
            //Assert.That(Print(p, RrKind.AR, 0), "A sapporoworks.ne.jp. TTL=3600 59.106.27.208");


        }

        [Fact]
        public void 他ドメインの検索_タイプCNAME()
        {
            //exercise
            PacketDns p = null;

            for (var i = 0; i < 5; i ++)
            {
                p = lookup(DnsType.A, "www.yahoo.com", true);
                if (p == null) continue;
                if (p.GetCount(RrKind.AN) > 0) break;
            }

            //verify
            //Assert.Equal(Print(p), Is.EqualTo("QD=1 AN=2 NS=5 AR=5"));
            //Assert.Equal("QD=1 AN=3 NS=5 AR=8", Print(p));

            Assert.Equal("Query A www.yahoo.com.", Print(p, RrKind.QD, 0));

            var anList = new List<string>();
            anList.Add("Cname www.yahoo.com. TTL=300 fd-fp3.wg1.b.yahoo.com.");
            anList.Add("A fd-fp3.wg1.b.yahoo.com. TTL=60 206.190.36.105");
            anList.Add("A fd-fp3.wg1.b.yahoo.com. TTL=60 206.190.36.45");
            anList.Add("A fd-fp3.wg1.b.yahoo.com. TTL=60 106.10.139.246");
            anList.Add("A fd-fp3.wg1.b.yahoo.com. TTL=60 98.138.252.30");
            //Assert.Equal(Print(p, RrKind.AN, 0), "Cname www.yahoo.com. TTL=300 fd-fp3.wg1.b.yahoo.com.");
            //Assert.Equal(Print(p, RrKind.AN, 1), "A fd-fp3.wg1.b.yahoo.com. TTL=60 206.190.36.105");
            //Assert.Equal(Print(p, RrKind.AN, 2), "A fd-fp3.wg1.b.yahoo.com. TTL=60 206.190.36.45");
            ////Assert.Equal(Print(p, RrKind.AN, 2), "A fd-fp3.wg1.b.yahoo.com. TTL=60 206.190.36.105");
            Assert.Contains<string>(Print(p, RrKind.AN, 0), anList);
            Assert.Contains<string>(Print(p, RrKind.AN, 1), anList);
            //Assert.Contains<string>(Print(p, RrKind.AN, 2), anList);

            var arList = new List<string>();
            arList.Add("A ns1.yahoo.com. TTL=172800 68.180.131.16");
            arList.Add("A ns2.yahoo.com. TTL=172800 68.142.255.16");
            arList.Add("A ns3.yahoo.com. TTL=172800 203.84.221.53");
            arList.Add("A ns4.yahoo.com. TTL=172800 98.138.11.157");
            arList.Add("A ns5.yahoo.com. TTL=172800 119.160.247.124");
            arList.Add("AAAA ns1.yahoo.com. TTL=172800 2001:4998:130::1001");
            arList.Add("AAAA ns2.yahoo.com. TTL=172800 2001:4998:140::1002");
            arList.Add("AAAA ns3.yahoo.com. TTL=172800 2406:8600:b8:fe03::1003");

            //Assert.Equal(Print(p, RrKind.AR, 0), "A ns1.yahoo.com. TTL=172800 68.180.131.16");
            //Assert.Equal(Print(p, RrKind.AR, 1), "A ns5.yahoo.com. TTL=172800 119.160.247.124");
            //Assert.Equal(Print(p, RrKind.AR, 2), "A ns2.yahoo.com. TTL=172800 68.142.255.16");
            //Assert.Equal(Print(p, RrKind.AR, 3), "A ns3.yahoo.com. TTL=172800 203.84.221.53");
            //Assert.Equal(Print(p, RrKind.AR, 4), "AAAA ns3.yahoo.com. TTL=172800 2406:8600:b8:fe03::1003");
            Assert.Contains<string>(Print(p, RrKind.AR, 0), arList);
            Assert.Contains<string>(Print(p, RrKind.AR, 1), arList);
            Assert.Contains<string>(Print(p, RrKind.AR, 2), arList);
            Assert.Contains<string>(Print(p, RrKind.AR, 3), arList);
            Assert.Contains<string>(Print(p, RrKind.AR, 4), arList);
            Assert.Contains<string>(Print(p, RrKind.AR, 5), arList);

        }

        [Fact]
        public void 他ドメインの検索_yahooo_co_jp()
        {
            //exercise
            PacketDns p = null;
            for (var i = 0; i < 5; i++)
            {
                p = lookup(DnsType.A, "www.yahoo.co.jp", true);
                if (p == null) continue;
                if (p.GetCount(RrKind.AN) > 0) break;
            }

            //verify
            //Assert.Equal(Print(p), "QD=1 AN=5 NS=4 AR=4");
            Assert.Equal(Print(p, RrKind.AN, 0), "Cname www.yahoo.co.jp. TTL=900 www.g.yahoo.co.jp.");
            //AN.1のAレコードは、ダイナミックにIPが変わるので、Testの対象外とする
            //Assert.Equal(Print(p, RrKind.AN, 1), "A www.g.yahoo.co.jp. TTL=60 203.216.235.189");
        }

        [Fact]
        public void 他ドメインの検索_www_asahi_co_jp()
        {
            //exercise
            PacketDns p = null;
            for (var i = 0; i < 5; i++)
            {
                p = lookup(DnsType.A, "www.asahi.co.jp", true);
                if (p == null) continue;
                if (p.GetCount(RrKind.AN) > 0) break;
            }

            //verify
            //Assert.Equal("QD=1 AN=2 NS=2 AR=2", Print(p));
            //Assert.Equal("Cname www.asahi.co.jp. TTL=600 202.242.245.10", Print(p, RrKind.AN, 0));
            //Assert.Equal("Cname www.asahi.co.jp. TTL=600 www-asahi.durasite.net.", Print(p, RrKind.AN, 0));
            Assert.Equal("Cname www.asahi.co.jp. TTL=28800 www-asahi.durasite.net.", Print(p, RrKind.AN, 0));
        }

        [Fact]
        public void 他ドメインの検索_www_ip_com()
        {
            //exercise
            PacketDns p = null;
            for (var i = 0; i < 5; i++)
            {
                p = lookup(DnsType.A, "www.ip.com", true);
                if (p.GetCount(RrKind.AN) > 0) break;
            }

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