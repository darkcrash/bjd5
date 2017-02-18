using System;
using System.IO;
using Bjd;
using Bjd.Logs;
using Xunit;
using Xunit.Abstractions;
using Bjd.SmtpServer;
using Bjd.Test;
using Bjd.Threading;
using Bjd.Mails;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;
using Bjd.Services;
using Bjd.Test.Logs;
using System.Threading.Tasks;

namespace Bjd.SmtpServer.Test
{
    public class OneFetchJobTest : ILife, IDisposable
    {

        internal class ServerFixture : TestServer, IDisposable
        {

            public readonly Logger Logger;
            public Fetch fet = null;


            public ServerFixture() : base(TestServerType.Pop, "OneFetchJobTest.ini")
            {
                //var bind = new OneBind(new Ip(IpKind.InAddrAny), ProtocolKind.Tcp);
                //var sv = new SmtpServer.Server(_service.Kernel, _service.Kernel.CreateConf("Smtp"), bind);
                fet = new Fetch(_service.Kernel, null, "", null, 0, 0);
                Logger = _service.Kernel.CreateLogger("Smtp", true, fet);

                //usrr2のメールボックスへの２通のメールをセット
                SetMail("user2", "00635026511425888292");
                SetMail("user2", "00635026511765086924");


                var sv = (Pop3Server.Server)_v4Sv;


            }
        }




        private ServerFixture _testServer;
        private int port;
        private bool isDisposed = false;
        private TestOutputService _output;

        // ログイン失敗などで、しばらくサーバが使用できないため、TESTごとサーバを立ち上げて試験する必要がある
        public OneFetchJobTest(ITestOutputHelper helper)
        {
            _testServer = new ServerFixture();
            _output = new TestOutputService(helper);
            port = _testServer.port;

        }

        public void Dispose()
        {
            _output.Dispose();
            _testServer.Dispose();
            _testServer = null;
            isDisposed = true;
        }


        [Fact]
        public void ConnectionOnly()
        {
            //setUp
            MailSave mailSave = null;
            var domainName = "";
            var interval = 10;//10分
            var synchronize = 0;
            var keepTime = 100;//100分
            var logger = _testServer.Logger;

            var oneFetch = new OneFetch(interval, "127.0.0.1", port, "user1", "user1", "localuser", synchronize, keepTime);
            //var sut = new OneFetchJob(new Kernel(), mailSave, domainName, oneFetch, 3, 1000);
            using (var sut = new OneFetchJob(_testServer._service.Kernel, mailSave, domainName, oneFetch, 3, 1000))
            {
                var expected = true;
                //exercise
                var actual = sut.Job(logger, DateTime.Now, this);
                //verify
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void CancelWhenHostNotFound()
        {
            //setUp
            MailSave mailSave = null;
            var domainName = "";
            var interval = 10;//10分
            var synchronize = 0;
            var keepTime = 100;//100分
            var logger = _testServer.Logger;

            //不正ホスト名 xxxxx
            var oneFetch = new OneFetch(interval, "xxxxx", port, "user1", "user1", "localuser", synchronize, keepTime);
            using (var sut = new OneFetchJob(_testServer._service.Kernel, mailSave, domainName, oneFetch, 3, 1000))
            {
                var expected = false;
                //exercise
                var actual = sut.Job(logger, DateTime.Now, this);
                //verify
                Assert.Equal(expected, actual);
            }
        }


        [Fact]
        public void CancelAfter5MinWhenInterval10Min()
        {
            //setUp
            MailSave mailSave = null;
            var domainName = "";
            var interval = 10;//10分
            var synchronize = 0;
            var keepTime = 100;//100分
            var logger = _testServer.Logger;

            var oneFetch = new OneFetch(interval, "127.0.0.1", port, "user1", "user1", "localuser", synchronize, keepTime);
            using (var sut = new OneFetchJob(_testServer._service.Kernel, mailSave, domainName, oneFetch, 3, 1000))
            {
                var expected = false;
                //exercise
                //１回目の接続
                sut.Job(new Logger(), DateTime.Now, this);
                //２回目（5分後）の接続
                var actual = sut.Job(logger, DateTime.Now.AddMinutes(5), this);
                //verify
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Execution()
        {
            //setUp
            MailSave mailSave = null;
            var domainName = "";
            var interval = 10;//10分
            var synchronize = 0;
            var keepTime = 100;//100分
            var logger = _testServer.Logger;

            var oneFetch = new OneFetch(interval, "127.0.0.1", port, "user2", "user2", "localuser", synchronize, keepTime);
            using (var sut = new OneFetchJob(_testServer._service.Kernel, mailSave, domainName, oneFetch, 3, 1000))
            {
                var expected = true;
                //exercise
                var actual = sut.Job(logger, DateTime.Now, this);
                //verify
                Assert.True(actual, sut.GetLastError());
                Assert.Equal(expected, actual);
            }
        }


        public bool IsLife()
        {
            return !isDisposed;
        }
    }
}
