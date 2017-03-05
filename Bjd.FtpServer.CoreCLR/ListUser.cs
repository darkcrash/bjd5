using System;
using System.Collections.Generic;
using Bjd.Configurations;
using Bjd.Utils;

namespace Bjd.FtpServer
{
    class ListUser : ListBase<OneUser>
    {
        public ListUser(Kernel kernel, IEnumerable<DatRecord> dat)
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
                            var ftpAcl = (FtpAcl)Convert.ToInt32(o.ColumnValueList[0]);
                            var homeDir = o.ColumnValueList[1];
                            var userName = o.ColumnValueList[2];
                            try
                            {
                                var password = Crypt.Decrypt(o.ColumnValueList[3]);
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
