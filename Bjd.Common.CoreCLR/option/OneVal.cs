using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using Bjd.ctrl;
using Bjd.net;
using Bjd.util;

namespace Bjd.option
{
    //1つの値を表現するクラス<br>
    //ListValと共に再帰処理が可能になっている<br>
    public class OneVal : IDisposable
    {

        public String Name { get; private set; }
        public Object Value { get; private set; }
        public Crlf Crlf { get; private set; }

        public Type ValueType { get; private set; }

        public bool IsSecret { get; private set; }

        public OneVal(String name, Object value, Crlf crlf) : this(name, value, crlf, false)
        {
        }

        public OneVal(String name, Object value, Crlf crlf, bool isSecret)
        {
            Name = name;
            Value = value;
            Crlf = crlf;
            IsSecret = isSecret;
            if (value != null)
                this.ValueType = value.GetType();


            //*************************************************************
            //仕様上、階層構造をなすOneValの名前は、ユニークである必要がる
            //プログラム作成時に重複を発見できるように、重複があった場合、ここでエラーをポップアップする
            //*************************************************************

            //名前一覧
            var tmp = new List<String>();

            //このlistの中に重複が無いかどうかをまず確認する
            List<OneVal> list = GetList(null);
            foreach (OneVal o in list)
            {
                if (0 <= tmp.IndexOf(o.Name))
                {
                    //名前一覧に重複は無いか
                    Console.WriteLine(String.Format("OneVal(OnePage)の名前に重複があります {0}", o.Name));
                    //Msg.Show(MsgKind.Error, String.Format("OneVal(OnePage)の名前に重複があります {0}", o.Name));
                }
                tmp.Add(o.Name); //名前一覧への蓄積
            }

        }

        //Ver6.0.0
        public void SetValue(object value)
        {
            Value = value;

        }

        // 階層下のOneValを一覧する
        public List<OneVal> GetList(List<OneVal> list)
        {
            if (list == null)
            {
                list = new List<OneVal>();
            }

            list.Add(this);
            return list;
        }

        public List<OneVal> GetSaveList(List<OneVal> list)
        {
            if (list == null)
            {
                list = new List<OneVal>();
            }
            list.Add(this);
            return list;
        }


        public void Dispose() { }


        //設定ファイル(Option.ini)への出力
        //isSecret=true デバッグ用の設定ファイル出力用（パスワード等を***で表現する）
        public String ToReg(bool isSecret)
        {
            return "";
        }

        //出力ファイル(Option.ini)からの入力用<br>
        //不正な文字列があった場合は、無効行として無視される<br>
        public bool FromReg(String str)
        {
            if (str == null)
            {
                Value = null;
                return false;
            }
            try
            {

                if (this.ValueType == typeof(Dat))
                {
                    ((Dat)this.Value).FromReg(str);
                }
                else if (this.ValueType.GetTypeInfo().IsEnum)
                {
                    this.Value = Enum.Parse(this.ValueType, str);
                }
                else if (typeof(ValidObj).IsAssignableFrom(this.ValueType))
                {
                    ((ValidObj)this.Value).FromString(str);
                }
                else
                {
                    Value = Convert.ChangeType(str, this.ValueType);
                }
            }
            catch (Exception)
            {
                throw;
            }
            System.Diagnostics.Trace.TraceInformation($"{this.Name}={this.Value}");
            return true;
        }


    }


}

