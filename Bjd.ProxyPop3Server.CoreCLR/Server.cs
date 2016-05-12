using System.Collections.Generic;
using System.Text;
using System.Threading;
using Bjd;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Options;
using Bjd.Net.Sockets;
using Bjd.Utils;

namespace Bjd.ProxyPop3Server
{
    class Server : MailProxyServer
    {

        //コンストラクタ
        public Server(Kernel kernel, Conf conf, OneBind oneBind)
            : base(kernel, conf, oneBind)
        {

        }
        protected override string BeforeJob(SockTcp client, List<byte[]> clientBuf)
        {
            Protocol = MailProxyProtocol.Pop3;

            //挨拶文をサーバに変わって送出する
            client.AsciiSend("+OK ");

            //USER コマンドを受け付けるまでループ(最大５回)する
            for (var i = 0; i < 5; i++)
            {
                var buf = client.LineRecv(TimeoutSec, this);
                if (buf != null)
                {
                    var str = Inet.TrimCrlf(Encoding.ASCII.GetString(buf));
                    if (str.ToUpper().IndexOf("USER") == 0)
                    {
                        clientBuf.Add(buf);
                        var tmp = str.Split(' ');
                        if (tmp.Length >= 2)
                        {
                            return tmp[1];//ユーザ名
                        }
                    }
                    else if (str.ToUpper().IndexOf("QUIT") == 0)
                    {
                        return null;
                    }
                    else {
                        client.AsciiSend("-ERR ");
                    }
                }
                else {
                    Thread.Sleep(300);
                }
            }
            return null;
        }

        //RemoteServerでのみ使用される
        public override void Append(OneLog oneLog)
        {

        }

        protected override void CheckLang()
        {
        }
    }
}


