using System.IO;
using System.Linq;
using Bjd;
using Bjd.Options;
using System.Collections.Generic;

namespace Bjd.WebServer
{
    public class HttpContentType
    {


        //readonly OneOption _oneOption;
        readonly Conf _conf;
        Dictionary<string, string> _mimeDic = new Dictionary<string, string>();

        public HttpContentType(Conf conf)
        {
            //_oneOption = oneOption;
            _conf = conf;
            var mimeList = (Dat)_conf.Get("mime");
            foreach (var o in mimeList)
            {
                var key = o.ColumnValueList[0].ToUpper();
                var value = o.ColumnValueList[1];
                _mimeDic.Add(key, value);
            }

        }
        // 拡張子から、Mimeタイプを取得する sendPath()で使用される
        public string Get(string fileName)
        {
            var ext = Path.GetExtension(fileName);

            //パラメータにドットから始まる拡張子が送られた場合、内部でドット無しに修正する
            if (ext != null)
            {
                if (ext.Length > 0 && ext[0] == '.')
                    ext = ext.Substring(1);
            }

            var extUpper = ext.ToUpper();

            //mimeListからextの情報を検索する
            string mimeType = null;
            if (ext != null)
            {
                //foreach (var o in _mimeList)
                //{
                //    if (o.ColumnValueList[0].ToUpper() != ext.ToUpper())
                //        continue;
                //    mimeType = o.ColumnValueList[1];
                //    break;
                //}
                if (_mimeDic.ContainsKey(extUpper))
                {
                    mimeType = _mimeDic[extUpper];
                }
            }

            if (mimeType == null)
            {
                //拡張子でヒットしない場合は、「.」での設定を検索する
                //DOTO  Dat2.Valの実装をやめたのので動作確認が必要
                //mimeType = (string)mimeList.Val(0,".",1);
                //mimeType = null;
                //foreach (var o in _mimeList.Where(o => o.ColumnValueList[0] == "."))
                //{
                //    mimeType = o.ColumnValueList[1];
                //    break;
                //}
                if (_mimeDic.ContainsKey("."))
                {
                    mimeType = _mimeDic["."];
                }
                if (mimeType == null)
                    mimeType = "application/octet-stream";//なにもヒットしなかった場合
            }
            return mimeType;
        }
    }

}
