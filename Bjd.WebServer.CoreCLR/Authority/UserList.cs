using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Bjd;
using Bjd.Logs;
using Bjd.Configurations;
using Bjd.Utils;

namespace Bjd.WebServer.Authority
{
    class UserList
    {

        readonly List<OneUser> _ar = new List<OneUser>();

        public UserList(Dat userList)
        {
            foreach (var o in userList)
            {
                if (!o.Enable)
                    continue;
                var user = o.ColumnValueList[0];
                var pass = Crypt.Decrypt(o.ColumnValueList[1]);
                _ar.Add(new OneUser(user, pass));
            }
        }
        //ユーザリストにヒットが有るかどうかの検索
        public OneUser Search(string user)
        {
            return _ar.FirstOrDefault(o => o.User == user);
        }
    }
}
