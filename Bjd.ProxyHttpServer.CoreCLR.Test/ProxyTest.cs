using System;
using Bjd.Net;
using Bjd.Net.Sockets;
using Xunit;
using Bjd;
using System.Net.Sockets;
using Bjd.ProxyHttpServer;
using Bjd.Services;
using Bjd.Threading;
using Bjd.Options;

namespace ProxyHttpServerTest
{

    public class ProxyTest : ILife, IDisposable, IClassFixture<ProxyTest.ServerFixture>
    {

        public class ServerFixture : IDisposable
        {
            internal TestService _service;
            internal Proxy _proxy;

            public ServerFixture()
            {
                _service = TestService.CreateTestService();
                _service.SetOption("ProxyTest.ini");


                var kernel = _service.Kernel;
                kernel.ListInitialize();

                var ip = new Ip("127.0.0.1");
                const int port = 0;
                Ssl ssl = null;
                var tcpObj = new SockTcp(kernel, ip, port, 3, ssl);
                var upperProxy = new UpperProxy(false, "", 0, null, false, "", "");//上位プロキシ未使用
                const int timeout = 3;
                _proxy = new Proxy(kernel, null, tcpObj, timeout, upperProxy);

            }

            public void Dispose()
            {
                _service.Dispose();
            }


        }

        private ServerFixture _fixture;

        public ProxyTest(ServerFixture fixture)
        {
            _fixture = fixture;
        }

        public void Dispose()
        {
        }

        [Theory]
        [InlineData("127.0.0.1", 8889)]
        public void Test(string host, int port)
        {

            //int port = 8080;
            //string host = "127.0.0.1";
            var ip = new Ip(host);
            port = _fixture._service.GetAvailablePort(ip, port);


            var listener = new TcpListener(ip.IPAddress, port);
            listener.Start();

            _fixture._proxy.Connect(this, host, port, "TEST", ProxyProtocol.Http);

            Assert.Equal(_fixture._proxy.HostName, ip.ToString());
            Assert.Equal(_fixture._proxy.Port, port);

        }

        public bool IsLife()
        {
            return true;
        }
    }
}
