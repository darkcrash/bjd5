using System;
using System.Collections.Generic;
using System.Linq;
using Bjd.Controls;
using Bjd.Logs;
using Bjd.Options;
using Bjd.Test;
using Xunit;
using Bjd.SmtpServer;
using Bjd;
using Bjd.Services;
using System.IO;

namespace Bjd.SmtpServer.Test
{

    public class MlPostTest : IDisposable
    {
        private TestService _service;
        private Ml _ml;
        private TsMailSave _tsMailSave;

        public MlPostTest()
        {
            const string mlName = "1ban";
            var domainList = new List<string> { "example.com" };
            _service = TestService.CreateTestService();
            _service.SetOption("MlPostTest.ini");
            _service.ContentDirectory("TestDir");

            var kernel = _service.Kernel;
            var logger = new Logger();
            //var manageDir = TestUtil.GetTmpDir("TestDir");
            var manageDir = Path.Combine(kernel.Enviroment.ExecutableDirectory, "TestDir");


            _tsMailSave = new TsMailSave();//MailSaveのモックオブジェクト

            var memberList = new Dat(new[] { CtrlType.TextBox, CtrlType.TextBox, CtrlType.CheckBox, CtrlType.CheckBox, CtrlType.CheckBox, CtrlType.TextBox });
            memberList.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "USER1", "user1@example.com", false, true, true, "")); //一般・読者・投稿
            memberList.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "USER2", "user2@example.com", false, true, false, ""));//一般・読者・×
            memberList.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "USER3", "user3@example.com", false, false, true, ""));//一般・×・投稿
            memberList.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "ADMIN", "admin@example.com", true, false, true, "123"));//管理者・×・投稿
            memberList.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "ADMIN2", "admin2@example.com", true, true, true, "456"));//管理者・読者・投稿
            var docs = (from object o in Enum.GetValues(typeof(MlDocKind)) select "").ToList();
            const int maxSummary = 10;
            const int getMax = 10;
            const bool autoRegistration = true;
            const int titleKind = 5;
            var mlOption = new MlOption(maxSummary, getMax, autoRegistration, titleKind, docs, manageDir, memberList);

            _ml = new Ml(kernel, logger, _tsMailSave, mlOption, mlName, domainList);

        }

        public void Dispose()
        {
            _tsMailSave.Dispose();
            _ml.Remove();
        }

        [Theory]
        //user1からの投稿は、user1から読者全員（user1,user2,admin2）に届く
        [InlineData("user1", "1ban", "user1", "user1,user2,admin2")]
        //第３者からの投稿は、管理者から第３者へDenyメール 及び管理者へのエラーメールが届く
        [InlineData("xxx", "1ban", "1ban-admin", "xxx,admin,admin2")]
        //読者からの投稿は、管理者からのDenyメール 及び管理者へのエラーメールが届く
        [InlineData("user2", "1ban", "1ban-admin", "user2,admin,admin2")]
        //投稿を許可されないもの投稿は、管理者からのDenyメール 及び管理者へのエラーメールが届く
        [InlineData("user2", "1ban", "1ban-admin", "user2,admin,admin2")]
        //ML以外への投稿は、本MLオブジェクトには何も残らない
        [InlineData("user1", "2ban", "", "")]
        public void Test(string sender, string to, string from, string recvers)
        {

            //ドメインを追加
            const string domain = "@example.com";
            sender = sender + domain;
            to = to + domain;
            from = from + domain;
            //配信先
            var recvList = new List<string>();
            foreach (var r in recvers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                recvList.Add(r + domain);
            }

            var mail = new TsMail(sender, to, "dmy");
            _ml.Job(mail.MlEnvelope, mail.Mail);

            //user1とuser2に届く
            Assert.Equal(_tsMailSave.Count(), recvList.Count());
            for (int i = 0; i < recvList.Count(); i++)
            {
                Assert.Equal(recvList[i], _tsMailSave.GetTo(i).ToString());
                Assert.Equal(from, _tsMailSave.GetFrom(i).ToString());
            }
        }

        [Theory]
        [InlineData(10)]//10通の送信
        //[InlineData(10000)]//10000通の送信(時間がかかるので、デバッグ時のみ使用する)
        public void Save30Test(int count)
        {

            const string domain = "@example.com";
            const string from = "user1" + domain;

            for (var i = 0; i < count; i++)
            {
                var m = new TsMail(from, "1ban" + domain, "DMY");

                var subject = string.Format("TEST_{0}", i);//試験的に件名を挿入する
                m.Mail.AddHeader("subject", subject);

                _ml.Job(m.MlEnvelope, m.Mail);
            }
            //この時点で、user1,user2,adin2のそれぞれ30通が送信されているため_tsMailSave.Count()は90となる
            Assert.Equal(_tsMailSave.Count(), count * 3);

            for (var i = 0; i < _tsMailSave.Count(); i++)
            {
                var mail = _tsMailSave.GetMail(i);

                Assert.Equal(mail.GetHeader("from"), from);
                Assert.Equal(mail.GetHeader("to"), "1ban" + domain);
                Assert.Equal(mail.GetHeader("subject"), string.Format("[1ban:{0:D5}] TEST_{1}", i / 3 + 1, i / 3));
                Assert.Equal(mail.GetHeader("Reply-To"), "\"1ban\"<1ban@example.com>");
                Assert.Equal(mail.GetHeader("List-Id"), "1ban.example.com");
                Assert.Equal(mail.GetHeader("List-Post"), "<mailto:1ban@example.com>");
                Assert.Equal(mail.GetHeader("List-Owner"), "<mailto:1ban-admin@example.com>");
                Assert.Equal(mail.GetHeader("List-Help"), "<mailto:1ban-ctl@example.com?body=help>");
                Assert.Equal(mail.GetHeader("List-Unsubscribe"), "<mailto:1ban-ctl@example.com?body=unsubscribe>");

            }

        }
    }
}
