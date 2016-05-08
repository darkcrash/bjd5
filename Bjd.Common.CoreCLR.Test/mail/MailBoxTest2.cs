using System;
using Bjd;
using Bjd.Logs;
using Bjd.Mails;
using Bjd.Option;
using Xunit;

namespace Bjd.Common.Test.mail
{

    public class MailBoxTest2 : IDisposable
    {

        private static TmpOption _op = null; //設定ファイルの上書きと退避
        private Conf _conf;

        public MailBoxTest2()
        {
            //設定ファイルの退避と上書き
            _op = new TmpOption("Bjd.CoreCLR.Test", "MailBoxTest.ini");
            var kernel = new Kernel();
            var oneOption = new OptionMailBox(kernel, "");
            _conf = new Conf(oneOption);
        }

        public void Dispose()
        {
            //設定ファイルのリストア
            _op.Dispose();
        }

        [Theory]
        [InlineData("user1", "user1", true)]
        public void AuthTest(string user, string pass, bool expected)
        {
            //setUp
            var dir = (String)_conf.Get("dir");
            var datUser = (Dat)_conf.Get("user");
            var sut = new MailBox(new Logger(), datUser, dir);
            //var expected = true;
            //exercise
            var actual = sut.Auth(user, pass);
            //verify
            Assert.Equal(expected, actual);
        }


    }

}
