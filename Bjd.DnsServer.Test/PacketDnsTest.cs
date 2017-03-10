using Bjd.Net;
using Bjd.Test;
using Bjd.DnsServer;
using Xunit;

namespace DnsServerTest
{


    public class PacketDnsTest
    {

        //set type=a で www.google.com をリクエストした時のレスポンス
        private const string Str0 = "0003818000010000000400040377777706676f6f676c6503636f6d00001c0001c01000020001000145c80006036e7334c010c01000020001000145c80006036e7332c010c01000020001000145c80006036e7333c010c01000020001000145c80006036e7331c010c06200010001000146090004d8ef200ac03e000100010001466b0004d8ef220ac05000010001000146090004d8ef240ac02c00010001000146090004d8ef260a";
        //set type=mx で gmail.comをリクエストした時のレスポンス
        private const string Str1 = "00028180000100050004000705676d61696c03636f6d00000f0001c00c000f000100000d630020000a04616c74310d676d61696c2d736d74702d696e016c06676f6f676c65c012c00c000f000100000d630009001404616c7432c02ec00c000f000100000d630009001e04616c7433c02ec00c000f000100000d630009002804616c7434c02ec00c000f000100000d6300040005c02ec00c000200010000d48d0006036e7334c03ec00c000200010000d48d0006036e7332c03ec00c000200010000d48d0006036e7331c03ec00c000200010000d48d0006036e7333c03ec02e000100010000012700044a7d191bc055000100010000003c00044a7d8c1bc07f000100010000009500044a7d831bc0c6000100010000d5380004d8ef200ac0b4000100010000d5320004d8ef220ac0d8000100010000d5710004d8ef240ac0a2000100010000d4f80004d8ef260a";
        //set type=soa で nifty.comをリクエストした時のレスポンス
        private const string Str2 = "000481800001000100020002056e6966747903636f6d0000060001c00c00060001000006160033046f6e7330056e69667479026164026a70000a686f73746d6173746572c02c0bfe412800000e10000003840036ee8000000384c00c00020001000006d20002c027c00c00020001000006d20007046f6e7331c02cc02700010001000007120004caf8254dc07400010001000006da0004caf8149c";

        //set type=a で www.google.com をリクエストした時のレスポンス
        //private string _str3 = "0005818000010000000400040377777706676f6f676c6503636f6d00001c0001c0100002000100000a7d0006036e7333c010c0100002000100000a7d0006036e7331c010c0100002000100000a7d0006036e7334c010c0100002000100000a7d0006036e7332c010c03e0001000100000b5e0004d8ef200ac0620001000100000bde0004d8ef220ac02c0001000100000af50004d8ef240ac0500001000100000ab30004d8ef260a";

        [Fact]
        public void パケット解釈_str0()
        {
            //exercise
            var sut = new PacketDns(TestUtil.HexStream2Bytes(Str0));
            //verify
            Assert.Equal(sut.GetId(), (ushort)0x0003);
            Assert.Equal(sut.GetCount(RrKind.QD), 1);
            Assert.Equal(sut.GetCount(RrKind.AN), 0);
            Assert.Equal(sut.GetCount(RrKind.NS), 4);
            Assert.Equal(sut.GetCount(RrKind.AR), 4);
            Assert.Equal(sut.GetRcode(), (short)0);
            Assert.Equal(sut.GetAa(), false);
            Assert.Equal(sut.GetRd(), true);
            Assert.Equal(sut.GetDnsType(), DnsType.AAAA);
            Assert.Equal(sut.GetRequestName(), "www.google.com.");
            Assert.Equal(sut.GetRr(RrKind.QD, 0).ToString(), (new RrQuery("www.google.com.", DnsType.AAAA)).ToString());
            Assert.Equal(sut.GetRr(RrKind.NS, 0).ToString(), (new RrNs("google.com.", 83400, "ns4.google.com.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.NS, 1).ToString(), (new RrNs("google.com.", 83400, "ns2.google.com.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.NS, 2).ToString(), (new RrNs("google.com.", 83400, "ns3.google.com.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.NS, 3).ToString(), (new RrNs("google.com.", 83400, "ns1.google.com.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.AR, 0).ToString(), (new RrA("ns1.google.com.", 83465, new Ip("216.239.32.10"))).ToString());
            Assert.Equal(sut.GetRr(RrKind.AR, 1).ToString(), (new RrA("ns2.google.com.", 83563, new Ip("216.239.34.10"))).ToString());
            Assert.Equal(sut.GetRr(RrKind.AR, 2).ToString(), (new RrA("ns3.google.com.", 83465, new Ip("216.239.36.10"))).ToString());
            Assert.Equal(sut.GetRr(RrKind.AR, 3).ToString(), (new RrA("ns4.google.com.", 83465, new Ip("216.239.38.10"))).ToString());
        }

        [Fact]
        public void パケット解釈_str1()
        {
            //exercise
            var sut = new PacketDns(TestUtil.HexStream2Bytes(Str1));
            //verify
            Assert.Equal(sut.GetId(), (ushort)0x0002);
            Assert.Equal(sut.GetCount(RrKind.QD), 1);
            Assert.Equal(sut.GetCount(RrKind.AN), 5);
            Assert.Equal(sut.GetCount(RrKind.NS), 4);
            Assert.Equal(sut.GetCount(RrKind.AR), 7);
            Assert.Equal(sut.GetRcode(), (short)0);
            Assert.Equal(sut.GetAa(), false);
            Assert.Equal(sut.GetRd(), true);
            Assert.Equal(sut.GetDnsType(), DnsType.Mx);
            Assert.Equal(sut.GetRequestName(), "gmail.com.");
            Assert.Equal(sut.GetRr(RrKind.QD, 0).ToString(), (new RrQuery("gmail.com.", DnsType.Mx)).ToString());
            Assert.Equal(sut.GetRr(RrKind.AN, 0).ToString(), (new RrMx("gmail.com.", 3427, 10, "alt1.gmail-smtp-in.l.google.com.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.AN, 1).ToString(), (new RrMx("gmail.com.", 3427, 20, "alt2.gmail-smtp-in.l.google.com.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.AN, 2).ToString(), (new RrMx("gmail.com.", 3427, 30, "alt3.gmail-smtp-in.l.google.com.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.AN, 3).ToString(), (new RrMx("gmail.com.", 3427, 40, "alt4.gmail-smtp-in.l.google.com.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.AN, 4).ToString(), (new RrMx("gmail.com.", 3427, 5, "gmail-smtp-in.l.google.com.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.NS, 0).ToString(), (new RrNs("gmail.com.", 54413, "ns4.google.com.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.NS, 1).ToString(), (new RrNs("gmail.com.", 54413, "ns2.google.com.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.NS, 2).ToString(), (new RrNs("gmail.com.", 54413, "ns1.google.com.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.NS, 3).ToString(), (new RrNs("gmail.com.", 54413, "ns3.google.com.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.AR, 0).ToString(), (new RrA("gmail-smtp-in.l.google.com.", 295, new Ip("74.125.25.27"))).ToString());
            Assert.Equal(sut.GetRr(RrKind.AR, 1).ToString(), (new RrA("alt2.gmail-smtp-in.l.google.com.", 60, new Ip("74.125.140.27"))).ToString());
            Assert.Equal(sut.GetRr(RrKind.AR, 2).ToString(), (new RrA("alt4.gmail-smtp-in.l.google.com.", 149, new Ip("74.125.131.27"))).ToString());
            Assert.Equal(sut.GetRr(RrKind.AR, 3).ToString(), (new RrA("ns1.google.com.", 54584, new Ip("216.239.32.10"))).ToString());
            Assert.Equal(sut.GetRr(RrKind.AR, 4).ToString(), (new RrA("ns2.google.com.", 54578, new Ip("216.239.34.10"))).ToString());
            Assert.Equal(sut.GetRr(RrKind.AR, 5).ToString(), (new RrA("ns3.google.com.", 54641, new Ip("216.239.36.10"))).ToString());
            Assert.Equal(sut.GetRr(RrKind.AR, 6).ToString(), (new RrA("ns4.google.com.", 54520, new Ip("216.239.38.10"))).ToString());
        }

        [Fact]
        public void パケット解釈_str2()
        {
            //exercise
            var sut = new PacketDns(TestUtil.HexStream2Bytes(Str2));
            //verify
            Assert.Equal(sut.GetId(), (ushort)0x0004);
            Assert.Equal(sut.GetCount(RrKind.QD), 1);
            Assert.Equal(sut.GetCount(RrKind.AN), 1);
            Assert.Equal(sut.GetCount(RrKind.NS), 2);
            Assert.Equal(sut.GetCount(RrKind.AR), 2);
            Assert.Equal(sut.GetRcode(), (short)0);
            Assert.Equal(sut.GetAa(), false);
            Assert.Equal(sut.GetRd(), true);
            Assert.Equal(sut.GetDnsType(), DnsType.Soa);
            Assert.Equal(sut.GetRequestName(), "nifty.com.");
            Assert.Equal(sut.GetRr(RrKind.QD, 0).ToString(), (new RrQuery("nifty.com.", DnsType.Soa)).ToString());
            Assert.Equal(sut.GetRr(RrKind.AN, 0).ToString(), (new RrSoa("nifty.com.", 0x616, "ons0.nifty.ad.jp", "hostmaster.nifty.ad.jp", 0x0bfe4128, 0xe10, 0x384, 0x36ee80, 0x384)).ToString());
            Assert.Equal(sut.GetRr(RrKind.NS, 0).ToString(), (new RrNs("nifty.com.", 0x6d2, "ons0.nifty.ad.jp.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.NS, 1).ToString(), (new RrNs("nifty.com.", 0x6d2, "ons1.nifty.ad.jp.")).ToString());
            Assert.Equal(sut.GetRr(RrKind.AR, 0).ToString(), (new RrA("ons0.nifty.ad.jp.", 0x712, new Ip("202.248.37.77"))).ToString());
            Assert.Equal(sut.GetRr(RrKind.AR, 1).ToString(), (new RrA("ons1.nifty.ad.jp.", 0x6da, new Ip("202.248.20.156"))).ToString());
        }

        [Fact]
        public void パケット生成_A_NS()
        {
            //setUp

            //exercise
            //パケットの生成
            const ushort id = 0x0005;
            const bool qr = true; //応答
            const bool rd = true; //再帰要求(有効)
            const bool aa = false; //権限(なし)
            const bool ra = true;
            var sut = new PacketDns(id, qr, aa, rd, ra);
            sut.AddRr(RrKind.QD, new RrQuery("www.google.com.", DnsType.AAAA));
            sut.AddRr(RrKind.NS, new RrNs("google.com.", 0xa7d, "ns3.google.com."));
            sut.AddRr(RrKind.NS, new RrNs("google.com.", 0xa7d, "ns1.google.com."));
            sut.AddRr(RrKind.NS, new RrNs("google.com.", 0xa7d, "ns4.google.com."));
            sut.AddRr(RrKind.NS, new RrNs("google.com.", 0xa7d, "ns2.google.com."));
            sut.AddRr(RrKind.AR, new RrA("ns1.google.com.", 0xb5e, new Ip("216.239.32.10")));
            sut.AddRr(RrKind.AR, new RrA("ns2.google.com.", 0xbde, new Ip("216.239.34.10")));
            sut.AddRr(RrKind.AR, new RrA("ns3.google.com.", 0xaf5, new Ip("216.239.36.10")));
            sut.AddRr(RrKind.AR, new RrA("ns4.google.com.", 0xab3, new Ip("216.239.38.10")));
            //生成したパケットのバイト配列で、再度パケットクラスを生成する
            var p = new PacketDns(sut.GetBytes());

            //verify
            Assert.Equal(p.GetAa(), false);
            Assert.Equal(p.GetId(), (ushort)0x0005);
            Assert.Equal(p.GetDnsType(), DnsType.AAAA);
            Assert.Equal(p.GetCount(RrKind.QD), 1);
            Assert.Equal(p.GetCount(RrKind.AN), 0);
            Assert.Equal(p.GetCount(RrKind.NS), 4);
            Assert.Equal(p.GetCount(RrKind.AR), 4);
            Assert.Equal(p.GetRcode(), (short)0);
            Assert.Equal(p.GetRr(RrKind.NS, 0).ToString(), new RrNs("google.com.", 0xa7d, "ns3.google.com.").ToString());
            Assert.Equal(p.GetRr(RrKind.NS, 1).ToString(), new RrNs("google.com.", 0xa7d, "ns1.google.com.").ToString());
            Assert.Equal(p.GetRr(RrKind.NS, 2).ToString(), new RrNs("google.com.", 0xa7d, "ns4.google.com.").ToString());
            Assert.Equal(p.GetRr(RrKind.NS, 3).ToString(), new RrNs("google.com.", 0xa7d, "ns2.google.com.").ToString());

            Assert.Equal(p.GetRr(RrKind.AR, 0).ToString(), new RrA("ns1.google.com.", 0xb5e, new Ip("216.239.32.10")).ToString());
            Assert.Equal(p.GetRr(RrKind.AR, 1).ToString(), new RrA("ns2.google.com.", 0xbde, new Ip("216.239.34.10")).ToString());
            Assert.Equal(p.GetRr(RrKind.AR, 2).ToString(), new RrA("ns3.google.com.", 0xaf5, new Ip("216.239.36.10")).ToString());
            Assert.Equal(p.GetRr(RrKind.AR, 3).ToString(), new RrA("ns4.google.com.", 0xab3, new Ip("216.239.38.10")).ToString());
        }

        [Fact]
        public void パケット生成_MX()
        {
            //setUp

            //exercise
            //パケットの生成
            const ushort id = (ushort)0xf00f;
            const bool qr = true; //応答
            const bool rd = false; //再帰要求(有効)
            const bool aa = true; //権限(あり)
            const bool ra = true;
            var sut = new PacketDns(id, qr, aa, rd, ra);
            sut.AddRr(RrKind.QD, new RrQuery("google.com.", DnsType.Mx));
            sut.AddRr(RrKind.AN, new RrMx("google.com.", 0xa7d, 10, "smtp.google.com."));
            sut.AddRr(RrKind.NS, new RrNs("google.com.", 0xa7d, "ns3.google.com."));
            //生成したパケットのバイト配列で、再度パケットクラスを生成する
            byte[] b = sut.GetBytes();
            var p = new PacketDns(b);

            //verify
            Assert.Equal(p.GetAa(), true);
            Assert.Equal(p.GetId(), (ushort)0xf00f);
            Assert.Equal(p.GetDnsType(), DnsType.Mx);
            Assert.Equal(p.GetCount(RrKind.QD), 1);
            Assert.Equal(p.GetCount(RrKind.AN), 1);
            Assert.Equal(p.GetCount(RrKind.NS), 1);
            Assert.Equal(p.GetCount(RrKind.AR), 0);
            Assert.Equal(p.GetRcode(), (short)0);

            //DEBUG
            //OneRr r = p.GetRr(RrKind.AN, 0;


            Assert.Equal(p.GetRr(RrKind.AN, 0).ToString(), new RrMx("google.com.", 0xa7d, 10, "smtp.google.com.").ToString());
            Assert.Equal(p.GetRr(RrKind.NS, 0).ToString(), new RrNs("google.com.", 0xa7d, "ns3.google.com.").ToString());
            //		Assert.Equal(p.GetRr(RrKind.NS, 1).ToString(), new RrNs("google.com.", 0xa7d, "ns1.google.com.").ToString());
            //		Assert.Equal(p.GetRr(RrKind.NS, 2).ToString(), new RrNs("google.com.", 0xa7d, "ns4.google.com.").ToString());
            //		Assert.Equal(p.GetRr(RrKind.NS, 3).ToString(), new RrNs("google.com.", 0xa7d, "ns2.google.com.").ToString());
            //
            //		Assert.Equal(p.GetRr(RrKind.AR, 0).ToString(), new RrA("ns1.google.com.", 0xb5e, new Ip("216.239.32.10")).ToString());
            //		Assert.Equal(p.GetRr(RrKind.AR, 1).ToString(), new RrA("ns2.google.com.", 0xbde, new Ip("216.239.34.10")).ToString());
            //		Assert.Equal(p.GetRr(RrKind.AR, 2).ToString(), new RrA("ns3.google.com.", 0xaf5, new Ip("216.239.36.10")).ToString());
            //		Assert.Equal(p.GetRr(RrKind.AR, 3).ToString(), new RrA("ns4.google.com.", 0xab3, new Ip("216.239.38.10")).ToString());
        }

    }

}
