using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Bjd;
using Bjd.Logs;
using Bjd.Configurations;
using Bjd.Utils;

namespace Bjd.WebServer.Authority
{

    /***********************************************************/
    // グループリスト
    /***********************************************************/
    class OneGroup
    {
        readonly List<string> _userList = new List<string>();
        public OneGroup(string group, string users)
        {
            Group = group;
            foreach (var user in users.Split(';'))
            {
                _userList.Add(user);
            }
        }
        public string Group { get; private set; }

        //ユーザリストにヒットが有るかどうかの検索
        public bool Seartch(string user)
        {
            return _userList.IndexOf(user) != -1;
        }
    }

   
}
