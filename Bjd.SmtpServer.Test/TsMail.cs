using System.Text;
using Bjd;
using Bjd.Mailbox;
using Bjd.Net;
using Bjd.SmtpServer;
using Bjd.Initialization;
using System;

namespace Bjd.SmtpServer.Test
{
    //******************************************************************
    // テスト用メールオブジェクト
    //******************************************************************
    public class TsMail
    {
        TestService _service;
        Kernel _kernel;
        public Mail Mail { get; private set; }
        internal MlEnvelope MlEnvelope { get; private set; }
        public TsMail(TestService service, string from, string to, string bodyStr)
        {
            _service = service;
            _kernel = _service.Kernel;

            Mail = new Mail(_kernel);
            Mail.AppendLine(Encoding.ASCII.GetBytes("\r\n"));//区切り行(ヘッダ終了)
            var body = Encoding.ASCII.GetBytes(bodyStr);
            Mail.AppendLine(body);
            Mail.AddHeader("from", from);
            Mail.AddHeader("to", to);

            const string host = "TEST";
            var addr = new Ip("10.0.0.1");
            MlEnvelope = new MlEnvelope(CreateMailAddress(from), CreateMailAddress(to), host, addr);
        }

        MailAddress CreateMailAddress(string str)
        {
            var addr = str;
            var s0 = str.IndexOf("<");
            if (s0 != -1)
            {
                var tmp = str.Substring(s0 + 1);
                var s1 = tmp.IndexOf(">");
                if (s1 != -1)
                {
                    addr = tmp.Substring(0, s1);
                }
            }
            return new MailAddress(addr);
        }
    }
}
