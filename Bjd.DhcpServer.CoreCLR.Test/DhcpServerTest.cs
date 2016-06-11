using System;
using System.IO;
using System.Net.Sockets;
using Bjd.Net;
using Bjd.Options;
using Bjd.Net.Sockets;
using Xunit;
using Bjd.DhcpServer;
using Bjd;
using Bjd.Test;
using Bjd.Services;
using System.Net;
using Microsoft.Extensions.PlatformAbstractions;

namespace DhcpServerTest
{

    public class DhcpServerTest : IDisposable
    {
        private static TestService _service;
        private static Server _sv; //サーバ

        public DhcpServerTest()
        {
            _service = TestService.CreateTestService();
            _service.SetOption("DhcpServerTest.ini");


            OneBind oneBind = new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Udp);
            Kernel kernel = _service.Kernel;
            var option = kernel.ListOption.Get("Dhcp");
            Conf conf = new Conf(option);

            //サーバ起動
            _sv = new Server(kernel, conf, oneBind);
            _sv.Start();

        }

        public void Dispose()
        {
            //サーバ停止
            try
            {
                _sv.Stop();
                _sv.Dispose();
            }
            finally
            {
                //設定ファイルのリストア
                _service.Dispose();
            }
        }

        PacketDhcp Access(byte[] buf)
        {
            //クライアントソケット生成、及び送信
            var cl = new UdpClient(68);
            //cl.Connect((new Ip(IpKind.V4Localhost)).IPAddress, 67); //クライアントのポートが67でないとサーバが応答しない
            //cl.Send(buf, buf.Length);

            var ip = new IPEndPoint((new Ip(IpKind.V4Localhost)).IPAddress, 67);
            var resultSend = cl.SendAsync(buf, buf.Length, ip);
            resultSend.Wait();

            //受信
            var ep = new IPEndPoint(0, 0);
            //var recvBuf = cl.Receive(ref ep);
            var resultReceive = cl.ReceiveAsync();
            resultReceive.Wait();
            var recvBuf = resultReceive.Result.Buffer;

            if (recvBuf.Length == 0)
            {
                Assert.False(true);//受信データが無い場合
            }
            var rp = new PacketDhcp();
            rp.Read(recvBuf);

            //cl.Close();
            cl.Dispose();
            return rp;
        }

        [Fact]
        public void ステータス情報_ToString_の出力確認()
        {

            var expected = "+ サービス中 \t                Dhcp\t[127.0.0.1\t:UDP 67]\tThread";

            //exercise
            var actual = _sv.ToString().Substring(0, 56);
            //verify
            Assert.Equal(expected, actual);

        }


        [Fact]
        public void ConnectTest()
        {
            const ushort id = 100;
            var requestIp = new Ip("127.0.0.1");
            var serverIp = new Ip("127.0.0.1");
            var mac = new Mac("11-22-33-44-55-66");
            var maskIp = new Ip("255.255.255.0");
            var gwIp = new Ip("255.255.255.0");
            var dnsIp0 = new Ip("255.255.255.0");
            var dnsIp1 = new Ip("255.255.255.0");
            var sp = new PacketDhcp(id, requestIp, serverIp, mac, DhcpType.Discover, 3600, maskIp, gwIp, dnsIp0, dnsIp1, "");

            var bytes = sp.GetBuffer();
            bytes[0] = 1;//Opecode = 2->1

            var rp = Access(bytes);
            Assert.Equal(rp.Type, DhcpType.Offer);
        }

        [Fact]
        public void Connect2Test()
        {
            const ushort id = 100;
            var requestIp = new Ip("0.0.0.0");
            var serverIp = new Ip("127.0.0.1");
            var mac = new Mac("11-22-33-44-55-66");
            var maskIp = new Ip("255.255.255.0");
            var gwIp = new Ip("255.255.255.0");
            var dnsIp0 = new Ip("255.255.255.0");
            var dnsIp1 = new Ip("255.255.255.0");
            var sp = new PacketDhcp(id, requestIp, serverIp, mac, DhcpType.Request, 3600, maskIp, gwIp, dnsIp0, dnsIp1, "");

            var bytes = sp.GetBuffer();
            bytes[0] = 1;//Opecode = 2->1

            var rp = Access(bytes);
            Assert.Equal(rp.Type, DhcpType.Nak);
        }

        [Theory]
        [InlineData("192.168.2.1", "11-22-33-44-55-66", DhcpType.Offer)]
        [InlineData("0.0.0.0", "ff-ff-ff-ff-ff-ff", DhcpType.Offer)]
        public void RequestTest(string requestIpStr, string macStr, DhcpType ans)
        {
            const ushort id = 100;
            var requestIp = new Ip(requestIpStr);
            var serverIp = new Ip("127.0.0.1");
            var mac = new Mac(macStr);
            var maskIp = new Ip("255.255.255.0");
            var gwIp = new Ip("0.0.0.0");
            var dnsIp0 = new Ip("0.0.0.0");
            var dnsIp1 = new Ip("0.0.0.0");
            var sp = new PacketDhcp(id, requestIp, serverIp, mac, DhcpType.Discover, 3600, maskIp, gwIp, dnsIp0, dnsIp1, "");

            var bytes = sp.GetBuffer();
            bytes[0] = 1;//Opecode = 2->1

            var rp = Access(bytes);
            Assert.Equal(rp.Type, ans);
        }
    }
}
