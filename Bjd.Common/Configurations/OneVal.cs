﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Utils;
using Newtonsoft.Json.Linq;

namespace Bjd.Configurations
{
    //1つの値を表現するクラス<br>
    //ListValと共に再帰処理が可能になっている<br>
    public class OneVal : IDisposable
    {
        private Kernel _kernel;

        public string Name { get; private set; }
        public object Value { get; private set; }
        public Crlf Crlf { get; private set; }

        public CtrlType CtrlType { get; private set; }

        public Type ValueType { get; private set; }

        public bool IsSecret { get; private set; }

        public OneVal(Kernel kernel, CtrlType type, String name, Object value, Crlf crlf) : this(kernel, type, name, value, crlf, false)
        {
            _kernel = kernel;
        }

        public OneVal(Kernel kernel, CtrlType type, String name, Object value, Crlf crlf, bool isSecret)
        {
            _kernel = kernel;

            this.CtrlType = type;
            this.Name = name;
            this.Value = value;
            this.Crlf = crlf;
            this.IsSecret = isSecret;
            if (value != null)
                this.ValueType = value.GetType();
            if (CtrlType == CtrlType.ComboBox)
            {
                this.ValueType = value.GetType();
            }


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
            if (this.ValueType == typeof(Dat) && (this.Value as Dat != null))
            {
                ((Dat)this.Value).AddList(list);
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
            switch (this.CtrlType)
            {
                case CtrlType.CheckBox:
                    return ((bool)Value).ToString().ToLower();
                case CtrlType.TextBox:
                    return (string)Value;
                case CtrlType.Hidden:
                    if (isSecret)
                    {
                        return "***";
                    }
                    try
                    {
                        return Crypt.Encrypt((String)Value);
                    }
                    catch (Exception)
                    {
                        return "ERROR";
                    }
                case CtrlType.Memo:
                    return Util.SwapStr("\r\n", "\t", (string)Value);
                case CtrlType.Int:
                    return ((int)Value).ToString();
                case CtrlType.BindAddr:
                    return Value.ToString();
                case CtrlType.AddressV4:
                    return Value.ToString();
                default:


                    break;
            }

            if (this.Value == null)
                return null;

            if (this.ValueType == typeof(Dat))
            {
                return ((Dat)this.Value).ToReg(isSecret);
            }
            else if (this.ValueType == typeof(bool))
            {
                return this.Value.ToString().ToLower();
            }

            // isSecretの対応
            if (this.IsSecret)
            {
                if (isSecret)
                {
                    return "***";
                }
                else
                {
                    return Crypt.Encrypt(this.Value.ToString());
                }
            }

            return this.Value.ToString();
        }

        public object ToJson(bool isSecret)
        {
            switch (this.CtrlType)
            {
                case CtrlType.CheckBox:
                    return (bool)Value;
                case CtrlType.Label:
                case CtrlType.TextBox:
                case CtrlType.Font:
                case CtrlType.File:
                case CtrlType.Folder:
                    if (this.Value == null) return null;
                    return (string)Value;
                case CtrlType.Hidden:
                    if (isSecret)
                    {
                        return "***";
                    }
                    try
                    {
                        return Crypt.Encrypt((String)Value);
                    }
                    catch (Exception)
                    {
                        return "ERROR";
                    }
                case CtrlType.Memo:
                    return Util.SwapStr("\r\n", "\t", (string)Value);
                case CtrlType.Int:
                case CtrlType.Radio:
                    return (int)Value;
                case CtrlType.AddressV4:
                case CtrlType.AddressV6:
                case CtrlType.BindAddr:
                case CtrlType.ComboBox:
                    return Value.ToString();
                case CtrlType.TabPage:
                case CtrlType.Group:
                case CtrlType.Dat:
                default:
                    break;
            }

            if (this.Value == null)
                return null;

            return this.Value.ToString();
        }

        public string ToCtrlString()
        {
            return this.CtrlType.GetControlTypeString();
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
                switch (this.CtrlType)
                {
                    case CtrlType.CheckBox:
                        bool valCheckBox;
                        if (!bool.TryParse(str, out valCheckBox))
                        {
                            Value = false;
                            return false;
                        }
                        Value = valCheckBox;
                        break;
                    case CtrlType.Memo:
                        Value = Util.SwapStr("\t", "\r\n", str);
                        break;
                    case CtrlType.File:
                    case CtrlType.Folder:
                    case CtrlType.TextBox:
                        Value = str;
                        break;
                    case CtrlType.Hidden:
                        try
                        {
                            Value = Crypt.Decrypt(str);
                        }
                        catch (Exception)
                        {
                            Value = "";
                            return false;
                        }
                        break;
                    case CtrlType.ComboBox:
                        object valCombo;
                        try
                        {

                            valCombo = Enum.Parse(ValueType, str);
                            var values = Enum.GetValues(ValueType);
                            bool isMatch = false;
                            object firstValue = null;
                            foreach (var v in values)
                            {
                                if (firstValue == null) firstValue = v;
                                if (Enum.Equals(valCombo, v))
                                {
                                    isMatch = true;
                                    break;
                                }
                            }
                            if (!isMatch)
                            {
                                valCombo = firstValue;
                                return false;
                            }

                        }
                        catch
                        {
                            valCombo = Enum.GetValues(ValueType).GetValue(0);
                            Value = valCombo;
                            return false;
                        }
                        Value = valCombo;
                        break;
                    case CtrlType.Radio:
                        Int32 valRadio;
                        if (!Int32.TryParse(str, out valRadio))
                        {
                            Value = 0;
                            return false;
                        }
                        Value = valRadio;
                        if ((int)Value < 0)
                        {
                            Value = 0;
                            return false;
                        }
                        break;
                    case CtrlType.Int:
                        Int32 valInt;
                        if (!Int32.TryParse(str, out valInt))
                        {
                            Value = 0;
                            return false;
                        }
                        Value = valInt;
                        break;
                    case CtrlType.BindAddr:
                        try
                        {
                            Value = new BindAddr(str);
                        }
                        catch (ValidObjException)
                        {
                            Value = 0;
                            return false;
                        }
                        break;
                    case CtrlType.AddressV4:
                        try
                        {
                            Value = new Ip(str);
                        }
                        catch (ValidObjException)
                        {
                            Value = null;
                            return false;
                        }
                        break;
                    default:

                        if (this.ValueType == typeof(Dat))
                        {
                            return ((Dat)this.Value).FromReg(str);
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
                        break;
                }

            }
            catch (Exception)
            {
                _kernel.Logger.TraceError($"Error OneVal.FromReg({str})");
                Value = null;
                return false;
            }
            _kernel.Logger.TraceInformation($"{this.Name}={this.Value}");
            return true;
        }

        public bool FromJson(JArray val)
        {
            try
            {
                switch (this.CtrlType)
                {
                    case CtrlType.Dat:
                        var dat = (Dat)this.Value;
                        dat.FromJson(val);
                        break;
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                _kernel.Logger.TraceError($"Error OneVal.FromReg({val})");
                Value = null;
                return false;
            }
            _kernel.Logger.TraceInformation($"{this.Name}={this.Value}");
            return true;
        }

        public bool FromJson(int val)
        {
            try
            {
                switch (this.CtrlType)
                {
                    case CtrlType.Radio:
                    case CtrlType.Int:
                        Value = val;
                        break;
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                _kernel.Logger.TraceError($"Error OneVal.FromReg({val})");
                Value = null;
                return false;
            }
            _kernel.Logger.TraceInformation($"{this.Name}={this.Value}");
            return true;
        }

        public bool FromJson(bool val)
        {
            try
            {
                switch (this.CtrlType)
                {
                    case CtrlType.CheckBox:
                        Value = val;
                        break;
                    default:
                        return false;
                }

            }
            catch (Exception)
            {
                _kernel.Logger.TraceError($"Error OneVal.FromReg({val})");
                Value = null;
                return false;
            }
            _kernel.Logger.TraceInformation($"{this.Name}={this.Value}");
            return true;
        }

        public bool FromJson(string str)
        {
            if (str == null)
            {
                Value = null;
                return false;
            }
            try
            {
                switch (this.CtrlType)
                {
                    case CtrlType.ComboBox:
                        Value = Enum.Parse(ValueType, str);
                        break;
                    case CtrlType.Memo:
                        Value = Util.SwapStr("\t", "\r\n", str);
                        break;
                    case CtrlType.File:
                    case CtrlType.Folder:
                    case CtrlType.TextBox:
                        Value = str;
                        break;
                    case CtrlType.Hidden:
                        try
                        {
                            Value = Crypt.Decrypt(str);
                        }
                        catch (Exception)
                        {
                            Value = "";
                            return false;
                        }
                        break;
                    case CtrlType.BindAddr:
                        try
                        {
                            Value = new BindAddr(str);
                        }
                        catch (ValidObjException)
                        {
                            Value = 0;
                            return false;
                        }
                        break;
                    case CtrlType.AddressV4:
                        try
                        {
                            Value = new Ip(str);
                        }
                        catch (ValidObjException)
                        {
                            Value = null;
                            return false;
                        }
                        break;
                    default:
                        return false;
                }

            }
            catch (Exception)
            {
                _kernel.Logger.TraceError($"Error OneVal.FromReg({str})");
                Value = null;
                return false;
            }
            _kernel.Logger.TraceInformation($"{this.Name}={this.Value}");
            return true;
        }

    }


}

