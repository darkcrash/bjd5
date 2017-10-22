using System.Text;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using System.IO;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Bjd.ProxyHttpServer;
using Bjd.Initialization;
using Bjd.Threading;

namespace ProxyHttpServerTest
{

    public class ServerTest : ILife, IDisposable
    {

        public class ServerFixture : IDisposable
        {
            internal TestService _service;
            internal Server _v6Sv; //サーバ
            internal Server _v4Sv; //サーバ
            internal string srcDir = "";
            internal int port;

            public ServerFixture()
            {
                _service = TestService.CreateTestService();
                _service.SetOption("ServerTest.ini");
                _service.ContentDirectory("public_html");

                Kernel kernel = _service.Kernel;
                kernel.ListInitialize();

                var option = kernel.ListOption.Get("ProxyHttp");
                Conf conf = new Conf(option);

                var ipv4 = new Ip(IpKind.V4Localhost);
                var ipv6 = new Ip(IpKind.V6Localhost);

                port = _service.GetAvailablePort(ipv4, conf);

                srcDir = _service.Kernel.Enviroment.ExecutableDirectory;

                //サーバ起動
                _v4Sv = new Server(kernel, conf, new OneBind(ipv4, ProtocolKind.Tcp));
                _v4Sv.Start();

                _v6Sv = new Server(kernel, conf, new OneBind(ipv6, ProtocolKind.Tcp));
                _v6Sv.Start();


            }

            public void Dispose()
            {
                //サーバ停止
                try
                {
                    _v4Sv.Stop();
                    _v6Sv.Stop();

                    _v4Sv.Dispose();
                    _v6Sv.Dispose();
                }
                finally
                {
                    _service.Dispose();
                }

            }

        }

        private ServerFixture _fixture;

        public ServerTest()
        {
            _fixture = new ServerFixture();

        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V4()
        {

            var sv = _fixture._v4Sv;
            var expected = $"+ サービス中 \t           ProxyHttp\t[127.0.0.1\t:TCP {_fixture.port}]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 58);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V6()
        {

            var sv = _fixture._v6Sv;
            var expected = $"+ サービス中 \t           ProxyHttp\t[::1\t:TCP {_fixture.port}]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 52);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConnectTest_V4からV4へのプロキシ()
        {


            //setUp
            //ダミーWebサーバ
            int webPort = 3778;
            webPort  = _fixture._service.GetAvailablePort(IpKind.V4Localhost, webPort);
            //var webRoot = string.Format("{0}\\public_html", _fixture.srcDir);
            var webRoot = Path.Combine(_fixture.srcDir, "public_html");
            //Webサーバ起動           
            using (var tsWeb = new TsWeb(ref webPort, webRoot))
            {
                var kernel = _fixture._service.Kernel;

                var cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), _fixture.port, 10, null);
                cl.Send(Encoding.ASCII.GetBytes($"GET http://127.0.0.1:{webPort}/index.html HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n"));

                //exercise
                //var lines = Inet.RecvLines(cl, 3, this);
                var lines = new List<string>();
                for (var i = 0; i < 9; i++)
                {
                    var buf = cl.LineRecv(2, this);
                    buf = Inet.TrimCrlf(buf);
                    lines.Add(Encoding.ASCII.GetString(buf));
                }

                //verify
                Assert.Equal(9, lines.Count);
                Assert.Equal("HTTP/1.1 200 OK", lines[0]);
                Assert.Equal("Transfer-Encoding: chunked", lines[1]);
                Assert.Equal("Server: Microsoft-HTTPAPI/2.0", lines[2]);

                Assert.Equal("", lines[4]);
                Assert.Equal("3", lines[5]);
                Assert.Equal("123", lines[6]);
                Assert.Equal("0", lines[7]);
                Assert.Equal("", lines[8]);


                //tearDown
                //tsWeb.Dispose();//Webサーバ停止
            }
        }

        [Fact]
        public void ConnectTest_V6からV4へのプロキシ()
        {
            //setUp
            //ダミーWebサーバ
            int webPort = 3779;
            webPort = _fixture._service.GetAvailablePort(IpKind.V4Localhost, webPort);
            //var webRoot = string.Format("{0}\\public_html", _fixture.srcDir);
            var webRoot = Path.Combine(_fixture.srcDir, "public_html");
            //Webサーバ起動           
            using (var tsWeb = new TsWeb(ref webPort, webRoot))
            {
                var kernel = _fixture._service.Kernel;

                var cl = Inet.Connect(kernel, new Ip(IpKind.V6Localhost), _fixture.port, 10, null);
                cl.Send(Encoding.ASCII.GetBytes($"GET http://127.0.0.1:{webPort}/index.html HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n"));

                //exercise
                //var lines = Inet.RecvLines(cl, 3, this);
                var lines = new List<string>();
                for (var i = 0; i < 9; i++)
                {
                    var buf = cl.LineRecv(2, this);
                    buf = Inet.TrimCrlf(buf);
                    lines.Add(Encoding.ASCII.GetString(buf));
                }

                //verify
                Assert.Equal(9, lines.Count);
                Assert.Equal("HTTP/1.1 200 OK", lines[0]);
                Assert.Equal("Transfer-Encoding: chunked", lines[1]);
                Assert.Equal("Server: Microsoft-HTTPAPI/2.0", lines[2]);

                Assert.Equal("", lines[4]);
                Assert.Equal("3", lines[5]);
                Assert.Equal("123", lines[6]);
                Assert.Equal("0", lines[7]);
                Assert.Equal("", lines[8]);


                //tearDown
                //tsWeb.Dispose();//Webサーバ停止
            }
        }


        //外部SSLサーバへの接続試験
        [Theory]
        [InlineData("www.facebook.com")]
        [InlineData("mail.google.com")]
        [InlineData("www.google.co.jp")]
        public void SslTest(string hostname)
        {
            var kernel = _fixture._service.Kernel;

            //setUp
            //var cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), 8888, 10, null);
            var cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), _fixture.port, 10, null);
            cl.Send(Encoding.ASCII.GetBytes(string.Format("CONNECT {0}:443/ HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n", hostname)));

            //exercise
            //var lines = Inet.RecvLines(cl, 3, this);
            var lines = new List<string>();
            lines.Add(cl.AsciiRecv(2, this));


            //verify
            Assert.Equal("HTTP/1.0 200 Connection established\r\n", lines[0]);

            //tearDown
            cl.Close();
        }

        //パフォーマンス測定
        [Theory]
        [InlineData(500, 17777)]
        [InlineData(100, 17778)]
        [InlineData(1000, 17779)]
        //[InlineData(15000, 17781)]
        //[TestCase(1000000000)]
        public void PerformanceTest(int count, int port)
        {
            //ダミーWebサーバ
            //string webRoot = string.Format("{0}\\public_html", srcDir);
            string webRoot = Path.Combine(_fixture.srcDir, "public_html");

            var ip = new Ip(IpKind.V4Localhost);
            port = _fixture._service.GetAvailablePort(ip, port);

            //試験用ファイルの生成
            var fileName = Path.GetRandomFileName();
            //var path = string.Format("{0}\\{1}", webRoot, fileName);
            var path = Path.Combine(webRoot, fileName);
            var buf = new List<string>();
            for (int i = 0; i < count; i++)
            {
                buf.Add("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ");
            }
            File.WriteAllLines(path, buf);

            //Webサーバ起動           
            using (var tsWeb = new TsWeb(ref port, webRoot))
            {
                var kernel = _fixture._service.Kernel;

                //試験用クライアント

                //var cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), 8888, 10, null);
                var cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), _fixture.port, 10, null);

                //計測
                var sw = new Stopwatch();
                sw.Start();

                //cl.Send(Encoding.ASCII.GetBytes(string.Format("GET http://127.0.0.1:17777/{0} HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n", fileName)));
                cl.Send(Encoding.ASCII.GetBytes($"GET http://127.0.0.1:{port}/{fileName} HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n"));
                //var lines = Inet.RecvLines(cl, 5, this);
                var lines = new List<string>();
                for (var i = 0; i < count; i++)
                {
                    lines.Add(cl.AsciiRecv(2, this));
                }

                //計測終了
                sw.Stop();
                kernel.Logger.TraceInformation($"HTTPProxy Performance : {sw.ElapsedMilliseconds}ms LINES:{count}");

                ////作業ファイル削除
                //File.Delete(path);

                if (lines != null)
                {
                    Assert.Equal("HTTP/1.1 200 OK\r\n", lines[0]);
                }
                else
                {
                    Assert.Equal(null, "receive faild");
                }
                cl.Close();//試験用クライアント破棄
                //tsWeb.Dispose();//Webサーバ停止
            }

        }


        /*
        試験用Webサーバへのリクエスト試験
        [Fact]
        public void Web_Test() {
            byte[] buf = new byte[1024];
            
            //ダミーWebサーバ
            var tsDir = new TsDir();
            int webPort = 777;
            string webRoot = string.Format("{0}\\public_html",tsDir.Src);
            var tsWeb = new TsWeb(webPort, webRoot);//Webサーバ起動


            var tcp = new TcpClient("127.0.0.1", webPort); 
            NetworkStream ns = tcp.GetStream();

            byte[] sendBytes = Encoding.ASCII.GetBytes("GET /index.html HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n");
             //リクエスト送信
            ns.Write(sendBytes, 0, sendBytes.Length);
            //受信
            int size = ns.Read(buf, 0, buf.Length);
            
            tcp.Close();

            List<string> lines = Inet.GetLines(Encoding.ASCII.GetString(buf,0,size));
            Assert.Equal(lines[0],"HTTP/1.1 200 OK");
            
            tsWeb.Dispose();//Webサーバ停止
            
        }
        */
        public bool IsLife()
        {
            return true;
        }
    }
}
