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
        public void test()
        {
            using (var execute = new Execute())
            {
                execute.startStop("a001 TCPサーバの 起動・停止時のSockState()の確認", ProtocolKind.Tcp);
                execute.startStop("a002 UDPサーバの 起動・停止時のSockState()の確認", ProtocolKind.Udp);
                execute.getLocalAddress("a003 TCPサーバのgetLocalAddress()の確認", ProtocolKind.Tcp);
                execute.getLocalAddress("a004 UDPサーバのgetLocalAddress()の確認", ProtocolKind.Udp);
            }
        }

        [Fact]
        public void test1()
        {
            using (var execute = new Execute())
            {
                execute.startStop("a001 TCPサーバの 起動・停止時のSockState()の確認", ProtocolKind.Tcp);
            }
        }

        [Fact]
        public void test2()
        {
            using (var execute = new Execute())
            {
                execute.startStop("a002 UDPサーバの 起動・停止時のSockState()の確認", ProtocolKind.Udp);
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

            public void startStop(String title, ProtocolKind protocolKind)
            {
                if (protocolKind == ProtocolKind.Tcp)
                {
                    startStopTcp(title);
                }
                else
                {
                    startStopUdp(title);
                }

            }

            public void startStopTcp(String title)
            {


                var bindIp = new Ip(IpKind.V4Localhost);
                var life = new Life();
                const int port = 8881;
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
                const int port = 8881;
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

            public void getLocalAddress(String title, ProtocolKind protocolKind)
            {

                if (protocolKind == ProtocolKind.Tcp)
                {
                    getLocalAddressTcp(title);
                }
                else
                {
                    getLocalAddressUdp(title);
                }

            }

            public void getLocalAddressTcp(String title)
            {

                var bindIp = new Ip(IpKind.V4Localhost);
                var life = new Life();
                const int port = 9991;
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
                const int port = 9991;
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
