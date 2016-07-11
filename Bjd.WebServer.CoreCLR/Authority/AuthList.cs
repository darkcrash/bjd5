using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Bjd;
using Bjd.Logs;
using Bjd.Options;
using Bjd.Utils;

namespace Bjd.WebServer.Authority
{

    class AuthList
    {

        readonly List<OneAuth> _ar = new List<OneAuth>();

        public AuthList(Dat authList)
        {
            foreach (var o in authList)
            {
                if (!o.Enable)
                    continue;
                string uri = o.ColumnValueList[0];
                string authName = o.ColumnValueList[1];
                string requires = o.ColumnValueList[2];
                _ar.Add(new OneAuth(uri, authName, requires));
            }
        }

        //認証リストにヒットが有るかどうかの検索
        public OneAuth Search(string uri)
        {
            var sUri = uri.ToLower();
            foreach (OneAuth oneAuth in _ar)
            {

                //Ver5.5.7
                //if (uri.IndexOf(oneAuth.Uri)==0)
                //    return oneAuth;
                var sUri2 = oneAuth.Uri.ToLower();
                if (sUri.IndexOf(sUri2) == 0)
                    return oneAuth;
                //Ver5.5.7
                //Ver5.0.6
                //if (uri.Length > 1 && uri[uri.Length - 1] != '/') {
                //    uri = uri + '/';
                //    if (uri.IndexOf(oneAuth.Uri) == 0)
                //        return oneAuth;
                //}
                //Ver5.0.6
                if (sUri.Length > 1 && sUri[sUri.Length - 1] != '/')
                {
                    sUri = sUri + '/';
                    if (sUri.IndexOf(sUri2) == 0)
                        return oneAuth;
                }
            }
            return null;
        }

    }

}
