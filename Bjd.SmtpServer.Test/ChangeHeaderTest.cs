﻿using System.Text;
using Bjd.Controls;
using Bjd.Logs;
using Bjd.Mailbox;
using Bjd.Configurations;
using Bjd.Utils;
using Xunit;
using Bjd.SmtpServer;
using Bjd.Initialization;
using Xunit.Abstractions;

namespace Bjd.SmtpServer.Test
{
    public class ChangeHeaderTest
    {
        private ITestOutputHelper output;

        public ChangeHeaderTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Relpaceによるヘッダの置き換え()
        {
            using (var sv = TestService.CreateTestService())
            {
                sv.AddOutput(output);
                sv.Kernel.ListInitialize();

                //setUp
                var replace = new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox });
                replace.Add(true, "ABC\tXYZ");
                var sut = new ChangeHeader(replace, null);

                var mail = new Mail(sv.Kernel);
                mail.AddHeader("tag1", "ABC123");
                mail.AddHeader("tag2", "DEF123");
                mail.AddHeader("tag3", "GHI123");

                var expected = "tag1: XYZ123\r\n";

                //exercise
                sut.Exec(mail, new Logger(sv.Kernel));
                var actual = Encoding.ASCII.GetString(mail.GetBytes()).Substring(0, 14);

                //varify
                Assert.Equal(expected, actual);

            }

        }

        [Fact]
        public void Relpaceによるヘッダの置き換え2()
        {
            using (var sv = TestService.CreateTestService())
            {
                sv.AddOutput(output);
                sv.Kernel.ListInitialize();
                //setUp
                var replace = new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox });
                replace.Add(true, "ABC\tBBB");
                var sut = new ChangeHeader(replace, null);

                var mail = new Mail(sv.Kernel);
                mail.AddHeader("tag1", "ABC123");
                mail.AddHeader("tag2", "DEF123");
                mail.AddHeader("tag3", "GHI123");

                var expected = "BBB123";

                //exercise
                sut.Exec(mail, new Logger(sv.Kernel));
                var actual = mail.GetHeader("tag1");

                //varify
                Assert.Equal(expected, actual);

            }
        }

        [Fact]
        public void Relpaceによるヘッダの置き換え3()
        {
            using (var sv = TestService.CreateTestService())
            {
                sv.AddOutput(output);
                sv.Kernel.ListInitialize();
                //setUp
                var replace = new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox });
                replace.Add(true, "EFGH\tWXYZ");
                var sut = new ChangeHeader(replace, null);

                var mail = new Mail(sv.Kernel);
                mail.AddHeader("To", "\"ABCD\" <****@******>");
                mail.AddHeader("From", "\"EFGH\" <****@******>");
                mail.AddHeader("Subject", "test");

                var expected = "\"WXYZ\" <****@******>";

                //exercise
                sut.Exec(mail, new Logger(sv.Kernel));
                var actual = mail.GetHeader("From");

                //varify
                Assert.Equal(expected, actual);

            }
        }

        [Fact]
        public void Relpaceによるヘッダの置き換え4()
        {
            using (var sv = TestService.CreateTestService())
            {
                sv.AddOutput(output);
                sv.Kernel.ListInitialize();
                //setUp
                var replace = new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox });
                replace.Add(true, "User-Agent:.*\tUser-Agent:Henteko Mailer 09.87.12");
                var sut = new ChangeHeader(replace, null);

                var mail = new Mail(sv.Kernel);
                mail.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 5.1; rv:17.0) Gecko/20130801 Thunderbird/17.0.8");

                var expected = "Henteko Mailer 09.87.12";

                //exercise
                sut.Exec(mail, new Logger(sv.Kernel));
                var actual = mail.GetHeader("User-Agent");

                //varify
                Assert.Equal(expected, actual);

            }
        }




        [Fact]
        public void Appendによるヘッダの追加()
        {
            using (var sv = TestService.CreateTestService())
            {
                sv.AddOutput(output);
                sv.Kernel.ListInitialize();
                //setUp
                var appned = new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox });
                appned.Add(true, "tag2\tzzz");
                var sut = new ChangeHeader(null, appned);

                var mail = new Mail(sv.Kernel);

                var expected = "zzz";

                //exercise
                sut.Exec(mail, new Logger(sv.Kernel));
                var actual = mail.GetHeader("tag2");

                //varify
                Assert.Equal(expected, actual);

            }
        }


    }
}
