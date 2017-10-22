using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd.Controls;
using Bjd.Configurations;
using Bjd.Utils;
using Xunit;
using Bjd.SmtpServer;

namespace Bjd.SmtpServer.Test
{
    public class SmtpAuthTest : IDisposable
    {

        private SmtpAuthUserList _smtpAuthUserList;

        public SmtpAuthTest()
        {
            var esmtpUserList = new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox });
            esmtpUserList.Add(true, "user1\t3OuFXZzV8+iY6TC747UpCA==");
            _smtpAuthUserList = new SmtpAuthUserList(false, null, esmtpUserList);
        }

        public void Dispose()
        {
        }


        [Fact]
        public void 認証前にIsFinishはfalseがセットされる()
        {
            //setUp
            const bool usePlain = true;
            const bool useLogin = true;
            const bool useCramMd5 = true;
            var sut = new SmtpAuth(_smtpAuthUserList, usePlain, useLogin, useCramMd5);
            var expected = false;

            //exercise
            var actual = sut.IsFinish;
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 認証前でも有効なモードが無いときIsFinishはtrueがセットされる()
        {
            //setUp
            const bool usePlain = false;
            const bool useLogin = false;
            const bool useCramMd5 = false;
            var sut = new SmtpAuth(_smtpAuthUserList, usePlain, useLogin, useCramMd5);
            var expected = true;

            //exercise
            var actual = sut.IsFinish;

            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 無効なAUTHコマンド()
        {
            //setUp
            const bool usePlain = true;
            const bool useLogin = true;
            const bool useCramMd5 = true;
            var sut = new SmtpAuth(_smtpAuthUserList, usePlain, useLogin, useCramMd5);

            var expected = "504 Unrecognized authentication type.";

            //exercise
            var actual = sut.Job("AUTH XXXX");

            //verify
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void 無効なAUTHコマンド2()
        {
            //setUp
            const bool usePlain = false; //無効になっている
            const bool useLogin = true;
            const bool useCramMd5 = true;
            var sut = new SmtpAuth(_smtpAuthUserList, usePlain, useLogin, useCramMd5);

            var expected = "500 command not understood: AUTH PLAIN\r\n";
            //exercise
            var actual = sut.Job("AUTH PLAIN");//本来は、正しいコマンドだが

            //verify
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void PLAINによる認証_成功()
        {
            //setUp
            const bool usePlain = true;
            const bool useLogin = false;
            const bool useCramMd5 = false;
            var sut = new SmtpAuth(_smtpAuthUserList, usePlain, useLogin, useCramMd5);
            Assert.Equal("334 ", sut.Job("AUTH PLAIN"));
            Assert.Equal("235 Authentication successful.", sut.Job(Base64.Encode("user1\0user1\0user1")));
            var expected = true;

            //exercise
            var actual = sut.IsFinish;

            //verify
            Assert.Equal(expected, actual);
        }




        [Fact]
        public void PLAINによる認証_失敗()
        {
            //setUp
            const bool usePlain = true;
            const bool useLogin = false;
            const bool useCramMd5 = false;
            var sut = new SmtpAuth(_smtpAuthUserList, usePlain, useLogin, useCramMd5);
            Assert.Equal("334 ", sut.Job("AUTH PLAIN"));
            String expected = null;

            //exercise
            var actual = sut.Job(Base64.Encode("user1\0user1\0user2"));

            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LOGINによる認証_成功()
        {
            //setUp
            const bool usePlain = false;
            const bool useLogin = true;
            const bool useCramMd5 = false;
            var sut = new SmtpAuth(_smtpAuthUserList, usePlain, useLogin, useCramMd5);

            Assert.Equal("334 VXNlcm5hbWU6", sut.Job("AUTH LOGIN"));
            Assert.Equal("334 UGFzc3dvcmQ6", sut.Job(Base64.Encode("user1")));
            Assert.Equal("235 Authentication successful.", sut.Job(Base64.Encode("user1")));

            var expected = true;

            //exercise
            var actual = sut.IsFinish;

            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LOGINによる認証_失敗()
        {
            //setUp
            const bool usePlain = false;
            const bool useLogin = true;
            const bool useCramMd5 = false;
            var sut = new SmtpAuth(_smtpAuthUserList, usePlain, useLogin, useCramMd5);

            Assert.Equal("334 VXNlcm5hbWU6", sut.Job("AUTH LOGIN"));
            Assert.Equal("334 UGFzc3dvcmQ6", sut.Job(Base64.Encode("user1")));

            String expected = null;
            //exercise
            var actual = sut.Job(Base64.Encode("xxx"));

            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LOGIN_初期パラメータ付_による認証_成功()
        {
            //setUp
            const bool usePlain = false;
            const bool useLogin = true;
            const bool useCramMd5 = false;
            var sut = new SmtpAuth(_smtpAuthUserList, usePlain, useLogin, useCramMd5);

            var str = String.Format("AUTH login {0}", Base64.Encode("user1"));
            Assert.Equal("334 UGFzc3dvcmQ6", sut.Job(str));
            Assert.Equal("235 Authentication successful.", sut.Job(Base64.Encode("user1")));

            var expected = true;

            //exercise
            var actual = sut.IsFinish;

            //verify
            Assert.Equal(expected, actual);
        }



        [Fact]
        public void CRAM_MD5による認証_成功()
        {
            //setUp
            const bool usePlain = false;
            const bool useLogin = false;
            const bool useCramMd5 = true;
            var sut = new SmtpAuth(_smtpAuthUserList, usePlain, useLogin, useCramMd5);

            var str = sut.Job("AUTH CRAM-MD5");

            var hash = Md5.Hash("user1", Base64.Decode(str.Substring(4)));
            Assert.Equal("235 Authentication successful.", sut.Job(Base64.Encode(string.Format("user1 {0}", hash))));
            var expected = true;

            //exercise
            var actual = sut.IsFinish;

            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CRAM_MD5による認証_失敗()
        {
            //setUp
            const bool usePlain = false;
            const bool useLogin = false;
            const bool useCramMd5 = true;
            var sut = new SmtpAuth(_smtpAuthUserList, usePlain, useLogin, useCramMd5);

            var str = sut.Job("AUTH CRAM-MD5");

            var hash = Md5.Hash("user2", Base64.Decode(str.Substring(4)));
            var expected = (String)null;

            //exercise
            var actual = sut.Job(Base64.Encode(string.Format("user1 {0}", hash)));

            //verify
            Assert.Equal(expected, actual);

        }

    }
}
