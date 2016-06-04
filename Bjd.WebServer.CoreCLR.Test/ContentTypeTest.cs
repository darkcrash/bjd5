using System;
using Bjd.Options;
using Bjd.Test;
using Xunit;
using Bjd.WebServer;
using Bjd;
using Bjd.Services;

namespace WebServerTest
{

    public class ContentTypeTest : IDisposable, IClassFixture<ContentTypeTest.ServerFixture>
    {
        public class ServerFixture : IDisposable
        {
            private TestOption _op; //設定ファイルの上書きと退避
            internal TestService _service;
            internal ContentType _contentType;

            public ServerFixture()
            {
                //設定ファイルの退避と上書き
                _op = new TestOption("Bjd.WebServer.CoreCLR.Test", "WebServerTest.ini");

                _service = TestService.CreateTestService(_op);

                Kernel kernel = _service.Kernel;
                var option = kernel.ListOption.Get("Web-localhost:88");
                Conf conf = new Conf(option);

                _contentType = new ContentType(conf);

            }

            public void Dispose()
            {
                //設定ファイルのリストア
                _op.Dispose();

            }

        }

        private ServerFixture _fixture;

        public ContentTypeTest(ServerFixture fixture)
        {
            _fixture = fixture;
        }

        public void Dispose()
        {
        }

        [Theory]
        [InlineData("$$$", "application/octet-stream")]
        [InlineData("txT", "text/plain")]
        [InlineData("txt", "text/plain")]
        [InlineData("jpg", "image/jpeg")]
        public void ContentTypeGetTest(string ext, string typeText)
        {
            var fileName = string.Format("TEST.{0}", ext);
            var s = _fixture._contentType.Get(fileName);
            Assert.Equal(s, typeText);
        }




    }
}
