﻿using System;
using System.Collections.Generic;
using Bjd.Utils;
using Bjd.Controls;

namespace Bjd.Configurations
{
    public class Conf
    {
        //Optionクラスへ結合を排除するためのクラス<br>
        //Optionの値を個別に設定できる（テスト用）<br>

        private readonly Dictionary<string, object> _ar = new Dictionary<string, object>();
        public string NameTag { get; private set; }

        //テスト用コンストラクタ
        public Conf()
        {
            NameTag = "";
        }

        public Conf(ConfigurationBase oneOption)
        {

            NameTag = oneOption.NameTag;

            var list = oneOption.ListVal.GetList(null);
            foreach (var o in list)
            {
                _ar.Add(o.Name, o.Value);
            }
        }

        //値の取得
        //存在しないタグを指定すると実行事例がが発生する
        public object Get(string name)
        {
            if (!_ar.ContainsKey(name))
            {
                //HashMapの存在確認
                Util.RuntimeException(string.Format("未定義 {0}", name));
            }
            return _ar[name];
        }

        //値の設定
        //存在しないタグを指定すると実行事例がが発生する
        public void Set(string name, object value)
        {
            if (!_ar.ContainsKey(name))
            {
                //HashMapの存在確認
                Util.RuntimeException(string.Format("未定義 {0}", name));
            }
            _ar[name] = value;
        }

        //値の設定
        //存在しないタグを指定できる（テスト用）
        public void Add(string name, object value)
        {
            if (!_ar.ContainsKey(name))
            {
                _ar.Add(name, value);
            }
            else
            {
                _ar[name] = value;
            }
        }

        public void Save(ConfigurationDb iniDb)
        {
            throw new NotImplementedException();
        }
    }
}

