using System;
using Bjd;
using Bjd.Logs;
using Bjd.Mailbox;
using Bjd.Configurations;
using Xunit;
using Bjd.Initialization;
using Xunit.Abstractions;
using Bjd.Mailbox.Configurations;

namespace Bjd.Test.Mails
{

    public class MailBoxTest2 : IDisposable
    {

        private static TestService _service;
        private Conf _conf;

        public MailBoxTest2(ITestOutputHelper output)
        {
            _service = TestService.CreateTestService();
            _service.SetOption("MailBoxTest2.ini");
            _service.AddOutput(output);

            var kernel = _service.Kernel;
            kernel.ListInitialize();

            var oneOption = new ConfigurationMailBox(kernel, _service.MailboxPath);
            _conf = new Conf(oneOption);
        }

        public void Dispose()
        {
            _service.Dispose();
        }

        [Theory]
        [InlineData("user1", "user1", true)]
        public void AuthTest(string user, string pass, bool expected)
        {
            //setUp
            //var dir = (String)_conf.Get("dir");
            //var datUser = (Dat)_conf.Get("user");

            //var sut = new MailBox(new Logger(_service.Kernel), datUser, dir);
            var sut = new MailBox(_service.Kernel, _conf);
            //var expected = true;
            //exercise
            var actual = sut.Auth(user, pass);
            //verify
            Assert.Equal(expected, actual);
        }


    }

}
