﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd;
using Bjd.Mailbox;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Threading;

namespace Bjd.SmtpServer
{
    class PopClient : LastError, IDisposable
    {
        private readonly int _port;
        private readonly Ip _ip;
        private readonly ILife _iLife;

        private readonly int _sec; //タイムアウト
        private ISocket _sockTcp;
        private Kernel _kernel;

        public PopClientStatus Status { get; private set; }

        public PopClient(Kernel kernel, Ip ip, int port, int sec, ILife iLife)
        {
            _kernel = kernel;
            _ip = ip;
            _port = port;
            _sec = sec;
            _iLife = iLife;

            Status = PopClientStatus.Idle;

        }

        //接続
        public bool Connect()
        {
            _kernel.Logger.TraceInformation("PopClient.Connect");
            if (Status != PopClientStatus.Idle)
            {
                SetLastError("Connect() Status != Idle");
                return false;
            }
            if (_ip.InetKind == InetKind.V4)
            {
                _sockTcp = Inet.Connect(_kernel, _ip, _port, _sec + 3, null);
            }
            else
            {
                _sockTcp = Inet.Connect(_kernel, _ip, _port, _sec + 3, null);
            }
            if (_sockTcp.SockState == SockState.Connect)
            {
                //+OK受信
                if (!RecvStatus()) return false;

                Status = PopClientStatus.Authorization;
                return true;
            }
            SetLastError("Faild in PopClient Connect()");
            return false;
        }

        //ログイン
        public bool Login(String user, String pass)
        {
            _kernel.Logger.TraceInformation("PopClient.Login");
            //切断中の場合はエラー
            if (Status != PopClientStatus.Authorization)
            {
                SetLastError("Login() Status != Authorization");
                return false;
            }
            //USER送信
            if (!SendCmd(String.Format("USER {0}", user))) return false;
         
            //+OK受信
            if (!RecvStatus()) return false;

            //PASS送信
            if (!SendCmd(String.Format("PASS {0}", pass))) return false;

            //+OK受信
            if (!RecvStatus()) return false;

            Status = PopClientStatus.Transaction;
            return true;
        }

        public bool Quit()
        {
            _kernel.Logger.TraceInformation("PopClient.Quit");
            //切断中の場合はエラー
            if (Status == PopClientStatus.Idle)
            {
                SetLastError("Quit() Status == PIdle");
                return false;
            }
            //QUIT送信
            if (!SendCmd("QUIT")) return false;

            //+OK受信
            if (!RecvStatus()) return false;

            //切断
            _sockTcp.Close();
            _sockTcp = null;
            Status = PopClientStatus.Idle;
            return true;
        }

        public bool Uidl(List<String> lines)
        {
            _kernel.Logger.TraceInformation("PopClient.Uidl");
            lines.Clear();

            //切断中の場合はエラー
            if (Status != PopClientStatus.Transaction)
            {
                SetLastError("Uidl() Status != Transaction");
                return false;
            }
            //QUIT送信
            if (!SendCmd("UIDL")) return false;
    
            //+OK受信
            if (!RecvStatus()) return false;

            //.までの行を受信
            var buf = RecvData();
            if (buf == null) return false;

            var s = Encoding.ASCII.GetString(buf);
            if (s.Length >= 5)
            {
                //<CR><LF>.<CR><LF>を削除
                lines.AddRange(Inet.GetLines(s.Substring(0, s.Length - 5)));
            }
            return true;
        }

        public bool Retr(int n, Mail mail)
        {
            _kernel.Logger.TraceInformation("PopClient.Retr() ");
            //切断中の場合はエラー
            if (Status != PopClientStatus.Transaction)
            {
                SetLastError("Retr() Status != Transaction");
                return false;
            }

            //RETR送信
            _kernel.Logger.TraceInformation("PopClient.Retr() RETR");
            if (!SendCmd($"RETR {n + 1}")) return false;

            //+OK受信
            _kernel.Logger.TraceInformation("PopClient.Retr() RecvStatus");
            if (!RecvStatus()) return false;

            //.までの行を受信
            _kernel.Logger.TraceInformation("PopClient.Retr() RecvData");
            var buf = RecvData();
            if (buf == null) return false;
           
            //var tmp = new byte[buf.Length-3];
            //Buffer.BlockCopy(buf,0,tmp,0,buf.Length-3);
            //mail.Init2(tmp);

            _kernel.Logger.TraceInformation($"PopClient.Retr {buf.Length} byte received");
            mail.Init2(new ArraySegment<byte>(buf, 0, buf.Length - 3));

            return true;

        }

        public bool Dele(int n)
        {
            _kernel.Logger.TraceInformation("PopClient.Dele");
            //切断中の場合はエラー
            if (Status != PopClientStatus.Transaction)
            {
                SetLastError("Dele() Status != Transaction");
                return false;
            }
            //DELE送信
            if (!SendCmd(string.Format("DELE {0}", n + 1)))
            {
                return false;
            }
            //+OK受信
            if (!RecvStatus())
            {
                return false;
            }
            return true;
        }


        //.行までを受信する
        byte[] RecvData()
        {
            _kernel.Logger.TraceInformation("PopClient.RecvData");
            var dt = DateTime.Now.AddSeconds(_sec);
            //var line = new byte[0];
            var lines = new List<byte[]>();

            while (_iLife.IsLife())
            {
                var now = DateTime.Now;
                if (dt < now)
                {
                    return null; //タイムアウト
                }
                //var len = _sockTcp.Length();
                //if (len == 0)
                //{
                //    continue;
                //}
                //var buf = _sockTcp.LineRecv(len, _sec, _iLife);
                var line = _sockTcp.LineRecv(_sec, _iLife);
                if (line == null)
                {
                    return null; //切断された
                }

                dt = now.AddSeconds(_sec);
                lines.Add(line);
                //var tmp = new byte[line.Length + buf.Length];
                //Buffer.BlockCopy(line, 0, tmp, 0, line.Length);
                //Buffer.BlockCopy(buf, 0, tmp, line.Length, buf.Length);
                //line = tmp;
                if (line.Length == 3)
                {
                    if (line[line.Length - 1] == '\n' && line[line.Length - 2] == '\r' && line[line.Length - 3] == '.')
                    {
                        var result = new byte[lines.Sum(_ => _.Length)];
                        var pos = 0;
                        lines.ForEach(_ => { Buffer.BlockCopy(_, 0, result, pos, _.Length); pos += _.Length; });
                        return result;
                    }
                }
            }
            return null;
        }



        bool SendCmd(string cmdStr)
        {
            _kernel.Logger.TraceInformation($"PopClient.SendCmd {cmdStr}");
            //AsciiSendは、内部でCRLFを追加する
            if (cmdStr.Length + 2 != _sockTcp.AsciiSend(cmdStr))
            {
                SetLastError(String.Format("Faild in PopClient SendCmd({0})", cmdStr));
                ConfirmConnect();//接続確認
                return false;
            }
            return true;
        }

        bool RecvStatus()
        {
            _kernel.Logger.TraceInformation("PopClient.RecvStatus");

            var buf = _sockTcp.LineRecv(_sec, _iLife);
            if (buf == null)
            {
                SetLastError("Timeout in PopClient RecvStatus()");
                ConfirmConnect();//接続確認
                return false;
            }
            var str = Encoding.ASCII.GetString(buf);
            if (str.ToUpper().IndexOf("+OK") == 0) return true;

            SetLastError($"Not Found +OK in PopClient RecvStatus()");
            ConfirmConnect();//接続確認
            return false;
        }
        //接続確認
        void ConfirmConnect()
        {
            //既に切断されている場合
            if (_sockTcp.SockState != SockState.Connect)
            {
                Status = PopClientStatus.Idle;
            }
        }

        public void Dispose()
        {
            if (_sockTcp != null)
            {
                _sockTcp.Close();
                _sockTcp = null;
            }
        }

    }
}
