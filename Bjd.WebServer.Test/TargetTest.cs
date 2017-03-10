using System;
using System.IO;
using Bjd;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Test;
using Xunit;
using Bjd.WebServer;
using Bjd.Services;
using Bjd.WebServer.Handlers;

namespace WebServerTest
{

    public class TargetTest : IDisposable, IClassFixture<TargetTest.ServerFixture>
    {
        public class ServerFixture : IDisposable
        {
            internal ConfigurationBase option;
            internal TestService _service;
            private WebServer _v6Sv; //サーバ
            internal WebServer _v4Sv; //サーバ
            internal int port = 7091;


            public ServerFixture()
            {
                //設定ファイルの退避と上書き
                //_op = new TestOption("Bjd.WebServer.CoreCLR.Test", "WebServerTest.ini");

                _service = TestService.CreateTestService();
                _service.SetOption("TargetTest.ini");
                _service.ContentDirectory("public_html");

                Kernel kernel = _service.Kernel;
                kernel.ListInitialize();

                option = kernel.ListOption.Get("Web-localhost:7091");
                Conf conf = new Conf(option);
                var ipv4 = new Ip(IpKind.V4Localhost);
                var ipv6 = new Ip(IpKind.V6Localhost);
                port = _service.GetAvailablePort(ipv4, conf);

                //サーバ起動
                _v4Sv = new WebServer(kernel, new Conf(option), new OneBind(ipv4, ProtocolKind.Tcp));
                _v4Sv.Start();

                _v6Sv = new WebServer(kernel, new Conf(option), new OneBind(ipv6, ProtocolKind.Tcp));
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

        public TargetTest(ServerFixture fixture)
        {
            _fixture = fixture;

        }

        public void Dispose()
        {


        }


        //無効なドキュメントルート指定する
        [Fact]
        public void DocumentRootTest()
        {
            Conf conf = new Conf(_fixture.option);
            var sut = new HandlerSelector(_fixture._service.Kernel, conf, null);

            //無効なドキュメントルートを設定する
            conf.Set("documentRoot", "q:\\");
            sut = new HandlerSelector(_fixture._service.Kernel, conf, null);
            Assert.Equal(sut.DocumentRoot, null);


        }

        [Theory]
        [InlineData("/index.html", "index.html")]
        [InlineData("/index2.html", "index2.html")]//存在しないファイルを指定
        [InlineData("/", "index.html")]// /で指定した時、welcomeファイルに設定しているファイルが存在する場合、そのファイルになる
        [InlineData("/test1/", "test1\\")] //test1には、ファイルが存在しない
        [InlineData("/test2/", "test2\\index.html")] // test2にはindex.htmlが存在する
        [InlineData("/test3/", "test3\\index.php")] // test3にはindex.phpが存在する
        [InlineData("/test4/", "test4\\index.html")] // test4にはindex.htmlとindex.phpが存在する
        public void FullPathTest(string uri, string path)
        {
            Conf conf = new Conf(_fixture.option);
            var sut = new HandlerSelector(_fixture._service.Kernel, conf, null);
            var dir = _fixture._service.Kernel.Enviroment.ExecutableDirectory;
            var fullPath = Path.Combine(dir, _fixture._v4Sv.DocumentRoot, path);

            var result = sut.InitFromUri(uri);
            Assert.Equal(fullPath, result.FullPath);
        }

        [Theory]
        [InlineData("/index2.html", "index2.html")]//存在しないファイル
        [InlineData("/test4/index.html", "test4\\index.html")]//階層下のファイル
        [InlineData("/", "")]//フォルダの指定（※これ意味あるのか？）
        public void InitFromFileTest(string uri, string path)
        {
            Conf conf = new Conf(_fixture.option);
            var sut = new HandlerSelector(_fixture._service.Kernel, conf, null);
            var dir = _fixture._service.Kernel.Enviroment.ExecutableDirectory;
            var fullPath = Path.Combine(dir, _fixture._v4Sv.DocumentRoot, path);

            var result = sut.InitFromFile(fullPath);
            Assert.Equal(result.Uri, uri);
        }



    }
}
