using System;
using Bjd.ctrl;
using Bjd.mail;
using Bjd.option;
using Xunit;
using Bjd.SmtpServer;
using Bjd;

namespace Bjd.SmtpServer.Test
{

    public class MlUsersTest : IDisposable
    {

        MlUserList _mlUserList;

        public MlUsersTest()
        {
            //var kernel = new Kernel(null,null,null,null);
            //var logger = new Logger(kernel,"",false,null);

            //参加者
            var dat = new Dat(new[] { CtrlType.TextBox, CtrlType.TextBox, CtrlType.CheckBox, CtrlType.CheckBox, CtrlType.CheckBox, CtrlType.TextBox });
            bool manager = false;
            dat.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "USER1", "user1@example.com", manager, true, true, "")); //読者・投稿
            dat.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "USER2", "user2@example.com", manager, true, false, ""));//読者 　×
            dat.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "USER3", "user3@example.com", manager, false, true, ""));//×　　投稿
            manager = true;//管理者
            dat.Add(true, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "ADMIN", "admin@example.com", manager, false, true, "123"));//×　　投稿

            _mlUserList = new MlUserList(dat);
        }


        public void Dispose()
        {

        }

        //検索に成功するテスト
        [Theory]
        [InlineData("user1@example.com", "USER1", false, true, true, "")]
        [InlineData("user2@example.com", "USER2", false, true, false, "")]
        [InlineData("user3@example.com", "USER3", false, false, true, "")]
        [InlineData("admin@example.com", "ADMIN", true, false, true, "")]
        public void SearchSuccessTest(string mailAddress, string name, bool isManager, bool isReader, bool isContributor, string password)
        {
            //正常に登録されているか検索してみる
            var o = _mlUserList.Search(new MailAddress(mailAddress));//検索
            //名前
            Assert.Equal(o.Name, name);
            //メールアドレス
            Assert.Equal(o.MailAddress.ToString(), mailAddress);
            //管理者
            Assert.Equal(o.IsManager, isManager);
            //読者
            Assert.Equal(o.IsReader, isReader);
            //投稿
            Assert.Equal(o.IsContributor, isContributor);
            //パスワード
            Assert.Equal(o.Psssword, password);
        }

        //検索に失敗するテスト
        [Theory]
        [InlineData("1@1", false)]
        public void SearchErrorTest(string mailAddress, bool find)
        {
            var o = _mlUserList.Search(new MailAddress(mailAddress));//検索
            Assert.Null(o);
        }

        //追加して検索するテスト
        [Theory]
        [InlineData("1@1")]
        public void AddSearchTest(string mailAddress)
        {

            //ユーザを追加する
            _mlUserList.Add(new MailAddress(mailAddress), "追加");
            var o = _mlUserList.Search(new MailAddress(mailAddress));//追加したユーザを検索する

            //内容を確認する
            Assert.NotEqual(o, null);
            Assert.Equal(o.IsManager, false);
            Assert.Equal(o.IsContributor, true);
            Assert.Equal(o.IsReader, true);
            Assert.Equal(o.MailAddress.ToString(), mailAddress);
        }



    }
}
