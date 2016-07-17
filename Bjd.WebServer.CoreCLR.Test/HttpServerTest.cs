﻿using System;
using System.Text;
using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using Bjd.WebServer;
using Bjd.Services;
using Bjd.Threading;

namespace WebServerTest
{

    public class HttpServerTest : ILife, IDisposable
    {
         int portv4 = 7088;
         int portv6 = 7088;
        internal TestService _service;
        internal WebServer _v6Sv; //サーバ
        internal WebServer _v4Sv; //サーバ

        public HttpServerTest()
        {
            _service = TestService.CreateTestService();
            _service.SetOption("WebServerTest.ini");
            _service.ContentDirectory("public_html");

            var kernel = _service.Kernel;
            var option = kernel.ListOption.Get("Web-localhost:7088");
            Conf conf = new Conf(option);
            var ipv4 = new Ip(IpKind.V4Localhost);
            var ipv6 = new Ip(IpKind.V6Localhost);
            portv4 = _service.GetAvailablePort(ipv4, conf);
            portv6 = _service.GetAvailablePort(ipv6, conf);

            //サーバ起動
            _v4Sv = new WebServer(kernel, conf, new OneBind(ipv4, ProtocolKind.Tcp));
            _v4Sv.Start();

            _v6Sv = new WebServer(kernel, conf, new OneBind(ipv6, ProtocolKind.Tcp));
            _v6Sv.Start();

        }

        public void Dispose()
        {

            //サーバ停止
            _v4Sv.Stop();
            _v6Sv.Stop();

            _v4Sv.Dispose();
            _v6Sv.Dispose();

            _service.Dispose();


        }


        [Fact]
        public void ステータス情報_ToString_の出力確認_V4()
        {
            var sv = _v4Sv;
            var expected = $"+ サービス中 \t  Web-localhost:7088\t[127.0.0.1\t:TCP {portv4}]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 58);
            //verify
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void ステータス情報_ToString_の出力確認_V6()
        {

            var sv = _v6Sv;
            var expected = $"+ サービス中 \t  Web-localhost:7088\t[::1\t:TCP {portv6}]\tThread";

            //exercise
            var actual = sv.ToString().Substring(0, 52);
            //verify
            Assert.Equal(expected, actual);

        }


        [Fact]
        public void Http10Test()
        {
            var kernel = _service.Kernel;

            //setUp
            var _v4Cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), portv4, 10, null);
            var expected = "HTTP/1.0 200 Document follows\r\n";

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes("GET / HTTP/1.0\n\n"));
            var buf = _v4Cl.LineRecv(10, this);
            var actual = Encoding.ASCII.GetString(buf);
            //verify
            Assert.Equal(expected, actual);

            //tearDoen
            _v4Cl.Close();


        }

        [Fact]
        public void Http11Test()
        {
            var kernel = _service.Kernel;

            //setUp
            var _v4Cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), portv4, 10, null);
            var expected = "HTTP/1.1 400 Missing Host header or incompatible headers detected.\r\n";

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes("GET / HTTP/1.1\n\n"));
            var buf = _v4Cl.LineRecv(10, this);
            var actual = Encoding.ASCII.GetString(buf);
            //verify
            Assert.Equal(expected, actual);

            //tearDoen
            _v4Cl.Close();

        }

        [Theory]
        [InlineData("9.0")]
        [InlineData("1")]
        [InlineData("")]
        [InlineData("?")]
        public void サポート外バージョンのリクエストは処理されない(string ver)
        {
            var kernel = _service.Kernel;

            //setUp
            var _v4Cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), portv4, 10, null);
            byte[] expected = null;

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes(string.Format("GET / HTTP/{0}\n\n", ver)));
            var actual = _v4Cl.LineRecv(3, this);

            //verify
            Assert.Equal(expected, actual);

            //tearDoen
            _v4Cl.Close();


        }

        [Theory]
        [InlineData("XXX")]
        [InlineData("")]
        [InlineData("?")]
        [InlineData("*")]
        public void 無効なプロトコルのリクエストは処理されない(string protocol)
        {
            var kernel = _service.Kernel;

            //setUp
            var _v4Cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), portv4, 10, null);
            byte[] expected = null;

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes(string.Format("GET / {0}/1.0\n\n", protocol)));
            var actual = _v4Cl.LineRecv(3, this);
            //verify
            Assert.Equal(expected, actual);

            //tearDoen
            _v4Cl.Close();
        }

        [Theory]
        [InlineData("?")]
        [InlineData(",")]
        [InlineData(".")]
        [InlineData("aaa")]
        [InlineData("")]
        [InlineData("_")]
        [InlineData("????")]
        public void 無効なURIは処理されない(string uri)
        {
            var kernel = _service.Kernel;

            //setUp
            var _v4Cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), portv4, 10, null);
            byte[] expected = null;

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes(string.Format("GET {0} HTTP/1.0\n\n", uri)));
            var actual = _v4Cl.LineRecv(3, this);
            //verify
            Assert.Equal(expected, actual);

            //tearDoen
            _v4Cl.Close();
        }

        [Theory]
        [InlineData("SET")]
        [InlineData("POP")]
        [InlineData("")]
        public void 無効なメソッドは処理されない(string method)
        {
            var kernel = _service.Kernel;

            //setUp
            var _v4Cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), portv4, 10, null);
            byte[] expected = null;

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes(string.Format("{0} / HTTP/1.0\n\n", method)));
            var actual = _v4Cl.LineRecv(3, this);
            //verify
            Assert.Equal(expected, actual);

            //tearDoen
            _v4Cl.Close();
        }

        [Theory]
        [InlineData("GET / HTTP/111")]
        [InlineData("GET /")]
        [InlineData("GET")]
        [InlineData("HTTP/1.0")]
        [InlineData("XXX / HTTP/1.0")]
        [InlineData("GET_/_HTTP/1.0")]
        [InlineData("")]
        public void 無効なリクエストは処理されない(string reauest)
        {
            var kernel = _service.Kernel;

            //setUp
            var _v4Cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), portv4, 10, null);
            byte[] expected = null;

            //exercise
            _v4Cl.Send(Encoding.ASCII.GetBytes(reauest));
            var actual = _v4Cl.LineRecv(3, this);
            //verify
            Assert.Equal(expected, actual);

            //tearDoen
            _v4Cl.Close();
        }



        public bool IsLife()
        {
            return true;
        }
    }
}
