using System;
using System.IO;
using Bjd;
using Bjd.Net;
using Bjd.Options;
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
            internal OneOption option;
            internal TestService _service;
            private WebServer _v6Sv; //サーバ
            internal WebServer _v4Sv; //サーバ


            public ServerFixture()
            {
                //設定ファイルの退避と上書き
                //_op = new TestOption("Bjd.WebServer.CoreCLR.Test", "WebServerTest.ini");

                _service = TestService.CreateTestService();
                _service.SetOption("TargetTest.ini");
                _service.ContentDirectory("public_html");

                Kernel kernel = _service.Kernel;
                option = kernel.ListOption.Get("Web-localhost:7091");

                //サーバ起動
                _v4Sv = new WebServer(kernel, new Conf(option), new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
                _v4Sv.Start();

                _v6Sv = new WebServer(kernel, new Conf(option), new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
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
            var fullPath = Path.Combine(_fixture._service.Kernel.Enviroment.ExecutableDirectory, _fixture._v4Sv.DocumentRoot, path);

            var result = sut.InitFromUri(uri);
            Assert.Equal(result.FullPath, fullPath);
        }

        [Theory]
        [InlineData("/index2.html", "index2.html")]//存在しないファイル
        [InlineData("/test4/index.html", "test4\\index.html")]//階層下のファイル
        [InlineData("/", "")]//フォルダの指定（※これ意味あるのか？）
        public void InitFromFileTest(string uri, string path)
        {
            Conf conf = new Conf(_fixture.option);
            var sut = new HandlerSelector(_fixture._service.Kernel, conf, null);
            var fullPath = Path.Combine(_fixture._service.Kernel.Enviroment.ExecutableDirectory, _fixture._v4Sv.DocumentRoot, path);

            var result = sut.InitFromFile(fullPath);
            Assert.Equal(result.Uri, uri);
        }



    }
}
