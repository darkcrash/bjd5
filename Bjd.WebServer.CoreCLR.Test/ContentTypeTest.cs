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
            internal TestService _service;
            internal HttpContentType _contentType;

            public ServerFixture()
            {
                _service = TestService.CreateTestService();
                _service.SetOption("ContentTypeTest.ini");

                Kernel kernel = _service.Kernel;
                var option = kernel.ListOption.Get("Web-localhost:92");
                Conf conf = new Conf(option);

                _contentType = new HttpContentType(conf);

            }

            public void Dispose()
            {
                _service.Dispose();
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
