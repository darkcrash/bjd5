using System;
using System.Linq;
using System.Text;
using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using System.Globalization;
using System.IO;
using Bjd.WebServer;
using Bjd.Services;
using Bjd.Threading;
using System.Collections.Generic;

namespace WebServerTest
{

    public class SsiTest : ILife, IDisposable, IClassFixture<SsiTest.ServerFixture>
    {
        bool isLife = true;
        public class ServerFixture : IDisposable
        {
            internal int portv4 = 7089;
            internal int portv6 = 7089;
            internal TestService _service;
            private WebServer _v6Sv; //サーバ
            internal WebServer _v4Sv; //サーバ

            public ServerFixture()
            {
                _service = TestService.CreateTestService();
                _service.SetOption("SsiTest.ini");
                _service.ContentDirectory("public_html");

                var kernel = _service.Kernel;
                var option = kernel.ListOption.Get("Web-localhost:7089");
                Conf conf = new Conf(option);
                var ipv4 = new Ip(IpKind.V4Localhost);
                var ipv6 = new Ip(IpKind.V6Localhost);
                portv4 = _service.GetAvailablePort(ipv4, conf);
                portv6 = portv4;


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

        }

        private ServerFixture _fixture;

        public SsiTest(ServerFixture fixture)
        {
            _fixture = fixture;

        }

        public void Dispose()
        {
            isLife = false;
        }


        string Date2Str(DateTime dt)
        {
            //var culture = new CultureInfo("en-US", true);
            var culture = new CultureInfo("en-US");
            return dt.ToString("ddd M dd hh:mm:ss yyyy", culture);
        }

        [Theory]
        [InlineData("FSize.html", "179")]
        [InlineData("Echo.html", "DOCUMENT_NAME = Echo.html")]
        [InlineData("Echo.html", "LAST_MODIFIED = $")]
        [InlineData("Echo.html", "DATE_LOCAL = $")]
        [InlineData("Echo.html", "DATE_GMT = $")]
        [InlineData("Echo.html", "DOCUMENT_URI = $")]
        [InlineData("TimeFmt.html", "TIME_FMT = $")]
        [InlineData("Flastmod.html", "FLASTMOD = $")]
        //[InlineData("Echo.html", "QUERY_STRING_UNESCAPED = $")] //未実装
        public void SsiRequestTest(string fileName, string pattern)
        {

            //var path = string.Format("{0}\\SsiTest\\Echo.html", _fixture._v4Sv.DocumentRoot);
            var dir = _fixture._service.Kernel.Enviroment.ExecutableDirectory;
            var path = Path.Combine(dir, _fixture._v4Sv.DocumentRoot, "SsiTest", fileName);

            if (pattern == "LAST_MODIFIED = $")
            {
                pattern = string.Format("LAST_MODIFIED = {0}", Date2Str(File.GetLastWriteTime(path)));
            }
            else if (pattern == "DATE_LOCAL = $")
            {
                pattern = string.Format("DATE_LOCAL = {0}", Date2Str(DateTime.Now));
                pattern = pattern.Substring(0, 25); //秒以降は判定しない
            }
            else if (pattern == "DATE_GMT = $")
            {
                //pattern = string.Format("DATE_GMT = {0}", Date2Str(TimeZoneInfo.ConvertTimeToUtc(DateTime.Now)));
                pattern = string.Format("DATE_GMT = {0}", Date2Str(DateTime.Now.ToUniversalTime()));
                pattern = pattern.Substring(0, 25); //秒以降は判定しない
            }
            else if (pattern == "DOCUMENT_URI = $")
            {
                pattern = string.Format("DOCUMENT_URI = {0}", path);
            }
            else if (pattern == "QUERY_STRING_UNESCAPED = $")
            {
                pattern = string.Format("QUERY_STRING_UNESCAPED = {0}", path);
            }
            else if (pattern == "TIME_FMT = $")
            {
                var dt = DateTime.Now;
                pattern = string.Format("TIME_FMT = {0:D2}.{1:D2}.{2:D4}", dt.Day, dt.Month, dt.Year);
            }
            else if (pattern == "FLASTMOD = $")
            {
                pattern = string.Format("FLASTMOD = {0}", Date2Str(File.GetLastWriteTime(path)));
            }

            var cl = Inet.Connect(_fixture._service.Kernel, new Ip(IpKind.V4Localhost), _fixture.portv4, 10, null);

            cl.Send(Encoding.ASCII.GetBytes(string.Format("GET /SsiTest/{0} HTTP/1.1\nHost: ws00\n\n", fileName)));
            int sec = 10; //CGI処理待ち時間（これで大丈夫?）
            //var lines = Inet.RecvLines(cl, sec, this);
            var lines = new List<string>();
            var isMatch = false;
            while (true)
            {
                var result = cl.LineRecv(sec, this);
                if (result == null) break;
                result = Inet.TrimCrlf(result);
                var text = Encoding.ASCII.GetString(result);
                if (text.IndexOf(pattern) != -1)
                {
                    isMatch = true;
                    break;
                }
                lines.Add(text);
            }
            //var find = lines.Any(l => l.IndexOf(pattern) != -1);
            //Assert.Equal(find, true, string.Format("not found {0}", pattern));
            Assert.Equal(isMatch, true);

            cl.Close();

        }

        [Fact]
        public void SsiRequestExecCgi()
        {
            var expected = "100+200=300";

            var cl = Inet.Connect(_fixture._service.Kernel, new Ip(IpKind.V4Localhost), _fixture.portv4, 10, null);

            cl.Send(Encoding.ASCII.GetBytes("GET /SsiTest/ExecCgi.html HTTP/1.1\nHost: ws00\n\n"));
            int sec = 10;

            var lines = new List<string>();
            for (var i = 0; i < 21; i++)
            {
                var result = cl.LineRecv(sec, this);
                if (result == null) break;
                result = Inet.TrimCrlf(result);
                var text = Encoding.ASCII.GetString(result);
                lines.Add(text);
            }
            Assert.Equal(21, lines.Count);
            Assert.Equal(expected, lines[18]);

            cl.Close();

        }

        [Fact]
        public void SsiRequestIncludeHtml()
        {
            var expected = "Hello world.(SSL Include)";

            var cl = Inet.Connect(_fixture._service.Kernel, new Ip(IpKind.V4Localhost), _fixture.portv4, 10, null);

            cl.Send(Encoding.ASCII.GetBytes("GET /SsiTest/Include.html HTTP/1.1\nHost: ws00\n\n"));
            int sec = 10;

            var lines = new List<string>();
            for (var i = 0; i < 19; i++)
            {
                var result = cl.LineRecv(sec, this);
                if (result == null) break;
                result = Inet.TrimCrlf(result);
                var text = Encoding.ASCII.GetString(result);
                lines.Add(text);
            }
            Assert.Equal(19, lines.Count);
            Assert.Equal(expected, lines[16]);

            cl.Close();

        }

        [Fact]
        public void IncludeしたファイルがCGIファイルでない場合()
        {
            //SetUp
            var kernel = _fixture._service.Kernel;
            var cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), _fixture.portv4, 10, null);

            //exercise
            cl.Send(Encoding.ASCII.GetBytes(string.Format("GET /SsiTest/{0} HTTP/1.1\nHost: ws00\n\n", "Include2.html")));
            int sec = 10; //CGI処理待ち時間（これで大丈夫?）
            //var lines = Inet.RecvLines(cl, sec, this);

            var lines = new List<string>();
            for (var i = 0; i < 16; i++)
            {
                var result = cl.LineRecv(sec, this);
                if (result == null) break;
                result = Inet.TrimCrlf(result);
                var text = Encoding.ASCII.GetString(result);
                lines.Add(text);
            }

            var expected = "<html>";
            var actual = lines[8];
            //verify
            Assert.Equal(expected, actual);
            //TearDown
            cl.Close();

        }

        [Fact]
        public void IncludeしたファイルがCGIファイルの場合()
        {
            //SetUp
            var kernel = _fixture._service.Kernel;
            var cl = Inet.Connect(kernel, new Ip(IpKind.V4Localhost), _fixture.portv4, 10, null);

            //exercise
            cl.Send(Encoding.ASCII.GetBytes(string.Format("GET /SsiTest/{0} HTTP/1.1\nHost: ws00\n\n", "Include3.html")));
            int sec = 10; //CGI処理待ち時間（これで大丈夫?）
            //var lines = Inet.RecvLines(cl, sec, this);

            var lines = new List<string>();
            for (var i = 0; i < 10; i++)
            {
                var result = cl.LineRecv(sec, this);
                if (result == null) break;
                result = Inet.TrimCrlf(result);
                var text = Encoding.ASCII.GetString(result);
                lines.Add(text);
            }

            var expected = "100+200=300";
            var actual = lines[8];
            //verify
            Assert.Equal(expected, actual);
            //TearDown
            cl.Close();

        }


        public bool IsLife()
        {
            return isLife;
        }
    }
}
