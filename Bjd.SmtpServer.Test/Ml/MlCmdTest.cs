﻿using System;
using System.Linq;
using System.Text;
using Bjd;
using Bjd.Mailbox;
using Xunit;
using Bjd.SmtpServer;
using Bjd.Initialization;
using Xunit.Abstractions;

namespace Bjd.SmtpServer.Test
{
    public class MlCmdTest : IDisposable
    {

        private MlOneUser _user1;
        private TestService _service;
        private Kernel _kernel;

        public MlCmdTest(ITestOutputHelper output)
        {
            _service = TestService.CreateTestService();
            _service.AddOutput(output);
            _kernel = _service.Kernel;
            _user1 = new MlOneUser(true, "USER1", new MailAddress("user1@example.com"), false, true, true, "password");
        }

        public void Dispose()
        {
            _service.Dispose();
        }

        //１行コマンド
        [Theory]
        [InlineData("   get 3", MlCmdKind.Get, "3")]//余分な空白を含む
        [InlineData("   get 3    ", MlCmdKind.Get, "3")]//余分な空白を含む
        [InlineData("GET 3", MlCmdKind.Get, "3")]
        [InlineData("GeT 3", MlCmdKind.Get, "3")]
        [InlineData("geT 3", MlCmdKind.Get, "3")]
        [InlineData("get 3", MlCmdKind.Get, "3")]
        [InlineData("get 3-10", MlCmdKind.Get, "3-10")]
        [InlineData("get", MlCmdKind.Get, "")]
        [InlineData("add", MlCmdKind.Add, "")]

        public void TestWithStringParam(string cmdStr, MlCmdKind mlCmdKind, string paramStr)
        {
            var mail = new Mail(_kernel);
            mail.AppendLine(Encoding.ASCII.GetBytes("\r\n"));//区切り行(ヘッダ終了)
            mail.AppendLine(Encoding.ASCII.GetBytes(cmdStr));//区切り行(ヘッダ終了)
            var mlCmd = new MlCmd(null, mail, _user1);

            Assert.Single(mlCmd.Cast<object>()); // コマンド数は１

            foreach (OneMlCmd oneMlCmd in mlCmd)
            {
                Assert.Equal(oneMlCmd.CmdKind, mlCmdKind);
                Assert.Equal(oneMlCmd.ParamStr, paramStr);
                break;
            }
        }

        //複数行コマンド
        [Theory]
        [InlineData("get 3\r\nadd\r\nmember", 3)]
        [InlineData("get 3\r\n\r\nmember", 2)]//空行を含む
        [InlineData("\r\n\r\n\r\n\r\nmember", 1)]//空行を含む
        public void Test(string cmdStr, int count)
        {
            var mail = new Mail(_kernel);
            mail.AppendLine(Encoding.ASCII.GetBytes("\r\n"));//区切り行(ヘッダ終了)
            mail.AppendLine(Encoding.ASCII.GetBytes(cmdStr));//区切り行(ヘッダ終了)
            var mlCmd = new MlCmd(null, mail, _user1);

            Assert.Equal(mlCmd.Cast<object>().Count(), count); // コマンド数
        }
    }

}
