using System;
using System.Linq;
using System.Text;
using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Utils;
using Bjd.Common.Test;
using Xunit;
using System.Globalization;
using System.IO;
using Bjd.WebServer;
using Bjd.Services;

namespace WebServerTest
{

    public class SsiTest : ILife, IDisposable, IClassFixture<SsiTest.ServerFixture>
    {

        public class ServerFixture : IDisposable
        {
            private TmpOption _op; //設定ファイルの上書きと退避
            private Server _v6Sv; //サーバ
            internal Server _v4Sv; //サーバ

            public ServerFixture()
            {
                //設定ファイルの退避と上書き
                _op = new TmpOption("Bjd.WebServer.CoreCLR.Test", "WebServerTest.ini");

                Service.ServiceTest();

                var kernel = new Kernel();
                var option = kernel.ListOption.Get("Web-localhost:88");
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

        public SsiTest(ServerFixture fixture)
        {
            _fixture = fixture;

        }

        public  void Dispose()
        {

        }


        string Date2Str(DateTime dt)
        {
            //var culture = new CultureInfo("en-US", true);
            var culture = new CultureInfo("en-US");
            return dt.ToString("ddd M dd hh:mm:ss yyyy", culture);
        }

        [Theory]
        [InlineData("ExecCgi.html", "100+200=300")]
        [InlineData("Include.html", "Hello world.(SSL Include)")]
        //[InlineData("FSize.html", "179")]
        [InlineData("FSize.html", "168")]
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

            var path = string.Format("{0}\\SsiTest\\Echo.html", _fixture._v4Sv.DocumentRoot);
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

            var cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 88, 10, null);

            cl.Send(Encoding.ASCII.GetBytes(string.Format("GET /SsiTest/{0} HTTP/1.1\nHost: ws00\n\n", fileName)));
            int sec = 10; //CGI処理待ち時間（これで大丈夫?）
            var lines = Inet.RecvLines(cl, sec, this);
            var find = lines.Any(l => l.IndexOf(pattern) != -1);
            //Assert.Equal(find, true, string.Format("not found {0}", pattern));
            Assert.Equal(find, true);

            cl.Close();

        }

        [Fact]
        public void IncludeしたファイルがCGIファイルでない場合()
        {
            //SetUp

            var cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 88, 10, null);

            //exercise
            cl.Send(Encoding.ASCII.GetBytes(string.Format("GET /SsiTest/{0} HTTP/1.1\nHost: ws00\n\n", "Include2.html")));
            int sec = 30; //CGI処理待ち時間（これで大丈夫?）
            var lines = Inet.RecvLines(cl, sec, this);
            var expected = "<html>";
            var actual = lines[8];
            //verify
            Assert.Equal(actual, expected);
            //TearDown
            cl.Close();

        }

        [Fact]
        public void IncludeしたファイルがCGIファイルの場合()
        {
            //SetUp

            var cl = Inet.Connect(new Kernel(), new Ip(IpKind.V4Localhost), 88, 10, null);

            //exercise
            cl.Send(Encoding.ASCII.GetBytes(string.Format("GET /SsiTest/{0} HTTP/1.1\nHost: ws00\n\n", "Include3.html")));
            int sec = 30; //CGI処理待ち時間（これで大丈夫?）
            var lines = Inet.RecvLines(cl, sec, this);
            var expected = "100+200=300";
            var actual = lines[8];
            //verify
            Assert.Equal(actual, expected);
            //TearDown
            cl.Close();

        }


        public bool IsLife()
        {
            return true;
        }
    }
}
