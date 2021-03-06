using System.IO;
using Bjd.Mailbox;
using Bjd.Net.Sockets;

namespace Bjd.Pop3Server
{
    //***********************************************************************
    //メールボックスのメールをやり取りする情報を表現する
    //***********************************************************************
    internal class OneMessage
    {
        public OneMessage(string dir, string fname, string uid, long size)
        {
            _dir = dir;
            _fname = fname;
            Uid = uid;
            Size = size;
            Del = false;
        }

        readonly string _dir;
        readonly string _fname;

        //****************************************************************
        //プロパティ
        //****************************************************************
        public string Uid { get; private set; }
        public long Size { get; private set; }
        public bool Del { get; set; }

        public bool DeleteFile()
        {
            //string fileName = string.Format("{0}\\DF_{1}", _dir, _fname);
            string fileName = $"{_dir}{Path.DirectorySeparatorChar}DF_{_fname}";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                //fileName = string.Format("{0}\\MF_{1}", _dir, _fname);
                fileName = $"{_dir}{Path.DirectorySeparatorChar}MF_{_fname}";
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    return true;
                }
            }
            return false;
        }

        //メールの送信 count=本文の行数（-1の場合は全部）
        public bool Send(Kernel kernel, ISocket sockTcp, int count)
        {
            //string fileName = string.Format("{0}\\MF_{1}", _dir, _fname);
            string fileName = $"{_dir}{Path.DirectorySeparatorChar}MF_{_fname}";
            var mail = new Mail(kernel);
            mail.Read(fileName);
            if (!mail.Send(sockTcp, count))
            {
                // Logger.Set(LogKind.Error, null, 9000058, ex.Message);
                //mail.GetLastEror()を未処理
                return false;
            }
            return true;
        }

        public MailInfo GetMailInfo()
        {
            //string fileName = string.Format("{0}\\DF_{1}", _dir, _fname);
            string fileName = $"{_dir}{Path.DirectorySeparatorChar}DF_{_fname}";
            return new MailInfo(fileName);
        }
    }
}