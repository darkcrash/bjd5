using System;
using System.Collections.Generic;
using System.Text;

namespace Bjd.Configurations
{
    public class DatRecord : ValidObj, IDisposable
    {
        public bool Enable { get; private set; }
        public List<string> ColumnValueList { get; private set; }
        private readonly bool[] _isSecretList;

        private DatRecord()
        {
            // デフォルトコンストラクタの隠蔽
        }

        public DatRecord(bool enable, string[] columnList, bool[] isSecretList)
        {
            if (columnList == null)
            {
                throw new ValidObjException("引数に矛盾があります  columnList=null");
            }
            if (isSecretList == null)
            {
                throw new ValidObjException("引数に矛盾があります  isSecretList == null");
            }
            if (columnList.Length != isSecretList.Length)
            {
                throw new ValidObjException("引数に矛盾があります  columnList.length != isSecretList.length");
            }

            Enable = enable;
            _isSecretList = new bool[columnList.Length];
            ColumnValueList = new List<string>();
            for (int i = 0; i < columnList.Length; i++)
            {
                ColumnValueList.Add(columnList[i]);
                _isSecretList[i] = isSecretList[i];
            }
        }

        public string ToReg(bool isSecret)
        {
            var sb = new StringBuilder();
            if (!Enable)
            {
                sb.Append("#");
            }
            for (int i = 0; i < ColumnValueList.Count; i++)
            {
                sb.Append('\t');
                if (isSecret && _isSecretList[i])
                { // シークレットカラム
                    sb.Append("***");
                }
                else
                {
                    sb.Append(ColumnValueList[i]);
                }
            }
            return sb.ToString();
        }

        public bool FromReg(string str)
        {
            if (str == null)
            {
                return false;
            }
            string[] tmp = str.Split('\t');

            //カラム数確認
            if (tmp.Length != ColumnValueList.Count + 1)
            {
                return false;
            }

            //enableカラム
            switch (tmp[0])
            {
                case "":
                    Enable = true;
                    break;
                case "#":
                    Enable = false;
                    break;
                default:
                    return false;
            }
            //以降の文字列カラム
            ColumnValueList = new List<String>();
            for (var i = 1; i < tmp.Length; i++)
            {
                ColumnValueList.Add(tmp[i]);
            }
            return true;
        }

        public bool FromJson(string[] str)
        {
            if (str == null)
            {
                return false;
            }
            string[] tmp = str;

            //カラム数確認
            if (tmp.Length != ColumnValueList.Count + 1)
            {
                return false;
            }

            //enableカラム
            var enableString = tmp[0];
            bool enableBool;
            if (bool.TryParse(enableString, out enableBool))
            {
                Enable = enableBool;
            }
            else
            {
                return false;
            }


            //以降の文字列カラム
            ColumnValueList = new List<String>();
            for (var i = 1; i < tmp.Length; i++)
            {
                ColumnValueList.Add(tmp[i]);
            }
            return true;
        }

        protected override void Init()
        {
            ColumnValueList.Clear();
        }

        // toRegと誤って使用しないように注意
        public override string ToString()
        {
            return "ERROR";
        }

        public void Dispose()
        {
        }
    }
}
