using System;
using System.IO;
using Bjd;
using Bjd.Logs;
using Xunit;
using Bjd.SmtpServer;
using Bjd.Test;
using Bjd.Threading;

namespace Bjd.SmtpServer.Test
{
    public class OneFetchJobTest : ILife, IDisposable
    {

        public class ServerFixture : TestServer, IDisposable
        {
            public ServerFixture() : base(TestServerType.Pop, "OneFetchJobTest.ini")
            {
                //usrr2のメールボックスへの２通のメールをセット
                SetMail("user2", "00635026511425888292");
                SetMail("user2", "00635026511765086924");

            }
            public override void Dispose()
            {
                base.Dispose();
            }

        }

        private ServerFixture _testServer;

        // ログイン失敗などで、しばらくサーバが使用できないため、TESTごとサーバを立ち上げて試験する必要がある
        public OneFetchJobTest()
        {
            _testServer = new ServerFixture();


        }


        public void Dispose()
        {
            _testServer.Dispose();
        }



        [Fact]
        public void 接続のみの確認()
        {
            //setUp
            MailSave mailSave = null;
            var domainName = "";
            var interval = 10;//10分
            var synchronize = 0;
            var keepTime = 100;//100分
            var port = _testServer.port;
            var oneFetch = new OneFetch(interval, "127.0.0.1", port, "user1", "user1", "localuser", synchronize, keepTime);
            //var sut = new OneFetchJob(new Kernel(), mailSave, domainName, oneFetch, 3, 1000);
            var sut = new OneFetchJob(_testServer._service.Kernel, mailSave, domainName, oneFetch, 3, 1000);
            var expected = true;
            //exercise
            var actual = sut.Job(new Logger(), DateTime.Now, this);
            //verify
            Assert.Equal(expected, actual);
            //tearDown
            sut.Dispose();
        }

        [Fact]
        public void ホスト名の解決に失敗している時_処理はキャンセルされる()
        {
            //setUp
            MailSave mailSave = null;
            var domainName = "";
            var interval = 10;//10分
            var synchronize = 0;
            var keepTime = 100;//100分
            var port = _testServer.port;
            //不正ホスト名 xxxxx
            var oneFetch = new OneFetch(interval, "xxxxx", port, "user1", "user1", "localuser", synchronize, keepTime);
            var sut = new OneFetchJob(_testServer._service.Kernel, mailSave, domainName, oneFetch, 3, 1000);
            var expected = false;
            //exercise
            var actual = sut.Job(new Logger(), DateTime.Now, this);
            //verify
            Assert.Equal(expected, actual);
            //tearDown
            sut.Dispose();
        }


        [Fact]
        public void インターバルが10分の時_5分後の処理はキャンセルされる()
        {
            //setUp
            MailSave mailSave = null;
            var domainName = "";
            var interval = 10;//10分
            var synchronize = 0;
            var keepTime = 100;//100分
            var port = _testServer.port;
            var oneFetch = new OneFetch(interval, "127.0.0.1", port, "user1", "user1", "localuser", synchronize, keepTime);
            var sut = new OneFetchJob(_testServer._service.Kernel, mailSave, domainName, oneFetch, 3, 1000);
            var expected = false;
            //exercise
            //１回目の接続
            sut.Job(new Logger(), DateTime.Now, this);
            //２回目（5分後）の接続
            var actual = sut.Job(new Logger(), DateTime.Now.AddMinutes(5), this);
            //verify
            Assert.Equal(expected, actual);
            //tearDown
            sut.Dispose();
        }

        [Fact]
        public void 動作確認()
        {
            //setUp
            MailSave mailSave = null;
            var domainName = "";
            var interval = 10;//10分
            var synchronize = 0;
            var keepTime = 100;//100分
            var port = _testServer.port;
            var oneFetch = new OneFetch(interval, "127.0.0.1", port, "user2", "user2", "localuser", synchronize, keepTime);
            var sut = new OneFetchJob(_testServer._service.Kernel, mailSave, domainName, oneFetch, 3, 1000);
            var expected = true;
            //exercise
            var actual = sut.Job(new Logger(), DateTime.Now, this);
            //verify
            Assert.Equal(expected, actual);
            //tearDown
            sut.Dispose();
        }



        public bool IsLife()
        {
            return true;
        }
    }
}
