using System;
using System.Threading;
using Bjd;
using Bjd.net;
using Bjd.sock;
using NUnit.Framework;

namespace BjdTest.sock
{
    [TestFixture]
    internal class SockServerTest
    {
        [Test]
        public void test()
        {
            var execute = new Execute();
            execute.startStop("a001 TCPサーバの 起動・停止時のSockState()の確認", ProtocolKind.Tcp);
            execute.startStop("a002 UDPサーバの 起動・停止時のSockState()の確認", ProtocolKind.Udp);
            execute.getLocalAddress("a003 TCPサーバのgetLocalAddress()の確認", ProtocolKind.Tcp);
            execute.getLocalAddress("a004 UDPサーバのgetLocalAddress()の確認", ProtocolKind.Udp);
        }

        private class Execute
        {
            public void startStop(String title, ProtocolKind protocolKind)
            {
                if (protocolKind == ProtocolKind.Tcp)
                {
                    startStopTcp(title);
                }
                else {
                    startStopUdp(title);
                }

            }

            public void startStopTcp(String title)
            {


                var bindIp = new Ip(IpKind.V4Localhost);
                const int port = 8881;
                const int listenMax = 10;
                Ssl ssl = null;

                var sockServer = new SockServerTcp(new Kernel(), ProtocolKind.Tcp, ssl);

                Assert.That(sockServer.SockState, Is.EqualTo(SockState.Idle));

                ThreadStart action = () =>
                {
                    sockServer.Bind(bindIp, port, listenMax);
                };

                var _t = new Thread(action) { IsBackground = true };
                _t.Start();


                while (sockServer.SockState == SockState.Idle)
                {
                    Thread.Sleep(100);
                }
                Assert.That(sockServer.SockState, Is.EqualTo(SockState.Bind));
                sockServer.Close(); //bind()にThreadBaseのポインタを送っていないため、isLifeでブレイクできないので、selectで例外を発生させて終了する
                Assert.That(sockServer.SockState, Is.EqualTo(SockState.Error));

            }

            public void startStopUdp(String title)
            {

                var bindIp = new Ip(IpKind.V4Localhost);
                const int port = 8881;
                Ssl ssl = null;

                var sockServer = new SockServerUdp(new Kernel(), ProtocolKind.Udp, ssl);

                Assert.That(sockServer.SockState, Is.EqualTo(SockState.Idle));

                ThreadStart action = () =>
                {
                    sockServer.Bind(bindIp, port);
                };

                var _t = new Thread(action) { IsBackground = true };
                _t.Start();


                while (sockServer.SockState == SockState.Idle)
                {
                    Thread.Sleep(100);
                }
                Assert.That(sockServer.SockState, Is.EqualTo(SockState.Bind));
                sockServer.Close(); //bind()にThreadBaseのポインタを送っていないため、isLifeでブレイクできないので、selectで例外を発生させて終了する
                Assert.That(sockServer.SockState, Is.EqualTo(SockState.Error));

            }

            public void getLocalAddress(String title, ProtocolKind protocolKind)
            {

                if (protocolKind == ProtocolKind.Tcp)
                {
                    getLocalAddressTcp(title);
                }
                else {
                    getLocalAddressUdp(title);
                }

            }

            public void getLocalAddressTcp(String title)
            {

                var bindIp = new Ip(IpKind.V4Localhost);
                const int port = 9991;
                const int listenMax = 10;
                Ssl ssl = null;

                var sockServer = new SockServerTcp(new Kernel(), ProtocolKind.Tcp, ssl);

                ThreadStart action = () =>
                {
                    sockServer.Bind(bindIp, port, listenMax);
                };


                var _t = new Thread(action) { IsBackground = true };
                _t.Start();

                while (sockServer.SockState == SockState.Idle)
                {
                    Thread.Sleep(200);
                }

                var localAddress = sockServer.LocalAddress;
                Assert.That(localAddress.ToString(), Is.EqualTo("127.0.0.1:9991"));
                //bind()後 localAddressの取得が可能になる

                var remoteAddress = sockServer.RemoteAddress;
                Assert.IsNull(remoteAddress);
                //SockServerでは、remoteＡｄｄｒｅｓｓは常にnullになる

                sockServer.Close();

            }

            public void getLocalAddressUdp(String title)
            {

                var bindIp = new Ip(IpKind.V4Localhost);
                const int port = 9991;
                Ssl ssl = null;

                var sockServer = new SockServerUdp(new Kernel(), ProtocolKind.Udp, ssl);

                ThreadStart action = () =>
                {
                    sockServer.Bind(bindIp, port);
                };


                var _t = new Thread(action) { IsBackground = true };
                _t.Start();

                while (sockServer.SockState == SockState.Idle)
                {
                    Thread.Sleep(200);
                }

                var localAddress = sockServer.LocalAddress;
                Assert.That(localAddress.ToString(), Is.EqualTo("127.0.0.1:9991"));
                //bind()後 localAddressの取得が可能になる

                var remoteAddress = sockServer.RemoteAddress;
                Assert.IsNull(remoteAddress);
                //SockServerでは、remoteＡｄｄｒｅｓｓは常にnullになる

                sockServer.Close();

            }

        }
    }
}
