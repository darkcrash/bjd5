using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Bjd.log;
using Bjd.mail;
using Bjd.net;
using Xunit;
using Bjd.SmtpServer;

namespace Bjd.SmtpServer.Test
{
    public class MailQueueTest : IDisposable
    {
        private MailQueue sut;

        public MailQueueTest()
        {
            sut = new MailQueue("c:\\tmp2\\bjd5\\SmtpServerTest");
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(sut.Dir);
            }
            catch (Exception)
            {
                try
                {
                    Directory.Delete(sut.Dir, true);
                }
                catch (Exception)
                {

                }
            }
        }

        MailInfo CreateMailInfo()
        {
            var uid = "AAA1234567890";
            var size = 500;
            var host = "sw01";
            var addr = new Ip(IpKind.V4Localhost);
            //var date = "2013/01/01";
            var from = new MailAddress("user1@example.com");
            var to = new MailAddress("user2@example.com");
            return new MailInfo(uid, size, host, addr, from, to);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(99)]
        public void Saveによるn通の保存(int n)
        {
            //setUp
            var max = 100;
            var threadSpan = 0; //最小経過時間

            var mail = new Mail();
            var mailInfo = CreateMailInfo();
            for (int i = 0; i < n; i++)
            {
                sut.Save(mail, mailInfo);
            }
            var expected = n;

            //exerceise
            var actual = sut.GetList(max, threadSpan).Count;

            //verify
            Assert.Equal(expected, actual);

        }

        [Theory]
        [InlineData(0, 10)]  // 最小経過時間を0秒で指定した場合、全部が取得される
        [InlineData(1, 10)] // 最小経過時間を1秒で指定した場合、全部が取得される
        [InlineData(2, 0)]  //最小経過時間を2秒で指定した場合、まったく取得されない
        public void GetListによる経過時間指定による一覧取得の違い(int sec, int count)
        {
            //setUp
            var max = 10;
            for (var i = 0; i < max; i++)
            {
                sut.Save(new Mail(), CreateMailInfo());
            }
            var expected = count;
            //一度一覧取得を行う
            sut.GetList(max, sec);
            //経過時間
            Thread.Sleep(1000);

            //exerceise
            var actual = sut.GetList(max, sec).Count;

            //verify
            Assert.Equal(expected, actual);

        }


        [Theory]
        [InlineData(5)]
        [InlineData(1)]
        public void Deleteによるn通の削除(int n)
        {
            //setUp
            var max = 10;
            var threadSpan = 0; //最小経過時間

            var mail = new Mail();
            var mailInfo = CreateMailInfo();
            for (int i = 0; i < max; i++)
            {
                sut.Save(mail, mailInfo);
            }
            var expected = max - n;
            var list = sut.GetList(max, threadSpan);

            //exerceise
            foreach (var l in list)
            {
                var filename = l.MailInfo.FileName;
                sut.Delete(filename);
                n--;
                if (n == 0)
                    break;
            }
            var actual = sut.GetList(max, threadSpan).Count;

            //verify
            Assert.Equal(expected, actual);

        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(9)]
        public void Readによるn通目の読み込み(int n)
        {
            //setUp
            var max = 10;
            var threadSpan = 0; //最小経過時間

            var mail = new Mail();
            var expected = string.Format("{0}", n);
            mail.AddHeader("tag", expected);
            var mailInfo = CreateMailInfo();
            for (int i = 0; i < max; i++)
            {
                sut.Save(mail, mailInfo);
            }
            var list = sut.GetList(max, threadSpan);

            //exerceise
            sut.Read(list[n].MailInfo.FileName, ref mail);
            var actual = mail.GetHeader("tag");

            //verify
            Assert.Equal(expected, actual);

        }



    }
}
