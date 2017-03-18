using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Components;
using Bjd.Utils;

namespace Bjd.Mailbox
{

    public class MailBox : ComponentBase
    {
        private readonly List<OneMailBox> _ar = new List<OneMailBox>();
        private Logger _log;

        public string Dir { get; private set; } //メールボックスのフォルダ
        public bool Status { get; private set; } //初期化成否の確認
        //ユーザ一覧
        public List<string> UserList
        {
            get
            {
                return _ar.Select(o => o.User).ToList();
            }
        }

        public MailBox(Kernel kernel, Conf conf)
            : base(kernel, conf)
        {
            //SmtpServer若しくは、Pop3Serverが使用される場合のみメールボックスを初期化する                
            //var op = kernel.ListOption.Get("MailBox");
            //var conf = new Conf(op);
            var confDir = kernel.ReplaceOptionEnv((string)conf.Get("dir"));
            var datUser = (Dat)conf.Get("user");

            Status = true; //初期化状態 falseの場合は、初期化に失敗しているので使用できない

            //_log = logger;
            _log = kernel.CreateLogger("MailBox", (bool)conf.Get("useDetailsLog"), null);

            //MailBoxを配置するフォルダ
            //Dir = dir;
            Dir = Path.Combine(kernel.Enviroment.ExecutableDirectory, confDir);
            try
            {
                Directory.CreateDirectory(Dir);
            }
            catch (Exception)
            {

            }

            if (!Directory.Exists(Dir))
            {
                _log.Set(LogKind.Error, null, 9000029, string.Format("dir="));
                Status = false;
                Dir = null;
                return; //以降の初期化を処理しない
            }
            //ユーザリストの初期化
            Init(datUser);
        }

        //ユーザリストの初期化
        private void Init(IEnumerable<DatRecord> datUser)
        {
            _ar.Clear();
            if (datUser != null)
            {
                foreach (var o in datUser)
                {
                    if (!o.Enable)
                        continue; //有効なデータだけを対象にする
                    var name = o.ColumnValueList[0];
                    var pass = Crypt.Decrypt(o.ColumnValueList[1]);
                    _ar.Add(new OneMailBox(name, pass));
                    //var folder = string.Format("{0}\\{1}", Dir, name);
                    var folder = Path.Combine(Dir, name);
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                }
            }
        }

        protected string CreateFileName()
        {
            lock (this)
            {
                while (true)
                {
                    var str = string.Format("{0:D20}", DateTime.Now.Ticks);
                    //スレッドセーフの確保(ウエイトでDateTIme.Nowの重複を避ける)
                    Thread.Sleep(1);
                    //var fileName = string.Format("{0}\\MF_{1}", Dir, str);
                    var fileName = $"{Dir}{Path.DirectorySeparatorChar}MF_{str}";
                    if (!File.Exists(fileName))
                    {
                        return str;
                    }
                }
            }
        }

        public bool Save(string user, Mail mail, MailInfo mailInfo)
        {
            //Ver_Ml
            if (!IsUser(user))
            {
                _log.Set(LogKind.Error, null, 9000047, string.Format("[{0}] {1}", user, mailInfo));
                return false;
            }

            //フォルダ作成
            //var folder = string.Format("{0}\\{1}", Dir, user);
            var folder = $"{Dir}{Path.DirectorySeparatorChar}{user}";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            //ファイル名生成
            var name = CreateFileName();
            //var mfName = string.Format("{0}\\MF_{1}", folder, name);
            //var dfName = string.Format("{0}\\DF_{1}", folder, name);
            var mfName = $"{folder}{Path.DirectorySeparatorChar}MF_{name}";
            var dfName = $"{folder}{Path.DirectorySeparatorChar}DF_{name}";

            //ファイル保存
            var success = false;
            try
            {
                if (mail.Save(mfName))
                {
                    if (mailInfo.Save(dfName))
                    {
                        success = true;
                    }
                }
                else
                {
                    _log.Set(LogKind.Error, null, 9000059, mail.GetLastError());
                }
            }
            catch (Exception)
            {
                ;
            }
            //失敗した場合は、作成途中のファイルを全部削除
            if (!success)
            {
                if (File.Exists(mfName))
                {
                    File.Delete(mfName);
                }
                if (File.Exists(dfName))
                {
                    File.Delete(dfName);
                }
                return false;
            }
            //_logger.Set(LogKind.Normal, null, 8, mailInfo.ToString());

            return true;
        }

        //ユーザが存在するかどうか
        public bool IsUser(string user)
        {
            return _ar.Any(o => o.User == user);
        }

        //最後にログインに成功した時刻の取得 (PopBeforeSMTP用）
        public DateTime LastLogin(Ip addr)
        {
            foreach (var oneMailBox in _ar.Where(oneMailBox => oneMailBox.Addr == addr.ToString()))
            {
                return oneMailBox.Dt;
            }
            return new DateTime(0);
        }

        //認証（パスワード確認) ※パスワードの無いユーザが存在する?
        public bool Auth(string user, string pass)
        {
            foreach (var o in _ar)
            {
                if (o.User == user)
                {
                    return o.Pass == pass;
                }
            }
            return false;
        }

        //パスワード取得
        public string GetPass(string user)
        {
            foreach (var oneUser in _ar)
            {
                if (oneUser.User == user)
                {
                    return oneUser.Pass;
                }
            }
            return null;
        }
        //パスワード変更 pop3Server.Chpsから使用される
        public bool SetPass(string user, string pass)
        {
            foreach (var oneUser in _ar)
            {
                if (oneUser.User == user)
                {
                    oneUser.SetPass(pass);
                    return true;
                }
            }
            return false;
        }


        public bool Login(string user, Ip addr)
        {
            foreach (var oneUser in _ar)
            {
                if (oneUser.User != user)
                    continue;
                if (oneUser.Login(addr.ToString()))
                {
                    return true;
                }
            }
            return false;
        }


        public void Logout(string user)
        {
            foreach (var oneUser in _ar)
            {
                if (oneUser.User == user)
                {
                    oneUser.Logout();
                    return;
                }
            }
        }
    }
}
