using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Bjd.Controls;
using Bjd.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Bjd.Utils
{
    //ファイルを使用した設定情報の保存<br>
    //1つのデフォルト値ファイルを使用して2つのファイルを出力する<br>
    public class IniDb
    {
        private readonly String _fileIni;
        private readonly String _fileDef;
        private readonly String _fileTxt;
        private readonly String _fileIniJson;

        public IniDb(String progDir, String fileName)
        {
            //_fileIni = progDir + "\\" + fileName + ".ini";
            //_fileDef = progDir + "\\" + fileName + ".def";
            //_fileTxt = progDir + "\\" + fileName + ".txt";
            _fileIni = System.IO.Path.Combine(progDir, fileName + ".ini");
            _fileDef = System.IO.Path.Combine(progDir, fileName + ".def");
            _fileTxt = System.IO.Path.Combine(progDir, fileName + ".txt");
            _fileIniJson = System.IO.Path.Combine(progDir, fileName + ".json");

        }

        public string Path
        {
            get
            {
                return _fileIni;
            }
        }

        private string CtrlType2Str(CtrlType ctrlType)
        {
            switch (ctrlType)
            {
                case CtrlType.CheckBox:
                    return "BOOL";
                case CtrlType.TextBox:
                    return "STRING";
                case CtrlType.Hidden:
                    return "HIDE_STRING";
                case CtrlType.ComboBox:
                    return "LIST";
                case CtrlType.Folder:
                    return "FOLDER";
                case CtrlType.File:
                    return "FILE";
                case CtrlType.Dat:
                    return "DAT";
                case CtrlType.Int:
                    return "INT";
                case CtrlType.AddressV4:
                    return "ADDRESS_V4";
                case CtrlType.BindAddr:
                    return "BINDADDR";
                case CtrlType.Font:
                    return "FONT";
                case CtrlType.Group:
                    return "GROUP";
                case CtrlType.Label:
                    return "LABEL";
                case CtrlType.Memo:
                    return "MEMO";
                case CtrlType.Radio:
                    return "RADIO";
                case CtrlType.TabPage:
                    return "TAB_PAGE";
            }
            throw new Exception("コントロールの型名が実装されていません OneVal::TypeStr()　" + ctrlType);
        }


        //１行を読み込むためのオブジェクト
        private class LineObject
        {
            public string NameTag { get; private set; }
            public string Name { get; private set; }
            public string ValStr { get; private set; }
            // public LineObject(CtrlType ctrlType, String nameTag, String name,String valStr) {
            public LineObject(String nameTag, String name, String valStr)
            {
                // this.ctrlType = ctrlType;
                NameTag = nameTag;
                Name = name;
                ValStr = valStr;
            }
        }

        //解釈に失敗した場合はnullを返す
        private static LineObject ReadLine(String str)
        {
            var index = str.IndexOf('=');
            if (index == -1)
            {
                return null;
            }
            //		CtrlType ctrlType = str2CtrlType(str.substring(0, index));
            str = str.Substring(index + 1);
            index = str.IndexOf('=');
            if (index == -1)
            {
                return null;
            }
            var buf = str.Substring(0, index);
            var tmp = buf.Split('\b');
            if (tmp.Length != 2)
            {
                return null;
            }
            var nameTag = tmp[0];
            var name = tmp[1];

            var valStr = str.Substring(index + 1);
            return new LineObject(nameTag, name, valStr);
        }

        private bool ReadJson(string nameTag, ListVal listVal)
        {
            if (!File.Exists(_fileIniJson))
                return false;
            var isRead = false;

            // read json object from file
            JObject jObject = null;
            if (File.Exists(_fileIniJson))
            {
                try
                {
                    using (var fs = new FileStream(_fileIniJson, FileMode.Open, FileAccess.Read,  FileShare.Write))
                    using (var reader = new StreamReader(fs))
                    {
                        var jsonReader = new JsonTextReader(reader);
                        var jsonSerializer = new JsonSerializer();
                        var o = jsonSerializer.Deserialize(jsonReader);
                        jObject = o as JObject;
                    }
                }
                catch { }

            }

            // create json object
            if (jObject == null)
                return false;

            foreach (var childToken in jObject.Children())
            {
                // The permitted childToken is only property
                if (childToken.Type != JTokenType.Property) continue;

                // The permitted value is only object-type
                var childProperty = (JProperty)childToken;
                if (childProperty.Value.Type != JTokenType.Object) continue;

                var jsonnameTag = childProperty.Name;

                if (!(jsonnameTag == nameTag || jsonnameTag == nameTag + "Server"))
                    continue;

                var childObject = (JObject)childProperty.Value;

                foreach (var valToken in childObject.Children())
                {
                    // The permitted childToken is only property
                    if (valToken.Type != JTokenType.Property) continue;
                    var valProperty = (JProperty)valToken;
                    var valName = valProperty.Name;

                    var oneVal = listVal.Search(valName);
                    if (oneVal == null) continue;

                    try
                    {

                        switch (oneVal.CtrlType)
                        {
                            case CtrlType.Dat:
                                var valArray = (JArray)valProperty.Value;
                                oneVal.FromJson(valArray);
                                break;
                            case CtrlType.CheckBox:
                                var valBool = valProperty.Value.Value<bool>();
                                oneVal.FromJson(valBool);
                                break;
                            case CtrlType.Int:
                            case CtrlType.Radio:
                                var valInt = valProperty.Value.Value<int>();
                                oneVal.FromJson(valInt);
                                break;
                            case CtrlType.ComboBox:
                            case CtrlType.AddressV4:
                            case CtrlType.AddressV6:
                            case CtrlType.BindAddr:
                            case CtrlType.File:
                            case CtrlType.Folder:
                            case CtrlType.Font:
                            case CtrlType.Label:
                            case CtrlType.Memo:
                            case CtrlType.TextBox:
                                var valString = valProperty.Value.Value<string>();
                                oneVal.FromJson(valString);
                                break;
                            default:
                                continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceInformation($"IniDbRead:Exception {ex.Message}");
                        continue;
                    }

                    isRead = true; // 1件でもデータを読み込んだ場合にtrue
                }


            }

            return isRead;

        }

        private bool Read(string fileName, string nameTag, ListVal listVal)
        {
            if (!File.Exists(fileName))
                return false;
            var isRead = false;

            System.Diagnostics.Trace.Indent();
            System.Diagnostics.Trace.TraceInformation($"IniDbRead:{fileName} - {nameTag}");
            System.Diagnostics.Trace.Unindent();

            var lines = File.ReadAllLines(fileName, Encoding.UTF8);

            foreach (var s in lines)
            {
                var o = ReadLine(s);

                if (o == null)
                    continue;

                if (!(o.NameTag == nameTag || o.NameTag == nameTag + "Server"))
                    continue;

                var oneVal = listVal.Search(o.Name);

                //Ver5.9.2 過去バージョンのOption.ini読み込みへの対応
                //ProxyPop3 拡張設定
                if (o.NameTag == "ProxyPop3Server" && o.Name == "specialUser")
                {
                    oneVal = listVal.Search("specialUserList");
                }

                //Ver5.8.8 過去バージョンのOption.ini読み込みへの対応
                if (oneVal == null)
                {
                    if (o.Name == "nomalFileName")
                    {
                        oneVal = listVal.Search("normalLogKind");
                    }
                    else if (o.Name == "secureFileName")
                    {
                        oneVal = listVal.Search("secureLogKind");
                        //Ver5.9.2
                    }
                    else if (o.Name == "LimitString")
                    {
                        oneVal = listVal.Search("limitString");
                    }
                    else if (o.Name == "UseLimitString")
                    {
                        oneVal = listVal.Search("useLimitString");
                    }
                    else if (o.Name == "EnableLimitString")
                    {
                        oneVal = listVal.Search("isDisplay");
                    }
                    else if (o.Name == "useLog")
                    {
                        oneVal = listVal.Search("useLogFile");
                    }
                }

                //Ver6.1.0 過去バージョンのOption.ini読み込みへの対応
                //DnsDomain
                if (o.NameTag == "DnsDomain")
                {
                    var col = o.ValStr.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (col.Length == 1)
                    {
                        //Ver6.0.9以前は、カラムが１つであるので、( \tgoogle.com )
                        //追加された２カラム目のデフォルト値を追加して読み直す (\tgoogle.com\True )
                        o = ReadLine(s + "\tTrue");
                    }
                }

                if (oneVal != null)
                {
                    if (!oneVal.FromReg(o.ValStr))
                    {
                        if (o.ValStr != "")
                        {
                        }
                    }
                    isRead = true; // 1件でもデータを読み込んだ場合にtrue
                }
            }

            return isRead;
        }

        //ファイルの削除
        public void Delete()
        {
            if (File.Exists(_fileTxt))
            {
                File.Delete(_fileTxt);
            }
            if (File.Exists(_fileIni))
            {
                File.Delete(_fileIni);
            }
            if (File.Exists(_fileIniJson))
            {
                File.Delete(_fileIniJson);
            }
        }



        // 読込み
        public void Read(string nameTag, ListVal listVal)
        {
            var isReadJson = ReadJson(nameTag, listVal);
            if (!isReadJson)
            {
                var isRead = Read(_fileIni, nameTag, listVal);
                if (!isRead)
                {
                    //１件も読み込まなかった場合
                    //defファイルには、Web-local:80のうちのWeb (-の前の部分)がtagとなっている
                    var n = nameTag.Split('-')[0];
                    Read(_fileDef, n, listVal); //デフォルト設定値を読み込む
                }
            }
            //SaveJson(nameTag, listVal);
        }


        // 保存
        public void Save(string nameTag, ListVal listVal)
        {
            // Ver5.0.1 デバッグファイルに対象のValListを書き込む
            for (var i = 0; i < 2; i++)
            {
                var target = (i == 0) ? _fileIni : _fileTxt;
                var isSecret = i != 0;

                // 対象外のネームスペース行を読み込む
                var lines = new List<string>();
                if (File.Exists(target))
                {
                    foreach (var s in File.ReadAllLines(target, Encoding.UTF8))
                    {
                        LineObject o;
                        try
                        {
                            o = ReadLine(s);
                            // nameTagが違う場合、listに追加
                            if (o.NameTag != nameTag)
                            {
                                //Ver5.8.4 Ver5.7.xの設定を排除する
                                var index = o.NameTag.IndexOf("Server");
                                if (index != -1 && index == o.NameTag.Length - 6)
                                {
                                    // ～～Serverの設定を削除
                                }
                                else
                                {
                                    lines.Add(s);
                                }

                            }
                        }
                        catch
                        {
                            //TODO エラー処理未処理
                        }
                    }
                }
                // 対象のValListを書き込む
                foreach (var o in listVal.GetSaveList(null))
                {
                    //var ctrlStr = CtrlType2Str(ctrlType);
                    var ctrlStr = o.ToCtrlString();
                    lines.Add(string.Format("{0}={1}\b{2}={3}", ctrlStr, nameTag, o.Name, o.ToReg(isSecret)));
                }
                var enc = System.Text.CodePagesEncodingProvider.Instance;
                File.WriteAllLines(target, lines.ToArray(), enc.GetEncoding(932));
            }
        }

        private bool SaveJson(string nameTag, ListVal listVal)
        {
            var numberList = new List<Type> { typeof(int) };
            var boolList = new List<Type> { typeof(bool) };

            // read json object from file
            JObject jObject = null;
            if (File.Exists(_fileIniJson))
            {
                try
                {
                    using (var fs = new FileStream(_fileIniJson, FileMode.Open, FileAccess.Read))
                    using (var reader = new StreamReader(fs))
                    {
                        var jsonReader = new JsonTextReader(reader);
                        var jsonSerializer = new JsonSerializer();
                        var o = jsonSerializer.Deserialize(jsonReader);
                        jObject = o as JObject;
                        jObject.Remove(nameTag);
                    }
                }
                catch { }

            }

            // create json object
            if (jObject == null)
            {
                jObject = new JObject();
            }

            // write file from json object
            using (var fs = new FileStream(_fileIniJson, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(fs, Encoding.UTF8))
            {
                var jsonWriter = new JsonTextWriter(writer);
                var jsonSerializer = new JsonSerializer();

                // cast value to ValueType
                Func<OneVal, string, object> OneValToValue =
                    (o, v) =>
                    {
                        if (o.CtrlType == CtrlType.ComboBox)
                        {
                            return v;
                        }
                        else if (numberList.Contains(o.ValueType))
                        {
                            return Convert.ToInt32(v);
                        }
                        else if (boolList.Contains(o.ValueType))
                        {
                            return Convert.ToBoolean(v);
                        }
                        return v;
                    };

                // Dat to Json
                Func<List<OneVal>, List<DatRecord>, JArray> CreateJObjectArray = null;
                CreateJObjectArray = (lv, ld) =>
                {
                    JArray value = new JArray();
                    foreach (var rec in ld)
                    {
                        JObject inlineObject = new JObject();

                        JProperty inlineObjectEnable = new JProperty("enable", rec.Enable);
                        inlineObject.Add(inlineObjectEnable);

                        for (var i = 0; i < lv.Count; i++)
                        {
                            var currentOneVal = lv[i];
                            //var v2 = currentOneVal.ToJson(false);
                            var v = OneValToValue(currentOneVal, rec.ColumnValueList[i]);
                            JProperty inlineObjectProp = new JProperty(currentOneVal.Name, v);
                            inlineObject.Add(inlineObjectProp);
                        }

                        value.Add(inlineObject);
                    }

                    return value;
                };

                // to json
                Func<List<OneVal>, JObject> CreateJObject = null;
                CreateJObject = (lv) =>
                {
                    JObject value = new JObject();
                    // 対象のValListを書き込む
                    foreach (var o in lv)
                    {
                        JProperty childProperty;
                        if (o.CtrlType == CtrlType.TabPage) continue;
                        if (o.CtrlType == CtrlType.Group) continue;
                        else if (o.CtrlType == CtrlType.Dat)
                        {
                            var d = o.Value as Dat;

                            var dList = d.GetList();
                            var valList = d.GetOneDatList();

                            var childKey = o.Name;
                            var childObject = CreateJObjectArray(dList, valList);

                            childProperty = new JProperty(childKey, childObject);
                        }
                        else
                        {
                            childProperty = new JProperty(o.Name, o.ToJson(false));
                        }
                        value.Add(childProperty);
                    }
                    return value;
                };

                // create json object
                var obj = CreateJObject(listVal.GetSaveList(null));
                var prop = new JProperty(nameTag, obj);
                jObject.Add(prop);

                // serialize with indented formatting
                jsonWriter.Formatting = Formatting.Indented;
                jsonSerializer.Serialize(jsonWriter, jObject);
                jsonWriter.Flush();

            }

            return true;

        }



        // 設定ファイルから"lang"の値を読み出す
        public bool IsJp()
        {
            var listVal = new ListVal{
                new OneVal(CtrlType.ComboBox, "lang", LangKind.Jp, Crlf.Nextline)
            };
            Read("Basic", listVal);
            var oneVal = listVal.Search("lang");
            var bjdLangId = (int)oneVal.Value;
            if (bjdLangId == 2/*Auto*/)
            {

                return (System.Globalization.CultureInfo.CurrentUICulture.Name == "ja-JP");
            }
            else
            {
                return (bjdLangId == 0);
            }
        }
    }
}

