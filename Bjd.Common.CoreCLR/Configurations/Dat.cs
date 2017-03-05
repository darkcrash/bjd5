using System;
using System.Text;
using System.Linq;
using Bjd.Utils;
using Bjd.Controls;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bjd.Configurations
{
    public class Dat : ListBase<DatRecord>
    {

        private readonly bool[] _isSecretList;
        private readonly int _columnMax;
        private readonly ListVal _columnList;

        public Dat(CtrlType[] ctrlTypeList)
        {
            //カラム数の初期化
            _columnMax = ctrlTypeList.Length;

            //isSecretListの生成
            _isSecretList = new bool[_columnMax];

            for (int i = 0; i < _columnMax; i++)
            {
                _isSecretList[i] = (ctrlTypeList[i] == CtrlType.Hidden);
            }
        }
        public Dat(ListVal val)
        {
            //カラム数の初期化
            _columnMax = val.Count;

            _columnList = val;

            //isSecretListの生成
            _isSecretList = new bool[_columnMax];

            for (int i = 0; i < _columnMax; i++)
            {
                _isSecretList[i] = _columnList[i].IsSecret;
            }

        }

        //文字列によるOneDatの追加
        //内部で、OneDatの型がチェックされる
        public bool Add(bool enable, string str)
        {
            if (str == null)
            {
                return false; //引数にnullが渡されました
            }
            var list = str.Split('\t');
            if (list.Length != _columnMax)
            {
                return false; //カラム数が一致しません
            }
            DatRecord oneDat;
            try
            {
                oneDat = new DatRecord(enable, list, _isSecretList);
            }
            catch (ValidObjException)
            {
                return false; // 初期化文字列が不正
            }
            Ar.Add(oneDat);
            return true;
        }

        public void AddList(List<OneVal> list)
        {
            _columnList.GetList(list);
        }

        public List<OneVal> GetList()
        {
            var list = new List<OneVal>();
            return _columnList.GetList(list);
        }

        //文字列化
        //isSecret 秘匿が必要なカラムを***に変換して出力する
        public String ToReg(bool isSecret)
        {
            var sb = new StringBuilder();
            foreach (var o in Ar)
            {
                if (sb.Length != 0)
                {
                    sb.Append("\b");
                }
                sb.Append(o.ToReg(isSecret));
            }
            return sb.ToString();
        }

        internal List<DatRecord> GetOneDatList()
        {
            return Ar;
        }

        //文字列による初期化
        public bool FromReg(String str)
        {
            Ar.Clear();
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            //Ver5.7.x以前のiniファイルをVer5.8用に修正する
            var tmp = Util.ConvValStr(str);
            str = tmp;

            // 各行処理
            String[] lines = str.Split('\b');
            if (lines.Length <= 0)
            {
                return false; //"lines.length <= 0"
            }

            foreach (var l in lines)
            {
                var s = l;
                //OneDatの生成
                DatRecord oneDat;
                try
                {
                    oneDat = new DatRecord(true, new String[_columnMax], _isSecretList);
                }
                catch (ValidObjException)
                {
                    return false;
                }

                if (s.Split('\t').Length != _isSecretList.Length + 1)
                {
                    // +1はenableカラムの分
                    //カラム数の不一致
                    return false;
                }

                //fromRegによる初期化
                if (oneDat.FromReg(s))
                {
                    Ar.Add(oneDat);
                    continue; // 処理成功
                }
                //処理失敗
                Ar.Clear();
                return false;
            }
            return true;
        }

        public bool FromJson(JArray array)
        {
            Ar.Clear();
            if (array == null || !array.HasValues)
            {
                return false;
            }

            // 各行処理
            if (array.Count <= 0)
            {
                return false; //"lines.length <= 0"
            }

            var children = array.Children<JObject>();
            foreach (var l in children)
            {
                var properties = l.Children<JProperty>();
                var count = properties.Count();


                //OneDatの生成
                DatRecord oneDat;
                try
                {
                    oneDat = new DatRecord(true, new String[_columnMax], _isSecretList);
                }
                catch (ValidObjException)
                {
                    return false;
                }

                if (count != _isSecretList.Length + 1)
                {
                    // +1はenableカラムの分
                    //カラム数の不一致
                    return false;
                }

                var enableProp = properties.Where(_ => _.Name == "enable").FirstOrDefault();
                var enable = Enumerable.Repeat(enableProp.Value.Value<string>(), 1).Select(_ => _);
                var keyValue = _columnList.Select(_ => properties.Where(__ => __.Name == _.Name).FirstOrDefault().Value.Value<string>());
                var values = enable.Concat(keyValue).ToArray();

                //fromRegによる初期化
                if (oneDat.FromJson(values))
                {
                    Ar.Add(oneDat);
                    continue; // 処理成功
                }
                //処理失敗
                Ar.Clear();
                return false;
            }
            return true;
        }

    }
}
