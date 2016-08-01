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
    public class ServerTestMyDomain : IDisposable, IClassFixture<ServerTestMyDomain.Fixture>
    {
        public class Fixture : ServerTestFixture
        {
        }

        private TestService _service;
        private Server _sv; //サーバ
        private int port;
        private ServerTestMyDomain.Fixture _server;

        public ServerTestMyDomain(ServerTestMyDomain.Fixture fixture)
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
            Assert.Equal(Print(p, RrKind.QD, 0), "Query AAAA www.aaa.com.");
            Assert.Equal(Print(p, RrKind.AN, 0), "AAAA www.aaa.com. TTL=2400 fe80::3882:6dac:af18:cba6");
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
            Assert.Equal(Print(p, RrKind.QD, 0), "Query AAAA xxx.aaa.com.");
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
            Assert.Equal(Print(p, RrKind.AR, 1), "AAAA www.aaa.com. TTL=2400 fe80::3882:6dac:af18:cba6");
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

    }
}