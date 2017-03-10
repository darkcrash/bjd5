using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd.Servers;
using Xunit;
using Bjd.SmtpServer;

namespace Bjd.SmtpServer.Test
{
    public class SmtpCmdTest
    {

        Cmd CreateCmd(string str)
        {
            var tmp = str.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            return new Cmd(str, tmp[0], tmp[1]);

        }

        [Theory]
        [InlineData("Mail From: 1@1")]
        [InlineData("Mail From:1@1")]
        public void ParamListの確認(string str)
        {
            //setUp
            var sut = new SmtpCmd(CreateCmd(str));
            //exercise
            var actual = sut.ParamList;
            //verify
            Assert.Equal(actual[0], "From:");
            Assert.Equal(actual[1], "1@1");
        }

        [Theory]
        [InlineData("Mail From: 1@1")]
        [InlineData("Mail From:1@1")]
        public void Kindの確認(string str)
        {
            //setUp
            var sut = new SmtpCmd(CreateCmd(str));
            var expected = SmtpCmdKind.Mail;
            //exercise
            var actual = sut.Kind;
            //verify
            Assert.Equal(expected, actual);
        }
    }
}
