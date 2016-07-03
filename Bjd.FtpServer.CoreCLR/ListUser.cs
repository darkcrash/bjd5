using System;
using System.Collections.Generic;
using Bjd.Options;
using Bjd.Utils;

namespace Bjd.FtpServer
{
    class ListUser : ListBase<OneUser>
    {
        public ListUser(Kernel kernel, IEnumerable<OneDat> dat)
        {
            if (dat != null)
            {
                foreach (var o in dat)
                {
                    //有効なデータだけを対象にする
                    if (o.Enable)
                    {
                        try
                        {
                            var ftpAcl = (FtpAcl)Convert.ToInt32(o.StrList[0]);
                            var homeDir = o.StrList[1];
                            var userName = o.StrList[2];
                            try
                            {
                                var password = Crypt.Decrypt(o.StrList[3]);
                                Ar.Add(new OneUser(kernel, ftpAcl, userName, password, homeDir));
                            }
                            catch (Exception e)
                            {
                                Util.RuntimeException(e.Message);
                            }
                        }
                        catch (Exception e)
                        {
                            Util.RuntimeException(e.Message);
                        }
                    }
                }
            }
        }

        public OneUser Get(string userName)
        {
            foreach (var o in Ar)
            {
                //Anonymousの場合、大文字小文字を区別しない
                if (userName.ToUpper() == "ANONYMOUS")
                {
                    if (o.UserName.ToUpper() == userName.ToUpper())
                    {
                        return o;
                    }
                }
                else {
                    if (o.UserName == userName)
                    {
                        return o;
                    }
                }
            }
            return null;
        }
    }
}
