using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Bjd;
using Bjd.Logs;
using Bjd.Options;
using Bjd.Utils;

namespace Bjd.WebServer.Authority
{

    /***********************************************************/
    // ユーザリスト
    /***********************************************************/
    class OneUser
    {
        public OneUser(string user, string pass)
        {
            User = user;
            Pass = pass;
        }
        public string User { get; private set; }
        public string Pass { get; private set; }
    }
}
