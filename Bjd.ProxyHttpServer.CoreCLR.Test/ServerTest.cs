using System.Text;
using System.Threading;
using Bjd;
using Bjd.Net;
using Bjd.Option;
using Bjd.Sockets;
using Bjd.Utils;
using Bjd.Common.Test;
using Xunit;
using System.IO;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Bjd.ProxyHttpServer;

namespace ProxyHttpServerTest
{

    public class ServerTest : ILife, IDisposable, IClassFixture<ServerTest.ServerFixture>
    {

        public class ServerFixture : IDisposable
        {
            private TmpOption _op; //設定ファイルの上書きと退避
            internal Server _v6Sv; //サーバ
            internal Server _v4Sv; //サーバ
            internal string srcDir = "";

            public ServerFixture()
            {
                TestUtil.CopyLangTxt();//BJD.Lang.txt

                //srcDir = string.Format("{0}\\ProxyHttpServerTest", TestUtil.ProjectDirectory());
                srcDir = AppContext.BaseDirectory;

                //設定ファイルの退避と上書き
                _op = new TmpOption("Bjd.ProxyHttpServer.CoreCLR.Test", "ProxyHttpServerTest.ini");

                Bjd.Service.Service.ServiceTest();

                Kernel kernel = new Kernel();
                var option = kernel.ListOption.Get("ProxyHttp");
                Conf conf = new Conf(option);

                //サーバ起動
                _v4Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
                _v4Sv.Start();

                _v6Sv = new Server(kernel, conf, new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
                _v6Sv.Start();


            }

            public void Dispose()
            {
                //サーバ停止
                _v4Sv.Stop();
                _v6Sv.Stop();

                _v4Sv.Dispose();
                _v6Sv.Dispose();

                //設定ファイルのリストア
                _op.Dispose();

            }

        }

        private ServerFixture _fixture;

        public ServerTest(ServerFixture fixture)
        {
            _fixture = fixture;

        }

        public void Dispose()
        {

        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V4()
        {

            var sv = _fixture._v4Sv;
            var expected = "+ サービス中 \t           ProxyHttp\t[127.0.0.1\t:TCP 8888]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 58);
            //verify
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V6()
        {

            var sv = _fixture._v6Sv;
            var expected = "+ サービス中 \t           ProxyHttp\t[::1\t:TCP 8888]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 52);
            //verify
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void ConnectTest_V4からV4へのプロキシ()
        {


            //setUp
            //ダミーWebサーバ
            const int webPort = 778;
            //var webRoot = string.Format("{0}\\public_html", _fixture.srcDir);
            var webRoot = Path.Combine(_fixture.srcDir, "public_html");
            var tsWeb = new TsWeb(webPort, webRoot);//Webサーバ起動

            var cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 8888, 10, null);
            cl.Send(Encoding.ASCII.GetBytes("GET http://127.0.0.1:778/index.html HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n"));

            //exercise
            var lines = Inet.RecvLines(cl, 3, this);

            //verify
            Assert.Equal(lines.Count, 9);
            Assert.Equal(lines[0], "HTTP/1.1 200 OK");
            Assert.Equal(lines[1], "Transfer-Encoding: chunked");
            Assert.Equal(lines[2], "Server: Microsoft-HTTPAPI/2.0");

            Assert.Equal(lines[4], "");
            Assert.Equal(lines[5], "3");
            Assert.Equal(lines[6], "123");
            Assert.Equal(lines[7], "0");
            Assert.Equal(lines[8], "");


            //tearDown
            tsWeb.Dispose();//Webサーバ停止

        }

        [Fact]
        public void ConnectTest_V6からV4へのプロキシ()
        {


            //setUp
            //ダミーWebサーバ
            const int webPort = 778;
            //var webRoot = string.Format("{0}\\public_html", _fixture.srcDir);
            var webRoot = Path.Combine(_fixture.srcDir, "public_html");
            var tsWeb = new TsWeb(webPort, webRoot);//Webサーバ起動

            var cl = Inet.Connect(new Kernel(), new Ip(IpKind.V6Localhost), 8888, 10, null);
            cl.Send(Encoding.ASCII.GetBytes("GET http://127.0.0.1:778/index.html HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n"));

            //exercise
            var lines = Inet.RecvLines(cl, 3, this);

            //verify
            Assert.Equal(lines.Count, 9);
            Assert.Equal(lines[0], "HTTP/1.1 200 OK");
            Assert.Equal(lines[1], "Transfer-Encoding: chunked");
            Assert.Equal(lines[2], "Server: Microsoft-HTTPAPI/2.0");

            Assert.Equal(lines[4], "");
            Assert.Equal(lines[5], "3");
            Assert.Equal(lines[6], "123");
            Assert.Equal(lines[7], "0");
            Assert.Equal(lines[8], "");


            //tearDown
            tsWeb.Dispose();//Webサーバ停止

        }


        //外部SSLサーバへの接続試験
        [Theory]
        [InlineData("www.facebook.com")]
        [InlineData("mail.google.com")]
        [InlineData("www.google.co.jp")]
        public void SslTest(string hostname)
        {

            //setUp
            var cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 8888, 10, null);
            cl.Send(Encoding.ASCII.GetBytes(string.Format("CONNECT {0}:443/ HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n", hostname)));

            //exercise
            var lines = Inet.RecvLines(cl, 3, this);

            //verify
            Assert.Equal(lines[0], "HTTP/1.0 200 Connection established");

            //tearDown
            cl.Close();
        }

        //パフォーマンス測定
        [Theory]
        [InlineData(5000)]
        [InlineData(1000)]
        [InlineData(30000)]
        //[TestCase(1000000000)]
        public void PerformanceTest(int count)
        {
            //ダミーWebサーバ
            const int webPort = 17777;
            //string webRoot = string.Format("{0}\\public_html", srcDir);
            string webRoot = Path.Combine(_fixture.srcDir, "public_html");

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

            var tsWeb = new TsWeb(webPort, webRoot);//Webサーバ起動

            //試験用クライアント

            var cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 8888, 10, null);

            //計測
            var sw = new Stopwatch();
            sw.Start();

            cl.Send(Encoding.ASCII.GetBytes(string.Format("GET http://127.0.0.1:17777/{0} HTTP/1.1\r\nHost: 127.0.0.1\r\n\r\n", fileName)));
            var lines = Inet.RecvLines(cl, 5, this);

            //計測終了
            sw.Stop();
            Console.Write("HTTPProxy Performance : {0}ms LINES:{1}\n", sw.ElapsedMilliseconds, count);

            //作業ファイル削除
            File.Delete(path);
            if (lines != null)
            {
                Assert.Equal(lines[0], "HTTP/1.1 200 OK");
            }
            else
            {
                Assert.Equal(null, "receive faild");
            }
            cl.Close();//試験用クライアント破棄
            tsWeb.Dispose();//Webサーバ停止


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
