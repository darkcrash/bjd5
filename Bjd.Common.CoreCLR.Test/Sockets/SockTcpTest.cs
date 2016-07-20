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

    public class SockTcpTest : ILife, IDisposable
    {
        const int timeout = 20;
        TestService _service;
        TestOutputService _output;
        public SockTcpTest(ITestOutputHelper output)
        {
            _service = TestService.CreateTestService();
            _output = new  TestOutputService(output);
        }

        public void Dispose()
        {
            _output.Dispose();
            _service.Dispose();
        }

        [Fact]
        public void EchoServerStringSendTextLineStringRecvTextLine()
        {
            //setUp
            var ip = new Ip("127.0.0.1");
            //const int port = 9993;
            int port = _service.GetAvailablePort(ip, 9993);

            using (var sv = new SockTcpTestEchoServer(_service.Kernel, ip, port))
            {
                sv.Start();
                var sut = new SockTcp(_service.Kernel, ip, port, timeout, null);
                sut.StringSend("本日は晴天なり", "UTF-8");
                Thread.Sleep(10);

                var expected = "本日は晴天なり\r\n";

                //exercise
                var actual = sut.StringRecv("UTF-8", timeout, this);

                //verify
                Assert.Equal(expected, actual);

                //tearDown
                sut.Close();
                sut.Dispose();
                sv.Stop();
            }

        }

        [Fact]
        public void EchoServerLineSendTextLineLineRecvTextLine()
        {
            //setUp
            var ip = new Ip("127.0.0.1");
            //const int port = 9994;
            int port = _service.GetAvailablePort(ip, 9994);

            using (var sv = new SockTcpTestEchoServer(_service.Kernel, ip, port))
            {

                sv.Start();
                var sut = new SockTcp(_service.Kernel, ip, port, timeout, null);
                sut.LineSend(Encoding.UTF8.GetBytes("本日は晴天なり"));
                Thread.Sleep(10);

                var expected = "本日は晴天なり\r\n";

                //exercise
                var bytes = sut.LineRecv(timeout, this);
                var actual = Encoding.UTF8.GetString(bytes);

                //verify
                Assert.Equal(expected, actual);

                //tearDown
                sut.Close();
                sut.Dispose();
                sv.Stop();
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void EchoServerLineSend(int count)
        {
            //setUp
            var ip = new Ip("127.0.0.1");
            //const int port = 9995;
            int port = _service.GetAvailablePort(ip, 9995);

            using (var sv = new SockTcpTestEchoServer(_service.Kernel, ip, port))
            {
                sv.Start();
                var sut = new SockTcp(_service.Kernel, ip, port, timeout, null);

                var data = Encoding.UTF8.GetBytes("本日は晴天なり");
                var expected = Encoding.UTF8.GetBytes("本日は晴天なり\r\n");

                for (var i = 0; i < count; i++)
                {
                    sut.LineSend(data);

                    //exercise
                    var actual = sut.LineRecv(timeout, this);

                    //verify
                    Assert.Equal(expected, actual);
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
