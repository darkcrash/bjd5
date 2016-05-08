using System;
using System.IO;
using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Common.Test;
using Xunit;
using Bjd.WebServer;
using Bjd.Services;

namespace WebServerTest
{

    public class TargetTest : IDisposable, IClassFixture<TargetTest.ServerFixture>
    {
        public class ServerFixture : IDisposable
        {
            internal OneOption option;
            private TmpOption _op; //設定ファイルの上書きと退避
            private Server _v6Sv; //サーバ
            internal Server _v4Sv; //サーバ


            public ServerFixture()
            {
                //設定ファイルの退避と上書き
                _op = new TmpOption("Bjd.WebServer.CoreCLR.Test", "WebServerTest.ini");

                Service.ServiceTest();

                Kernel kernel = new Kernel();
                option = kernel.ListOption.Get("Web-localhost:88");

                //サーバ起動
                _v4Sv = new Server(kernel, new Conf(option), new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
                _v4Sv.Start();

                _v6Sv = new Server(kernel, new Conf(option), new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
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
            var sut = new Target(conf, null);

            //無効なドキュメントルートを設定する
            conf.Set("documentRoot", "q:\\");
            sut = new Target(conf, null);
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
            var sut = new Target(conf, null);
            var fullPath = Path.Combine(_fixture._v4Sv.DocumentRoot, path);

            sut.InitFromUri(uri);
            Assert.Equal(sut.FullPath, fullPath);
        }

        [Theory]
        [InlineData("/index2.html", "index2.html")]//存在しないファイル
        [InlineData("/test4/index.html", "test4\\index.html")]//階層下のファイル
        [InlineData("/", "")]//フォルダの指定（※これ意味あるのか？）
        public void InitFromFileTest(string uri, string path)
        {
            Conf conf = new Conf(_fixture.option);
            var sut = new Target(conf, null);
            var fullPath = Path.Combine(_fixture._v4Sv.DocumentRoot, path);

            sut.InitFromFile(fullPath);
            Assert.Equal(sut.Uri, uri);
        }



    }
}
