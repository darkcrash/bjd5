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
    // 認証リスト
    /***********************************************************/
    class OneAuth
    {
        readonly List<string> _requireList = new List<string>();//ユーザ名とグループ名のリスト
        public OneAuth(string uri, string authName, string requires)
        {

            Uri = uri;
            AuthName = authName;

            foreach (string require in requires.Split(';'))
            {
                _requireList.Add(require);
            }
        }
        public string Uri { get; private set; }
        public string AuthName { get; private set; }
        //ユーザ。グループのリストにヒットが有るかどうかの検索
        public bool Seartch(string user)
        {
            if (_requireList.IndexOf(user) != -1)
                return true;
            return false;
        }
    }
  }
