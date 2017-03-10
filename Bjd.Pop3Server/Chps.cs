using Bjd.Mails;
using Bjd.Configurations;
using Bjd.Utils;

namespace Bjd.Pop3Server
{
    internal class Chps
    {
        //パスワード変更
        public static bool Change(string user, string pass, MailBox mailBox, Conf conf)
        {
            if (pass == null)
            {
                //無効なパスワードの指定は失敗する
                return false;
            }
            var dat = (Dat)conf.Get("user");
            foreach (var o in dat)
            {
                if (o.ColumnValueList[0] == user)
                {
                    o.ColumnValueList[1] = Crypt.Encrypt(pass);
                    conf.Set("user", dat); //データ変更
                    if (mailBox.SetPass(user, pass))
                    {
                        if (mailBox.Auth(user, pass))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            return false;
        }
    }
}
