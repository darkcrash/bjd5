using System;
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

    public class HttpServerTestBadUri : ILife, IDisposable
    {
         int portv4 = 7088;
         int portv6 = 7088;
        internal TestService _service;
        internal WebServer _v6Sv; //サーバ
        internal WebServer _v4Sv; //サーバ

        public HttpServerTestBadUri()
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




        public bool IsLife()
        {
            return true;
        }
    }
}
