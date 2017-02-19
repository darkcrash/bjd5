using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Bjd;
using Bjd.Logs;
using Bjd.Options;
using Bjd.Utils;

namespace Bjd.WebServer.Authority
{
    class Authorization
    {
        //readonly OneOption _oneOption;
        readonly Kernel _kernel;
        readonly Conf _conf;
        readonly Logger _logger;
        private AuthList _authList;
        private GroupList _groupList;
        private UserList _userList;

        public Authorization(Kernel kernel, Conf conf, Logger logger)
        {
            _kernel = kernel;
            _conf = conf;
            _logger = logger;
            _kernel.Logger.TraceInformation($"Authorization..ctor");
            //_oneOption = oneOption;
            //認証リスト
            _authList = new AuthList((Dat)_conf.Get("authList"));
            _groupList = new GroupList((Dat)_conf.Get("groupList"));
            _userList = new UserList((Dat)_conf.Get("userList"));

        }

        //送信されてきた認証情報（ユーザ＋パスワード）の取得
        bool CheckHeader(string authorization, ref string user, ref string pass)
        {

            if (authorization == null)
            {
                return false;
            }
            int index = authorization.IndexOf(' ');
            if (0 <= index)
            {

                var str = authorization.Substring(index + 1);
                //Ver5.0.0-b13
                //byte[] bytes = Convert.FromBase64String(str);
                //string s = Encoding.ASCII.GetString(bytes);
                var s = Base64.Decode(str);

                index = s.IndexOf(':');
                if (0 <= index)
                {
                    user = s.Substring(0, index);
                    pass = s.Substring(index + 1);
                    return true;
                }
            }
            return false;
        }

        public bool Check(string uri, string authorization, ref string authName)
        {
            _kernel.Logger.TraceInformation($"Authorization.Check {uri}");
            //認証リスト
            //var authList = new AuthList((Dat)_conf.Get("authList"));
            var authList = _authList;

            //認証リストにヒットしているかどうかの確認
            var oneAuth = authList.Search(uri);
            if (oneAuth == null)
                return true;//認証リストにヒットなし

            //送信されてきた認証情報（ユーザ＋パスワード）の取得
            var user = "";
            var pass = "";
            if (!CheckHeader(authorization, ref user, ref pass))
                goto err;

            //認証リスト（AuthList）に当該ユーザの定義が存在するかどうか
            if (!oneAuth.Seartch(user))
            {
                var find = false;//グループリストからユーザが検索できるかどうか
                //認証リストで直接ユーザ名を見つけられなかった場合、グループリストを検索する
                //グループリスト
                //var groupList = new GroupList((Dat)_conf.Get("groupList"));
                var groupList = _groupList;
                foreach (OneGroup o in groupList)
                {
                    if (!oneAuth.Seartch(o.Group))
                        continue;
                    if (!o.Seartch(user))
                        continue;
                    find = true;//一応ユーザとして認められている
                    break;
                }
                if (!find)
                {
                    _logger.Set(LogKind.Secure, null, 6, string.Format("user:{0} pass:{1}", user, pass));//�F�؃G���[�i�F�؃��X�g�ɒ�`����Ă��Ȃ����[�U����̃A�N�Z�X�ł��j";
                    goto err;
                }
            }
            //パスワードの確認
            //var userList = new UserList((Dat)_conf.Get("userList"));
            var userList = _userList;
            var oneUser = userList.Search(user);
            if (oneUser == null)
            {
                //ユーザリストに情報が存在しない
                _logger.Set(LogKind.Secure, null, 7, string.Format("user:{0} pass:{1}", user, pass));//�F�؃G���[�i���[�U���X�g�ɓ��Y���[�U�̏�񂪂���܂���j";
            }
            else
            {
                if (oneUser.Pass == pass)
                {//パスワード一致
                    _logger.Set(LogKind.Detail, null, 8, string.Format("Authrization success user:{0} pass:{1}", user, pass));//�F�ؐ���
                    return true;
                }
                //パスワード不一致
                _logger.Set(LogKind.Secure, null, 9, string.Format("user:{0} pass:{1}", user, pass));//�F�؃G���[�i�p�X���[�h���Ⴂ�܂��j";
            }
            err:
            authName = oneAuth.AuthName;
            return false;//認証エラー発生
        }

    }
}
