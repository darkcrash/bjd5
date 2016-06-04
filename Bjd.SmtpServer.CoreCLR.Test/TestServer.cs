using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Bjd;
using Bjd.Mails;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;
using Bjd.Test;
using Bjd.Services;

namespace Bjd.SmtpServer.Test
{
    public enum TestServerType
    {
        Pop,
        Smtp
    }

    public class TestServer : IDisposable
    {

        //private readonly TestOption _op; //設定ファイルの上書きと退避
        public readonly TestService _service;
        private readonly OneServer _v6Sv; //サーバ
        private readonly OneServer _v4Sv; //サーバ

        public TestServer(TestServerType type, String iniSubDir, String iniFileName)
        {
            //TestUtil.CopyLangTxt();//BJD.Lang.txt

            var confName = type == TestServerType.Pop ? "Pop3" : "Smtp";

            //設定ファイルの退避と上書き
            //_op = new TestOption(iniSubDir, iniFileName);

            _service = TestService.CreateTestService();
            _service.ContentFile(iniSubDir, iniFileName);

            var kernel = _service.Kernel;
            var option = kernel.ListOption.Get(confName);
            var conf = new Conf(option);


            //サーバ起動
            if (type == TestServerType.Pop)
            {
                _v4Sv = new Pop3Server.Server(kernel, conf, new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
                _v6Sv = new Pop3Server.Server(kernel, conf, new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
            }
            else
            {
                _v4Sv = new SmtpServer.Server(kernel, conf, new OneBind(new Ip(IpKind.V4Localhost), ProtocolKind.Tcp));
                _v6Sv = new SmtpServer.Server(kernel, conf, new OneBind(new Ip(IpKind.V6Localhost), ProtocolKind.Tcp));
            }
            _v4Sv.Start();
            _v6Sv.Start();

            Thread.Sleep(100); //少し余裕がないと多重でテストした場合に、サーバが起動しきらないうちにクライアントからの接続が始まってしまう。

        }

        public String ToString(InetKind inetKind)
        {
            if (inetKind == InetKind.V4)
            {
                return _v4Sv.ToString();
            }
            return _v6Sv.ToString();
        }

        public void SetMail(String user, String fileName)
        {
            //メールボックスへのデータセット
            //var srcDir = AppContext.BaseDirectory;
            //var dstDir = System.IO.Path.Combine(TestDefine.Instance.TestMailboxPath, user);
            //Directory.CreateDirectory(dstDir);
            //File.Copy(srcDir + "DF_" + fileName, dstDir + "DF_" + fileName, true);
            //File.Copy(srcDir + "MF_" + fileName, dstDir + "MF_" + fileName, true);
            //File.Copy(Path.Combine(srcDir, "DF_" + fileName), Path.Combine(dstDir, "DF_" + fileName), true);
            //File.Copy(Path.Combine(srcDir, "MF_" + fileName), Path.Combine(dstDir, "MF_" + fileName), true);
            //_service.ContentFileWithDestnationPath("DF_" + fileName, Path.Combine("mailbox", user, "DF_" + fileName));
            //_service.ContentFileWithDestnationPath("MF_" + fileName, Path.Combine("mailbox", user, "MF_" + fileName));
            _service.AddMail("DF_" + fileName, user);
            _service.AddMail("MF_" + fileName, user);

        }

        //DFファイルの一覧を取得する
        public string[] GetDf(string user)
        {
            //var dir = string.Format("c:\\tmp2\\bjd5\\SmtpServerTest\\mailbox\\{0}", user);
            //var dir = String.Format("{0}\\SmtpServerTest\\mailbox\\{1}", TestUtil.ProjectDirectory(), user);
            //var dir = Path.Combine(TestDefine.Instance.TestMailboxPath, user);
            var dir = Path.Combine(_service.MailboxPath, user);
            if (!Directory.Exists(dir))
                return new string[] { };
            var files = Directory.GetFiles(dir, "DF*");
            return files;
        }

        //メールの一覧を取得する
        public List<Mail> GetMf(string user)
        {
            //var dir = String.Format("{0}\\SmtpServerTest\\mailbox\\{1}", TestUtil.ProjectDirectory(), user);
            //var dir = Path.Combine(TestDefine.Instance.TestMailboxPath, user);
            var dir = Path.Combine(_service.MailboxPath, user);
            if (!Directory.Exists(dir))
                return new List<Mail>();
            var ar = new List<Mail>();
            foreach (var fileName in Directory.GetFiles(dir, "MF*"))
            {
                var mail = new Mail();
                mail.Read(fileName);
                ar.Add(mail);
            }
            return ar;
        }


        public virtual void Dispose()
        {
            //サーバ停止
            _v4Sv.Stop();
            _v6Sv.Stop();

            _v4Sv.Dispose();
            _v6Sv.Dispose();

            //設定ファイルのリストア
            //_op.Dispose();

            ////メールボックスの削除
            ////var path = String.Format("{0}\\SmtpServerTest\\mailbox", TestUtil.ProjectDirectory());
            //var path = TestDefine.Instance.TestMailboxPath;
            ////Directory.Delete(@"c:\tmp2\bjd5\SmtpServerTest\mailbox", true);
            //Directory.Delete(path, true);
        }
    }
}
