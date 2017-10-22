using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Bjd.SmtpServer;
using Bjd.Initialization;
using Xunit.Abstractions;

namespace Bjd.SmtpServer.Test
{
    public class MlCreatorTest : IDisposable
    {
        MlCreator _mlCreator;
        TestService _service;
        Kernel _kernel;

        public MlCreatorTest(ITestOutputHelper output)
        {
            _service = TestService.CreateTestService();
            _service.AddOutput(output);
            _kernel = _service.Kernel;

            var mlAddr = new MlAddr("1ban", new List<string> { "example.com" });
            var docs = new List<string>();
            foreach (var docKind in Enum.GetValues(typeof(MlDocKind)))
            {
                var buf = docKind.ToString();
                if (buf.Length < 2 || buf[buf.Length - 2] != '\r' || buf[buf.Length - 1] != '\n')
                {
                    buf = buf + "\r\n";
                }
                docs.Add(buf);
            }
            _mlCreator = new MlCreator(_kernel, mlAddr, docs);

        }

        public void Dispose()
        {
            _service.Dispose();
        }
        //[Fact]
        //public  void WelcomeTest(){
        //    var mail = _mlCreator.Welcome();
        //    var body = Encoding.ASCII.GetString(mail.GetBody());
        //    var subject = mail.GetHeader("Subject");
        //    var contentType = mail.GetHeader("Content-Type");

        //    Assert.Equal(body, "Welcome\r\n");
        //    Assert.Equal(subject, "welcome (1ban ML)");
        //    Assert.Equal(contentType, "text/plain; charset=iso-2022-jp");

        //}

        [Theory]
        [InlineData(MlDocKind.Welcome)]
        [InlineData(MlDocKind.Admin)]
        [InlineData(MlDocKind.Help)]
        [InlineData(MlDocKind.Guide)]
        public void CreateMailTest(MlDocKind kind)
        {
            var mail = _mlCreator.Welcome();
            switch (kind)
            {
                case MlDocKind.Admin:
                    mail = _mlCreator.Admin();
                    break;
                case MlDocKind.Help:
                    mail = _mlCreator.Help();
                    break;
                case MlDocKind.Guide:
                    mail = _mlCreator.Guide();
                    break;
            }

            var body = Encoding.ASCII.GetString(mail.GetBody());
            var subject = mail.GetHeader("Subject");
            var contentType = mail.GetHeader("Content-Type");

            Assert.Equal(body, string.Format("{0}\r\n", kind));
            Assert.Equal(subject, string.Format("{0} (1ban ML)", kind.ToString().ToLower()));
            Assert.Equal("text/plain; charset=iso-2022-jp", contentType);
        }



    }
}
