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
    public class ServerTestFixture : IDisposable
    {

        internal TestService _service;
        internal Server _sv; //サーバ
        internal int port;

        public ServerTestFixture()
        {
            _service = TestService.CreateTestService();
            _service.SetOption("DnsServerTest.ini");

            //named.caのコピー
            _service.ContentFile("named.ca");

            Kernel kernel = _service.Kernel;
            kernel.ListInitialize();

            Ip ip = new Ip(IpKind.V4Localhost);
            OneBind oneBind = new OneBind(ip, ProtocolKind.Udp);
            var option = kernel.ListOption.Get("Dns");
            Conf conf = new Conf(option);
            port = _service.GetAvailableUdpPort(ip, conf);

            //サーバ起動
            _sv = new Server(kernel, conf, oneBind);
            _sv.Start();

        }

        public void Dispose()
        {

            //サーバ停止
            _sv.Stop();
            _sv.Dispose();

            _service.Dispose();

        }

    }
}