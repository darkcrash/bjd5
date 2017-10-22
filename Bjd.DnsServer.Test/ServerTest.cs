using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Test;
using Bjd.DnsServer;
using Xunit;
using Bjd.Initialization;

namespace DnsServerTest
{


    //このテストを成功させるには、c:\dev\bjd5\BJD\outにDnsServer.dllが必要
    public class ServerTest : IDisposable, IClassFixture<ServerTest.Fixture>
    {
        public class Fixture : ServerTestFixture
        {
        }

        private TestService _service;
        private Server _sv; //サーバ
        private int port;
        private ServerTest.Fixture _server;

        public ServerTest(ServerTest.Fixture fixture)
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
        public void ステータス情報_ToString_の出力確認()
        {
            System.Threading.Tasks.Task.Delay(1000).Wait();
            var expected = $"+ サービス中 \t                 Dns\t[127.0.0.1\t:UDP {port}]\tThread";

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
            Assert.Equal("QD=1 AN=1 NS=1 AR=0", Print(p));
            Assert.Equal("Query A localhost.", Print(p, RrKind.QD, 0));
            Assert.Equal("A localhost. TTL=2400 127.0.0.1", Print(p, RrKind.AN, 0));
            Assert.Equal("Ns localhost. TTL=2400 localhost.", Print(p, RrKind.NS, 0));
        }

        [Fact]
        public void localhostの検索_タイプAAAA()
        {
            //exercise
            var p = lookup(DnsType.AAAA, "localhost");

            //verify
            Assert.Equal("QD=1 AN=1 NS=1 AR=0", Print(p));
            Assert.Equal("Query AAAA localhost.", Print(p, RrKind.QD, 0));
            Assert.Equal("AAAA localhost. TTL=2400 ::1", Print(p, RrKind.AN, 0));
            Assert.Equal("Ns localhost. TTL=2400 localhost.", Print(p, RrKind.NS, 0));
        }

        [Fact]
        public void localhostの検索_タイプPTR()
        {
            //exercise
            var p = lookup(DnsType.Ptr, "localhost");
            //verify
            Assert.Equal("QD=1 AN=0 NS=0 AR=0", Print(p));
            Assert.Equal("Query Ptr localhost.", Print(p, RrKind.QD, 0));
        }

        [Fact]
        public void localhost_V4の検索_タイプPTR()
        {
            //exercise
            var p = lookup(DnsType.Ptr, "1.0.0.127.in-addr.arpa");
            //verify
            Assert.Equal("QD=1 AN=1 NS=0 AR=0", Print(p));
            Assert.Equal("Query Ptr 1.0.0.127.in-addr.arpa.", Print(p, RrKind.QD, 0));
            Assert.Equal("Ptr 1.0.0.127.in-addr.arpa. TTL=2400 localhost.", Print(p, RrKind.AN, 0));

        }

        [Fact]
        public void localhost_V6の検索_タイプPTR()
        {
            //exercise
            var p = lookup(DnsType.Ptr, "1.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.ip6.arpa");
            //verify
            Assert.Equal("QD=1 AN=1 NS=0 AR=0", Print(p));
            Assert.Equal("Query Ptr 1.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.ip6.arpa.", Print(p, RrKind.QD, 0));
            Assert.Equal("Ptr 1.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.ip6.arpa. TTL=2400 localhost.", Print(p, RrKind.AN, 0));
        }


    }
}