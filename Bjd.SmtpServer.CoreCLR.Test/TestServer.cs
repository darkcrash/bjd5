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
        public readonly TestService _service;
        private readonly OneServer _v6Sv; //サーバ
        private readonly OneServer _v4Sv; //サーバ
        public readonly int port;

        public TestServer(TestServerType type, string iniOption)
        {
            var confName = type == TestServerType.Pop ? "Pop3" : "Smtp";

            _service = TestService.CreateTestService();
            _service.SetOption(iniOption);

            var kernel = _service.Kernel;
            var option = kernel.ListOption.Get(confName);
            var conf = new Conf(option);
            port = _service.GetAvailablePort(IpKind.V4Localhost, conf);


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

            //Thread.Sleep(100); //少し余裕がないと多重でテストした場合に、サーバが起動しきらないうちにクライアントからの接続が始まってしまう。

        }

        public string ToString(InetKind inetKind)
        {
            if (inetKind == InetKind.V4)
            {
                return _v4Sv.ToString();
            }
            return _v6Sv.ToString();
        }

        public void SetMail(string user, string fileName)
        {
            //メールボックスへのデータセット
            _service.AddMail("DF_" + fileName, user);
            _service.AddMail("MF_" + fileName, user);

        }

        //DFファイルの一覧を取得する
        public string[] GetDf(string user)
        {
            var dir = Path.Combine(_service.MailboxPath, user);
            if (!Directory.Exists(dir))
                return new string[] { };
            var files = Directory.GetFiles(dir, "DF*");
            return files;
        }

        //メールの一覧を取得する
        public List<Mail> GetMf(string user)
        {
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

            _service.Dispose();

        }
    }
}
