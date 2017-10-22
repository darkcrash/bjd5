using System;
using System.Collections.Generic;
using System.Linq;
using Bjd.Controls;
using Bjd.Logs;
using Bjd.Mailbox;
using Bjd.Configurations;
using Bjd.Utils;
using Bjd.Test;
using Xunit;
using Bjd.SmtpServer;
using Bjd;
using System.Text;
using Bjd.Initialization;
using System.IO;
using Xunit.Abstractions;

namespace Bjd.SmtpServer.Test
{

    public class MlGetTest : IDisposable
    {
        private TestService _service;
        private Ml _ml;
        private TsMailSave _tsMailSave;


        public MlGetTest(ITestOutputHelper output)
        {
            const string mlName = "1ban";
            var domainList = new List<string> { "example.com" };
            //var tsDir = new TsDir();
            _service = TestService.CreateTestService();
            _service.SetOption("MlGetTest.ini");
            _service.AddOutput(output);

            var kernel = _service.Kernel;
            kernel.ListInitialize();

            var logger = _service.Kernel.Logger;
            var manageDir = _service.GetTmpDir("TestDir");

            _tsMailSave = new TsMailSave(kernel);//MailSaveのモックオブジェクト

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
            _tsMailSave.Clear();
            _tsMailSave.Dispose();
            _ml.Remove();
            _ml.Dispose();
            _service.Dispose();
        }

        //蓄積されていないメールをリクエスト
        [Theory]
        [InlineData("get 3", "ERROR \"GET 3\"")]//エラーのためメールは帰らない（エラーがログに出力されている）
        [InlineData("get 5-10", "ERROR \"GET 5-10\"")]//エラーのためメールは帰らない（エラーがログに出力されている）
        [InlineData("get", "not found No.1 (1ban ML)")]
        public void Save0Test(string body, string subject)
        {
            //    ドメインを追加
            const string domain = "@example.com";
            const string from = "user1" + domain;

            var mail = new TsMail(_service, from, "1ban-ctl" + domain, body);
            _ml.Job(mail.MlEnvelope, mail.Mail);

            Assert.Equal(1, _tsMailSave.Count()); //返されるエラーメールは1通
            Assert.Equal(_tsMailSave.GetMail(0).GetHeader("subject"), subject);
            Assert.Equal(_tsMailSave.GetFrom(0).ToString(), "1ban-admin" + domain);

        }

        //30通蓄積された状態
        //MaxGet=10以内で処理できるリクエスト
        [Theory]
        [InlineData("get 3", 3, 3, 1)]
        [InlineData("get 1-3", 1, 3, 3)]
        [InlineData("get 0-10", 1, 10, 10)]
        [InlineData("get 20", 20, 20, 1)]
        [InlineData("get 16-25", 16, 25, 10)]
        [InlineData("get 25-34", 25, 30, 6)]//範囲を超えたリクエスト
        [InlineData("get last:3", 28, 30, 3)]
        [InlineData("get first:5", 1, 5, 5)]
        public void Save30UnderMaxTest(string body, int start, int end, int attach)
        {

            const string domain = "@example.com";
            const string from = "user1" + domain;

            for (int i = 0; i < 30; i++)
            {
                var m = new TsMail(_service, from, "1ban" + domain, "DMY");
                m.Mail.AddHeader("subject", string.Format("TEST_{0}", i));//試験的に件名を挿入する
                _ml.Job(m.MlEnvelope, m.Mail);
            }
            //この時点で、user1,user2,adin2のそれぞれ30通が送信されているため_tsMailSave.Count()は90となる
            //事後のテストのため一度クリアする
            _tsMailSave.Clear();

            //リクエスト
            var mail = new TsMail(_service, from, "1ban-ctl" + domain, body);
            _ml.Job(mail.MlEnvelope, mail.Mail);

            Assert.Equal(1, _tsMailSave.Count()); //返されるメールは１通
            var subject = string.Format("result for get [{0}-{1} MIME/multipart] (1ban ML)", start, end);
            Assert.Equal(_tsMailSave.GetMail(0).GetHeader("subject"), subject);
            Assert.Equal(_tsMailSave.GetFrom(0).ToString(), "1ban-admin" + domain);

            //添付されているメールの通数確認
            var ar = GetAttach(_tsMailSave.GetMail(0));
            Assert.Equal(ar.Count, attach);

        }



        //30通蓄積された状態
        //MaxGet=10を超えるリクエスト
        [Theory]
        [InlineData("get 1-15", 2, 11, 15, 5)] //全部で2通受け取り、最後のメールには11～15が添付される
        [InlineData("get 1-35", 3, 21, 30, 10)] //全部で3通受け取り、最後のメールには21～30が添付される
        [InlineData("get 25-35", 1, 25, 30, 6)]
        [InlineData("get", 3, 21, 30, 10)] //全部で3通受け取り、最後のメールには21～30が添付される
        [InlineData("get 15-10", 1, 10, 15, 6)] //(StartとEndが逆転している)全部で1通受け取り、最後のメールには10～15が添付される
        [InlineData("get First:15", 2, 11, 15, 5)]
        [InlineData("get Last:15", 2, 26, 30, 5)]
        public void Save30AboveMaxTest(string body, int count, int start, int end, int attach)
        {

            const string domain = "@example.com";
            const string from = "user1" + domain;

            for (int i = 0; i < 30; i++)
            {
                var m = new TsMail(_service, from, "1ban" + domain, "DMY");
                m.Mail.AddHeader("subject", string.Format("TEST_{0}", i));//試験的に件名を挿入する

                _ml.Job(m.MlEnvelope, m.Mail);
            }
            //この時点で、user1,user2,adin2のそれぞれ30通が送信されているため_tsMailSave.Count()は90となる
            //事後のテストのため一度クリアする
            _tsMailSave.Clear();

            //リクエスト
            var mail = new TsMail(_service, from, "1ban-ctl" + domain, body);

            _ml.Job(mail.MlEnvelope, mail.Mail);

            Assert.Equal(_tsMailSave.Count(), count); //返されるメールはcount通

            var subject = string.Format("result for get [{0}-{1} MIME/multipart] (1ban ML)", start, end);
            Assert.Equal(_tsMailSave.GetMail(count - 1).GetHeader("subject"), subject);

            //添付されているメールの通数確認
            var ar = GetAttach(_tsMailSave.GetMail(count - 1));
            Assert.Equal(ar.Count, attach);

        }

        //メール本文から添付されているメールを取り出す
        List<Mail> GetAttach(Mail orgMail)
        {
            var ar = new List<Mail>();

            var lines = new List<string>();
            foreach (var buf in Inet.GetLines(orgMail.GetBody()))
            {
                var s = Encoding.ASCII.GetString(buf);
                lines.Add(s);
            }
            Mail mail = null;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].IndexOf("--BJD-Boundary--") != -1)
                {
                    break;
                }
                if (lines[i].IndexOf("--BJD-Boundary") != -1)
                {
                    if (mail != null)
                        ar.Add(mail);
                    do
                    {
                        i++;
                    } while (lines[i] != "\r\n");
                    mail = new Mail(_service.Kernel);
                    continue;
                }
                if (mail != null)
                {
                    mail.AppendLine(Encoding.ASCII.GetBytes(lines[i]));
                }
            }
            if (mail != null)
                ar.Add(mail);
            return ar;
        }

    }
}