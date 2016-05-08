using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd.Server;
using Xunit;
using Bjd.SmtpServer;

namespace SmtpServerTest
{
    public class CheckParamTest
    {

        List<string> CreateParam(String str)
        {
            var tmp = str.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            SmtpCmd smtpCmd;
            if (tmp.Length == 1)
            {
                smtpCmd = new SmtpCmd(new Cmd(str, tmp[0], null));
            }
            else
            {
                smtpCmd = new SmtpCmd(new Cmd(str, tmp[0], tmp[1]));
            }
            return smtpCmd.ParamList;
        }

        [Theory]
        [InlineData("mail from: <1@1>")]
        [InlineData("mail from: \"<1@1>\"")]
        [InlineData("mail from: \"AAA<1@1>\"")]
        [InlineData("mail from: 1@1")]
        public void Mailコマンドのチェック_正常(String str)
        {
            //setUp
            const bool useNullFrom = false;
            const bool useNullDomain = false;
            var sut = new CheckParam(useNullFrom, useNullDomain);
            var paramList = CreateParam(str);

            var expected = true;

            //exercise
            var actual = sut.Mail(paramList);
            //verify
            Assert.Equal(expected, actual);
        }


        [Theory]
        [InlineData("mail from 1@1")]
        [InlineData("mail From")]
        [InlineData("mail XXX")]
        public void Mailコマンドのチェック_異常_Frmo不正(String str)
        {
            //setUp
            const bool useNullFrom = false;
            const bool useNullDomain = false;
            var sut = new CheckParam(useNullFrom, useNullDomain);
            var paramList = CreateParam(str);
            var expected = string.Format("501 5.5.2 Syntax error in parameters scanning {0}", paramList[0]);

            //exercise
            sut.Mail(paramList);
            var actual = sut.Message;
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("mail")]
        public void Mailコマンドのチェック_異常(String str)
        {
            //setUp
            const bool useNullFrom = false;
            const bool useNullDomain = false;
            var sut = new CheckParam(useNullFrom, useNullDomain);
            var paramList = CreateParam(str);
            var expected = string.Format("501 Syntax error in parameters scanning \"\"");

            //exercise
            sut.Mail(paramList);
            var actual = sut.Message;
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("mail from: user")]
        [InlineData("mail from: user@")]
        public void Mailコマンドのチェック_異常_ドメイン名なしを許容しない(String str)
        {
            //setUp
            const bool useNullFrom = false;
            const bool useNullDomain = false; //ドメイン名なしを許容しない
            var sut = new CheckParam(useNullFrom, useNullDomain);
            var paramList = CreateParam(str);

            var expected = String.Format("553 {0}... Domain part missing", paramList[1]);

            //exercise
            sut.Mail(paramList);
            var actual = sut.Message;
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("mail from: user")]
        [InlineData("mail from: user@")]
        public void Mailコマンドのチェック_異常_ドメイン名なしを許容する(String str)
        {
            //setUp
            const bool useNullFrom = false;
            const bool useNullDomain = true; //ドメイン名なしを許容する
            var sut = new CheckParam(useNullFrom, useNullDomain);
            var paramList = CreateParam(str);

            var expected = true;

            //exercise
            var actual = sut.Mail(paramList);
            //verify
            Assert.Equal(expected, actual);
        }


        [Theory]
        [InlineData("rcpt to: <1@1>")]
        [InlineData("rcpt to: \"<1@1>\"")]
        [InlineData("rcpt to: \"AAA<1@1>\"")]
        [InlineData("rcpt to: 1@1")]
        public void Rcptコマンドのチェック_正常(String str)
        {
            //setUp
            const bool useNullFrom = false;
            const bool useNullDomain = false;
            var sut = new CheckParam(useNullFrom, useNullDomain);
            var paramList = CreateParam(str);

            var expected = true;

            //exercise
            var actual = sut.Rcpt(paramList);
            //verify
            Assert.Equal(expected, actual);
        }


        [Theory]
        [InlineData("rcpt to 1@1")]
        [InlineData("rcpt to")]
        [InlineData("rcpt XXX")]
        public void Rcptコマンドのチェック_異常_Frmo不正(String str)
        {
            //setUp
            const bool useNullFrom = false;
            const bool useNullDomain = false;
            var sut = new CheckParam(useNullFrom, useNullDomain);
            var paramList = CreateParam(str);
            var expected = string.Format("501 5.5.2 Syntax error in parameters scanning {0}", paramList[0]);

            //exercise
            sut.Rcpt(paramList);
            var actual = sut.Message;
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("rcpt")]
        public void Rcptコマンドのチェック_異常(String str)
        {
            //setUp
            const bool useNullFrom = false;
            const bool useNullDomain = false;
            var sut = new CheckParam(useNullFrom, useNullDomain);
            var paramList = CreateParam(str);
            var expected = string.Format("501 Syntax error in parameters scanning \"\"");

            //exercise
            sut.Rcpt(paramList);
            var actual = sut.Message;
            //verify
            Assert.Equal(expected, actual);
        }


    }
}
