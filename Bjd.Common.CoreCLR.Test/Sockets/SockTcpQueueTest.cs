using System;
using System.Text;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Net.Sockets;
using Xunit;
using Bjd.Services;
using Bjd.Threading;
using Xunit.Abstractions;
using Bjd.Test.Logs;

namespace Bjd.Test.Sockets
{

    public class SockTcpQueueTest : ILife, IDisposable
    {
        const int timeout = 20;
        TestService _service;
        TestOutputService _output;
        public SockTcpQueueTest(ITestOutputHelper output)
        {
            _service = TestService.CreateTestService();
            _output = new TestOutputService(output);
        }

        public void Dispose()
        {
            _output.Dispose();
            _service.Dispose();
        }

        [Fact]
        public void EchoServerSendCheckSockQueueLength()
        {
            //setUp
            var ip = new Ip("127.0.0.1");
            //const int port = 9982;
            int port = _service.GetAvailablePort(ip, 9982);
            using (var sv = new SockTcpTestEchoServer(_service.Kernel, ip, port))
            {
                sv.Start();

                var sut = new SockTcp(_service.Kernel, ip, port, timeout, null);
                const int max = 1000;
                for (int i = 0; i < 3; i++)
                {
                    sut.Send(new byte[max]);
                }


                int expected = max * 3;

                for (var i = 0; i < 20; i++)
                {
                    if (sut.Length() == expected) break;
                    Thread.Sleep(125);
                }

                //exercise
                var actual = sut.Length();

                //verify
                Assert.Equal(expected, actual);

                //tearDown
                sut.Close();
                sut.Dispose();
                sv.Stop();
            }
        }

        [Fact]
        public void EchoServerSendTcpQueueRecvPerLength()
        {
            var ip = new Ip("127.0.0.1");
            //const int port = 9981;
            int port = _service.GetAvailablePort(ip, 9981);

            using (var echoServer = new SockTcpTestEchoServer(_service.Kernel, ip, port))
            {
                echoServer.Start();

                var sockTcp = new SockTcp(_service.Kernel, ip, port, timeout, null);

                const int max = 1000;
                const int loop = 3;
                var tmp = new byte[max];
                for (var i = 0; i < max; i++)
                {
                    tmp[i] = (byte)i;
                }

                int recvCount = 0;
                for (var i = 0; i < loop; i++)
                {
                    var len = sockTcp.Send(tmp);
                    Assert.Equal(len, tmp.Length);

                    Thread.Sleep(10);

                    var b = sockTcp.Recv(len, timeout, this);
                    recvCount += b.Length;
                    for (int m = 0; m < max; m += 10)
                    {
                        Assert.Equal(b[m], tmp[m]); //送信したデータと受信したデータが同一かどうかのテスト
                    }
                }
                Assert.Equal(loop * max, recvCount); //送信したデータ数と受信したデータ数が一致するかどうかのテスト

                sockTcp.Close();
                sockTcp.Dispose();
                echoServer.Stop();
            }
        }

        [Fact]
        public void EchoServerToSendOverQueue()
        {
            //setUp
            var ip = new Ip("127.0.0.1");
            //const int port = 9996;
            int port = _service.GetAvailablePort(ip, 9996);
            int size = 2000000;

            using (var sv = new SockTcpTestEchoServer(_service.Kernel, ip, port))
            {
                sv.Start();
                var sut = new SockTcp(_service.Kernel, ip, port, timeout, null);
                var expectedText = new StringBuilder();
                for (var s = 0; s < 10000; s++)
                {
                    expectedText.Append("本日は晴天なり");
                }
                expectedText.Append("\r\n");
                var expected = Encoding.UTF8.GetBytes(expectedText.ToString());
                var dataLength = expected.Length;

                for (var p = 0; p < 2; p++)
                {
                    for (var i = 0; i < size; i += dataLength)
                    {
                        sut.Send(expected);
                    }

                    for (var i = 0; i < size; i += dataLength)
                    {
                        //exercise
                        var actual = sut.LineRecv(timeout, this);
                        //verify
                        Assert.Equal(expected, actual);
                    }
                }

                //tearDown
                sut.Close();
                sut.Dispose();
                sv.Stop();
            }

        }

        public bool IsLife()
        {
            return true;
        }
    }
}
