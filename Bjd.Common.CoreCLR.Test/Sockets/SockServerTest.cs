using System;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Net.Sockets;
using Xunit;
using Bjd.Services;

namespace Bjd.Test.Sockets
{
    public class SockServerTest
    {

        private class Life : Threading.ILife
        {
            public bool IsLife()
            {
                return true;
            }
        }

        [Fact]
        public void TcpServerStartStop()
        {
            using (var execute = new Execute())
            {
                execute.startStopTcp("a001 TCPサーバの 起動・停止時のSockState()の確認");
            }
        }

        [Fact]
        public void UdpServerStartStop()
        {
            using (var execute = new Execute())
            {
                execute.startStopUdp("a002 UDPサーバの 起動・停止時のSockState()の確認");
            }
        }

        [Fact]
        public void TcpGetLocalAddress()
        {
            using (var execute = new Execute())
            {
                execute.getLocalAddressTcp("a003 TCPサーバのgetLocalAddress()の確認");
            }
        }

        [Fact]
        public void UdpGetLocalAddress()
        {
            using (var execute = new Execute())
            {
                execute.getLocalAddressUdp("a004 UDPサーバのgetLocalAddress()の確認");
            }
        }


        private class Execute : IDisposable
        {
            TestService _service;

            public Execute()
            {
                _service = TestService.CreateTestService();
                _service.SetOption("Option.ini");
            }

            public void Dispose()
            {
                _service.Dispose();
            }

            public void startStopTcp(String title)
            {


                var bindIp = new Ip(IpKind.V4Localhost);
                var life = new Life();
                int port = _service.GetAvailablePort(bindIp, 8881);
                const int listenMax = 10;
                Ssl ssl = null;

                var sockServer = new SockServerTcp(_service.Kernel, ProtocolKind.Tcp, ssl);

                Assert.Equal(sockServer.SockState, SockState.Idle);

                ThreadStart action = () =>
                {
                    sockServer.Bind(bindIp, port, listenMax);
                    try { while (true) sockServer.Select(life); }
                    catch { }
                };

                var _t = new Thread(action) { IsBackground = true };
                _t.Start();


                while (sockServer.SockState == SockState.Idle)
                {
                    Thread.Sleep(100);
                }
                Assert.Equal(sockServer.SockState, SockState.Bind);
                sockServer.Close(); //bind()にThreadBaseのポインタを送っていないため、isLifeでブレイクできないので、selectで例外を発生させて終了する
                Assert.Equal(sockServer.SockState, SockState.Error);

            }

            public void startStopUdp(String title)
            {

                var bindIp = new Ip(IpKind.V4Localhost);
                var life = new Life();
                //const int port = 8881;
                int port = _service.GetAvailablePort(bindIp, 8881);
                Ssl ssl = null;

                var sockServer = new SockServerUdp(_service.Kernel, ProtocolKind.Udp, ssl);

                Assert.Equal(sockServer.SockState, SockState.Idle);

                ThreadStart action = () =>
                {
                    sockServer.Bind(bindIp, port);
                    try { while (true) sockServer.Select(life); }
                    catch { }
                };

                var _t = new Thread(action) { IsBackground = true };
                _t.Start();


                while (sockServer.SockState == SockState.Idle)
                {
                    Thread.Sleep(100);
                }
                Assert.Equal(sockServer.SockState, SockState.Bind);
                sockServer.Close(); //bind()にThreadBaseのポインタを送っていないため、isLifeでブレイクできないので、selectで例外を発生させて終了する
                Assert.Equal(sockServer.SockState, SockState.Error);

            }

            public void getLocalAddressTcp(String title)
            {

                var bindIp = new Ip(IpKind.V4Localhost);
                var life = new Life();
                //const int port = 9991;
                int port = _service.GetAvailablePort(bindIp, 9991);
                const int listenMax = 10;
                Ssl ssl = null;

                var sockServer = new SockServerTcp(_service.Kernel, ProtocolKind.Tcp, ssl);

                ThreadStart action = () =>
                {
                    sockServer.Bind(bindIp, port, listenMax);
                    try { while (true) sockServer.Select(life); }
                    catch { }
                };


                var _t = new Thread(action) { IsBackground = true };
                _t.Start();

                while (sockServer.SockState == SockState.Idle)
                {
                    Thread.Sleep(200);
                }

                var localAddress = sockServer.LocalAddress;
                Assert.Equal(localAddress.ToString(), "127.0.0.1:9991");
                //bind()後 localAddressの取得が可能になる

                var remoteAddress = sockServer.RemoteAddress;
                Assert.Null(remoteAddress);
                //SockServerでは、remoteＡｄｄｒｅｓｓは常にnullになる

                sockServer.Close();

            }

            public void getLocalAddressUdp(String title)
            {

                var bindIp = new Ip(IpKind.V4Localhost);
                var life = new Life();
                //const int port = 9991;
                int port = _service.GetAvailablePort(bindIp, 9991);
                Ssl ssl = null;

                var sockServer = new SockServerUdp(_service.Kernel, ProtocolKind.Udp, ssl);

                ThreadStart action = () =>
                {
                    sockServer.Bind(bindIp, port);
                    try { while (true) sockServer.Select(life); }
                    catch { }
                };


                var _t = new Thread(action) { IsBackground = true };
                _t.Start();

                while (sockServer.SockState == SockState.Idle)
                {
                    Thread.Sleep(200);
                }

                var localAddress = sockServer.LocalAddress;
                Assert.Equal(localAddress.ToString(), "127.0.0.1:9991");
                //bind()後 localAddressの取得が可能になる

                var remoteAddress = sockServer.RemoteAddress;
                Assert.Null(remoteAddress);
                //SockServerでは、remoteＡｄｄｒｅｓｓは常にnullになる

                sockServer.Close();

            }

        }
    }
}
