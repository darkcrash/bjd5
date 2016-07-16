using System;
using System.Text;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Net.Sockets;
using Xunit;
using Bjd.Services;
using Bjd.Threading;

namespace Bjd.Test.Sockets
{

    public class SockTcpTest : ILife, IDisposable
    {
        TestService _service;
        public SockTcpTest()
        {
            _service = TestService.CreateTestService();
        }

        public void Dispose()
        {
            _service.Dispose();
        }

        //テスト用のEchoサーバ
        private class EchoServer : ThreadBase
        {
            //private readonly SockServer _sockServer;
            private readonly SockServerTcp _sockServer;
            private readonly String _addr;
            private readonly int _port;
            private readonly Ssl _ssl = null;

            public EchoServer(Kernel kernel, String addr, int port) : base(kernel, null)
            {
                //_sockServer = new SockServer(new Kernel(),ProtocolKind.Tcp,_ssl);
                _sockServer = new SockServerTcp(kernel, ProtocolKind.Tcp, _ssl);
                _addr = addr;
                _port = port;
            }

            public override String GetMsg(int no)
            {
                return null;
            }

            protected override bool OnStartThread()
            {
                return true;
            }

            protected override void OnStopThread()
            {
                _sockServer.Close();
            }

            protected override void OnRunThread()
            {
                Ip ip = null;
                try
                {
                    ip = new Ip(_addr);
                }
                catch (ValidObjException ex)
                {
                    Assert.False(true, ex.Message);
                }
                if (_sockServer.Bind(ip, _port, 1))
                {
                    //[C#]
                    ThreadBaseKind = ThreadBaseKind.Running;

                    while (IsLife())
                    {
                        var child = _sockServer.Select(this);
                        if (child == null) break;
                        while (IsLife() && child.SockState == SockState.Connect)
                        {
                            var len = child.Length();
                            if (len > 0)
                            {
                                var buf = child.Recv(len, 100, this);
                                child.Send(buf);
                            }
                        }
                    }
                }
                ThreadBaseKind = ThreadBaseKind.After;
            }
        }
        [Fact]
        public void Echoサーバに送信して溜まったデータサイズ_lengthを確認する()
        {
            //setUp
            const string addr = "127.0.0.1";
            const int port = 9982;
            var sv = new EchoServer(_service.Kernel, addr, port);
            sv.Start();

            var sut = new SockTcp(_service.Kernel, new Ip(addr), port, 100, null);
            const int max = 1000;
            for (int i = 0; i < 3; i++)
            {
                sut.Send(new byte[max]);
            }

            Thread.Sleep(500);

            int expected = max * 3;

            //exercise
            var actual = sut.Length();

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            sut.Close();
            sv.Stop();
        }

        [Fact]
        public void Echoサーバにsendで送信てtcpQueueのlength分ずつRecvする()
        {
            const string addr = "127.0.0.1";
            const int port = 9981;

            var echoServer = new EchoServer(_service.Kernel, addr, port);
            echoServer.Start();

            const int timeout = 100;
            Ip ip = null;
            try
            {
                ip = new Ip(addr);
            }
            catch (ValidObjException ex)
            {
                Assert.False(true, ex.Message);
            }
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
            echoServer.Stop();
        }

        [Fact]
        public void EchoサーバにstringSendで1行送信してstringRecvで1行受信する()
        {
            //setUp
            const string addr = "127.0.0.1";
            const int port = 9993;

            var sv = new EchoServer(_service.Kernel, addr, port);
            sv.Start();
            var sut = new SockTcp(_service.Kernel, new Ip(addr), port, 100, null);
            sut.StringSend("本日は晴天なり", "UTF-8");
            Thread.Sleep(10);

            var expected = "本日は晴天なり\r\n";

            //exercise
            var actual = sut.StringRecv("UTF-8", 5, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            sut.Close();
            sv.Stop();
        }

        [Fact]
        public void EchoサーバにlineSendで1行送信してlineRecvで1行受信する()
        {
            //setUp
            const string addr = "127.0.0.1";
            const int port = 9994;

            var sv = new EchoServer(_service.Kernel, addr, port);
            sv.Start();
            var sut = new SockTcp(_service.Kernel, new Ip(addr), port, 100, null);
            sut.LineSend(Encoding.UTF8.GetBytes("本日は晴天なり"));
            Thread.Sleep(10);

            var expected = "本日は晴天なり\r\n";

            //exercise
            var bytes = sut.LineRecv(5, this);
            var actual = Encoding.UTF8.GetString(bytes);

            //verify
            Assert.Equal(expected, actual);

            //tearDown
            sut.Close();
            sv.Stop();
        }

        [Theory]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public void EchoサーバにlineSend(int count)
        {
            //setUp
            const string addr = "127.0.0.1";
            const int port = 9995;

            var sv = new EchoServer(_service.Kernel, addr, port);
            sv.Start();
            var sut = new SockTcp(_service.Kernel, new Ip(addr), port, 100, null);

            for (var i = 0; i < count; i++)
            {
                sut.LineSend(Encoding.UTF8.GetBytes("本日は晴天なり"));

                var expected = "本日は晴天なり\r\n";

                //exercise
                var bytes = sut.LineRecv(5, this);
                var actual = Encoding.UTF8.GetString(bytes);

                //verify
                Assert.Equal(expected, actual);
            }

            //tearDown
            sut.Close();
            sv.Stop();
        }

        [Fact]
        public void EchoサーバにlineSendOverQueue()
        {
            //setUp
            const string addr = "127.0.0.1";
            const int port = 9996;

            var sv = new EchoServer(_service.Kernel, addr, port);
            sv.Start();
            var sut = new SockTcp(_service.Kernel, new Ip(addr), port, 100, null);
            var expected = "本日は晴天なり\r\n";
            var data = Encoding.UTF8.GetBytes("本日は晴天なり");

            for (var p = 0; p < 100; p++)
            {
                for (var i = 0; i < 1000; i++)
                {
                    sut.LineSend(data);
                }

                for (var i = 0; i < 1000; i++)
                {
                    //exercise
                    var bytes = sut.LineRecv(5, this);
                    var actual = Encoding.UTF8.GetString(bytes);
                    //verify
                    Assert.Equal(expected, actual);
                }
            }

            //tearDown
            sut.Close();
            sv.Stop();
        }


        public bool IsLife()
        {
            return true;
        }
    }
}
