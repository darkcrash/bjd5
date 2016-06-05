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
    public class MlGuideTest : IDisposable
    {
        private TestService _service;
        private Ml _ml;
        private TsMailSave _tsMailSave;

        public  MlGuideTest()
        {
            const string mlName = "1ban";
            var domainList = new List<string> { "example.com" };
            //var tsDir = new TsDir();
            _service = TestService.CreateTestService();
            _service.SetOption("MlGuideTest.ini");
            _service.ContentDirectory("TestDir");

            var kernel = _service.Kernel;
            var logger = new Logger();
            //var manageDir = TestUtil.GetTmpDir("TestDir");
            var manageDir = Path.Combine(kernel.Enviroment.ExecutableDirectory, "TestDir");


            _tsMailSave = new TsMailSave(); //MailSaveのモックオブジェクト

            var memberList = new Dat(new[] { CtrlType.TextBox, CtrlType.TextBox, CtrlType.CheckBox, CtrlType.CheckBox, CtrlType.CheckBox, CtrlType.TextBox });
            memberList.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "USER1", "user1@example.com", false, true, true, "")); //一般・読者・投稿
            memberList.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "USER2", "user2@example.com", false, true, false, "")); //一般・読者・×
            memberList.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "USER3", "user3@example.com", false, false, true, "")); //一般・×・投稿
            memberList.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "ADMIN", "admin@example.com", true, false, true, "123")); //管理者・×・投稿
            memberList.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "ADMIN2", "admin2@example.com", true, true, true, "456")); //管理者・読者・投稿
            var docs = (from object o in Enum.GetValues(typeof(MlDocKind)) select "").ToList();
            const int maxSummary = 10;
            const int getMax = 10;
            const bool autoRegistration = true;
            const int titleKind = 5;
            var mlOption = new MlOption(maxSummary, getMax, autoRegistration, titleKind, docs, manageDir,
                                        memberList);

            _ml = new Ml(kernel, logger, _tsMailSave, mlOption, mlName, domainList);

        }

        public void Dispose()
        {
            _tsMailSave.Dispose();
            _ml.Remove();
        }

        [Theory]
        [InlineData("user1")]//メンバからのリクエスト
        [InlineData("xxxx")]//メンバ外からのリクエスト(メンバ外からもguideは取得できる
        public void GuideTest(string user)
        {
            //    ドメインを追加
            const string domain = "@example.com";
            var from = user + domain;

            var mail = new TsMail(from, "1ban-ctl" + domain, "guide");
            _ml.Job(mail.MlEnvelope, mail.Mail);

            Assert.Equal(_tsMailSave.Count(), 1);
            var m = _tsMailSave.GetMail(0);
            //送信者
            Assert.Equal(m.GetHeader("from"), "1ban-admin" + domain);
            //件名
            Assert.Equal(m.GetHeader("subject"), "guide (1ban ML)");

        }

        [Theory]
        [InlineData("user1", "help (1ban ML)")]//メンバからのリクエスト(成功)
        //[InlineData("admin", "help (1ban ML)")]//管理者からのリクエスト(成功)
        [InlineData("xxxx", "You are not member (1ban ML)")]//メンバ外からのリクエスト(失敗)
        public void HelpTest(string user, string subject)
        {
            //    ドメインを追加
            const string domain = "@example.com";
            string from = user + domain;

            var mail = new TsMail(from, "1ban-ctl" + domain, "help");
            _ml.Job(mail.MlEnvelope, mail.Mail);

            Assert.Equal(_tsMailSave.Count(), 1);
            var m = _tsMailSave.GetMail(0);
            //送信者
            Assert.Equal(m.GetHeader("from"), "1ban-admin" + domain);
            //件名
            Assert.Equal(m.GetHeader("subject"), subject);

        }

    }

}
