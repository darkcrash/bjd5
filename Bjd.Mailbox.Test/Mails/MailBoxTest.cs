﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using Bjd.Controls;
using Bjd.Logs;
using Bjd.Mailbox;
using Bjd.Net;
using Bjd.Configurations;
using Xunit;
using Bjd;
using System.Security.Cryptography;
using System.Threading;
using Bjd.Initialization;
using Xunit.Abstractions;

namespace Bjd.Test.Mails
{

    public class MailBoxTest : IDisposable
    {

        private MailBox sut;
        private Dat _datUser = null;
        private TestService _service;
        private Kernel _kernel;


        public MailBoxTest(ITestOutputHelper helper)
        {
            //const string dir = "mailbox";

            _service = TestService.CreateTestService();
            _service.AddOutput(helper);
            _service.CreateMailbox("user1");
            _service.CreateMailbox("user2");
            _service.CreateMailbox("user3");
            _kernel = _service.Kernel;
            _kernel.ListInitialize();

            _datUser = new Dat(new CtrlType[2] { CtrlType.TextBox, CtrlType.TextBox });
            _datUser.Add(true, "user1\t3OuFXZzV8+iY6TC747UpCA==");
            _datUser.Add(true, "user2\tNKfF4/Tw/WMhHZvTilAuJQ==");
            _datUser.Add(true, "user3\tXXX");

            var op = _kernel.ListOption.Get("MailBox");
            var conf = new Conf(op);
            conf.Add("user", _datUser);

            //sut = new MailBox(new Logger(_kernel), _datUser, _service.MailboxPath);
            sut = new MailBox(_kernel, conf);
        }

        public void Dispose()
        {
            Thread.Sleep(100);
            //後始末で、MainBoxフォルダごと削除する
            if (Directory.Exists(sut.Dir))
            {
                try
                {
                    Directory.Delete(sut.Dir);
                }
                catch
                {
                    Directory.Delete(sut.Dir, true);
                }
            }
            _service.Dispose();
        }

        [Fact]
        public void ステータス確認()
        {
            //setUp
            var expected = true;
            //exercise
            var actual = sut.Status;
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("user1", true)]
        [InlineData("user2", true)]
        [InlineData("user3", true)]
        [InlineData("", false)]
        [InlineData("xxx", false)]
        [InlineData(null, false)]
        public void IsUserによるユーザの存在確認(string user, bool expected)
        {
            //exercise
            var actual = sut.IsUser(user);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("user1", "user1")]
        [InlineData("user2", "user2")]
        [InlineData("user3", null)]//user3は、無効なパスワードで初期化されている
        [InlineData("xxx", null)]//存在しないユーザの問い合わせ
        public void GetPassによるパスワードの取得(string user, string expected)
        {
            //exercise
            var actual = sut.GetPass(user);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("user1", "user1", true)]
        [InlineData("user2", "user2", true)]
        [InlineData("user1", "xxx", false)]//パスワード誤り
        [InlineData("user3", "user3", false)]//パスワードが無効
        [InlineData("xxx", "xxx", false)]//登録外のユーザ
        [InlineData(null, "xxx", false)]//ユーザ名が無効（不正）
        [InlineData("user1", null, false)]//パスワードが無効（不正）
        public void Authによる認証(string user, string pass, bool expected)
        {
            //exercise
            var actual = sut.Auth(user, pass);
            //verify
            Assert.Equal(expected, actual);
        }


        [Theory]
        [InlineData("user1", "192.168.0.1")]
        [InlineData("user1", "10.0.0.1")]
        [InlineData("user2", "10.0.0.1")]
        [InlineData("user3", "10.0.0.1")]
        public void Loginによるログイン処理_成功(string user, string ip)
        {
            //setUp
            var expected = true;
            //exercise
            var actual = sut.Login(user, new Ip(ip));
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("xxx", "10.0.0.1")]//無効ユーザではログインできない
        [InlineData(null, "10.0.0.1")]//無効(不正)ユーザではログインできない
        public void Loginによるログイン処理_失敗(string user, string ip)
        {
            //setUp
            var expected = false; //失敗した場合はfalseが返される
            //exercise
            var actual = sut.Login(user, new Ip(ip));
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Login_二重ログインでfalseが返る()
        {
            //setUp
            var ip = new Ip("10.0.0.1");
            const string user = "user1";
            sut.Login(user, ip); //1回目のログイン
            var expected = false;//失敗した場合falseが返される
            //exercise
            var actual = sut.Login(user, ip); //２回目のログイン
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Login_二重ログイン後にログアウトすればログインは成功する()
        {
            //setUp
            const string user = "user1";
            var ip = new Ip("10.0.0.1");
            var expected = true;
            sut.Login(user, ip); //1回目のログイン
            sut.Login(user, ip); //2回目のログイン
            sut.Logout(user); //ログアウト
            //exercise
            var actual = sut.Login(user, ip); //２回目のログイン
            //verify
            Assert.Equal(expected, actual);
        }



        [Theory]
        [InlineData("user1", 0)]
        [InlineData("user1", 1000)]//１秒経過
        public void LastLoginによる最終ログイン時間の取得(string user, int waitMsec)
        {
            //Ticksは100ナノ秒単位
            //10で１マイクロ秒
            //10000で１ミリ秒
            //10000000で１秒
            //setUp
            var ip = new Ip("10.0.0.1");
            var now = DateTime.Now;//ログイン直前の時間計測
            sut.Login(user, ip);//ログイン
            Thread.Sleep(waitMsec);//経過時間
            var expected = true;

            //exercise
            var dt = sut.LastLogin(ip);//ログイン後の時間計測
            var actual = (dt.Ticks - now.Ticks) < 1000000; //10ミリ秒以下の誤差
            //verify
            Assert.Equal(expected, actual);

        }

        [Theory]
        [InlineData("xxx")]//無効(不正)ユーザではログインできない
        [InlineData(null)]//無効(不正)ユーザではログインできない
        public void LastLoginによる最終ログイン時間の取得_無効ユーザの場合0が返る(string user)
        {
            //setUp
            var ip = new Ip("10.0.0.1");
            sut.Login(user, ip);//ログイン
            var expected = 0;
            //exercise
            var actual = sut.LastLogin(ip).Ticks;
            //verify
            Assert.Equal(expected, actual);

        }


        [Theory]
        [InlineData("user1", false, true)]
        [InlineData("user1", true, true)]//ログアウトしても経過時間の取得は成功する
        [InlineData("xxx", false, false)]//無効ユーザ
        public void LogoutTest(string user, bool logout, bool success)
        {
            var ip = new Ip("10.0.0.1");
            sut.Login(user, ip);//ログイン
            if (logout)
            {
                sut.Logout(user);
            }
            var dt = sut.LastLogin(ip);//ログイン後の時間計測
            if (success)
            {
                Assert.NotEqual(0, dt.Ticks);//過去にログインした記録があれば0以外が返る
            }
            else
            {
                Assert.Equal(0, dt.Ticks);//過去にログイン形跡なし
            }
            sut.Logout(user);
        }





        [Fact]
        public void UserListによるユーザ一覧取得()
        {
            //exercise
            var actual = sut.UserList;
            //verify
            Assert.Equal(3, actual.Count);
            Assert.Equal("user1", actual[0]);
            Assert.Equal("user2", actual[1]);
            Assert.Equal("user3", actual[2]);
        }


        //保存件数（ファイル数)
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void SaveCountTest(int n)
        {
            var mail = new Mail(_kernel);
            const string uid = "XXX123";
            const int size = 100;
            const string host = "hostname";
            const string user = "user1";
            var ip = new Ip("10.0.0.1");
            var from = new MailAddress("1@1");
            var to = new MailAddress("2@2");
            var mailInfo = new MailInfo(uid, size, host, ip, from, to);

            //同一内容でn回送信
            for (int i = 0; i < n; i++)
            {
                sut.Save(user, mail, mailInfo);
            }

            //メールボックス内に蓄積されたファイル数を検証する
            var path = string.Format("{0}\\{1}", sut.Dir, user);
            var di = new DirectoryInfo(path);

            //DF_*がn個存在する
            var files = di.GetFiles("DF_*");
            Assert.Equal(files.Count(), n);
            //MF_*がn個存在する
            files = di.GetFiles("MF_*");
            Assert.Equal(files.Count(), n);

        }

        //保存（DF内容)
        [Theory]
        [InlineData("user1", true, "UID", 100, "hostname", "1@1", "2@2")]
        [InlineData("zzzz", false, "", 0, "", "", "")]//無効ユーザで保存失敗
        public void SaveDfTest(string user, bool status, string uid, int size, string hostname, string from, string to)
        {
            var mail = new Mail(_kernel);
            var ip = new Ip("10.0.0.1");
            var mailInfo = new MailInfo(uid, size, hostname, ip, new MailAddress(from), new MailAddress(to));

            var b = sut.Save(user, mail, mailInfo);
            //メールボックス内に蓄積されたファイル数を検証する
            var path = string.Format("{0}\\{1}", sut.Dir, user);
            var di = new DirectoryInfo(path);

            if (status)
            {
                Assert.True(b);//保存成功

                var files = di.GetFiles("DF_*");

                //メールボックス内に蓄積されたファイル数を検証する
                var lines = File.ReadAllLines(files[0].FullName);
                Assert.Equal(lines[0], uid); //１行目 uid
                Assert.Equal(lines[1], size.ToString()); //２行目 size
                Assert.Equal(lines[2], hostname); //３行目 hostname
                Assert.Equal(lines[3], ip.ToString()); //４行目 ip
                Assert.Equal(lines[7], from); //８行目 from
                Assert.Equal(lines[8], to); //９行目 to
            }
            else
            {
                Assert.False(b);//保存失敗
                Assert.False(Directory.Exists(di.FullName));
            }
        }


    }
}
