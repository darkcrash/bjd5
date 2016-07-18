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

    public class HttpServerTest : ILife, IDisposable, IClassFixture<HttpServerTestFixture>
    {
         int portv4 = 7088;
         int portv6 = 7088;
        internal TestService _service;
        internal WebServer _v6Sv; //サーバ
        internal WebServer _v4Sv; //サーバ
        bool isLife = true;

        public HttpServerTest(HttpServerTestFixture fixture)
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

        //[Theory]
        //[InlineData("9.0")]
        //[InlineData("1")]
        //[InlineData("")]
        //[InlineData("?")]
        //public void サポート外バージョンのリクエストは処理されない(string ver)
        //{
        //    var kernel = _service.Kernel;

        //    //setUp
        //    var _v4Cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), portv4, 10, null);
        //    byte[] expected = null;

        //    //exercise
        //    _v4Cl.Send(Encoding.ASCII.GetBytes(string.Format("GET / HTTP/{0}\n\n", ver)));
        //    var actual = _v4Cl.LineRecv(3, this);

        //    //verify
        //    Assert.Equal(expected, actual);

        //    //tearDoen
        //    _v4Cl.Close();


        //}

        //[Theory]
        //[InlineData("XXX")]
        //[InlineData("")]
        //[InlineData("?")]
        //[InlineData("*")]
        //public void 無効なプロトコルのリクエストは処理されない(string protocol)
        //{
        //    var kernel = _service.Kernel;

        //    //setUp
        //    var _v4Cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), portv4, 10, null);
        //    byte[] expected = null;

        //    //exercise
        //    _v4Cl.Send(Encoding.ASCII.GetBytes(string.Format("GET / {0}/1.0\n\n", protocol)));
        //    var actual = _v4Cl.LineRecv(3, this);
        //    //verify
        //    Assert.Equal(expected, actual);

        //    //tearDoen
        //    _v4Cl.Close();
        //}


        public bool IsLife()
        {
            return isLife;
        }
    }
}
