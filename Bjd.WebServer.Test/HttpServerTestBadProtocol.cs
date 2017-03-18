using System;
using System.Text;
using Bjd;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using Bjd.WebServer;
using Bjd.Initialization;
using Bjd.Threading;

namespace WebServerTest
{

    public class HttpServerTestBadProtocol : ILife, IDisposable, IClassFixture<HttpServerTestFixture>
    {
         int portv4 = 7088;
         int portv6 = 7088;
        internal TestService _service;
        internal WebServer _v6Sv; //サーバ
        internal WebServer _v4Sv; //サーバ
        bool isLife = true;

        public HttpServerTestBadProtocol(HttpServerTestFixture fixture)
        {
            portv4 = fixture.portv4;
            portv6 = fixture.portv6;
            _service = fixture._service;
            _v4Sv = fixture._v4Sv;
            _v6Sv = fixture._v6Sv;
        }

        public void Dispose()
        {
            isLife = false;
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


        public bool IsLife()
        {
            return isLife;
        }
    }
}
