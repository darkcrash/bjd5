using System;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Utils;

namespace Bjd.Configurations
{
    abstract public class ConfigurationBase : IDisposable
    {

        public ListVal ListVal { get; private set; }
        private readonly bool _isJp;
        public string NameTag { get; private set; }
        public string Path { get; private set; }//実態が格納されているモジュール(DLL)のフルパス

        abstract public char Mnemonic { get; }

        virtual public string MenuStr
        {
            get { return Lang.Value("MenuStr"); }
        }

        //Ver6.1.6
        protected readonly Lang Lang;
        private Kernel _kernel;

        public ConfigurationBase(Kernel kernel, string path, string nameTag)
        {
            _kernel = kernel;
            ListVal = new ListVal();
            _isJp = kernel.IsJp;
            Path = path;
            NameTag = nameTag;
            //Ver6.1.6
            Lang = new Lang(kernel, _isJp ? LangKind.Jp : LangKind.En, "Option" + nameTag);

        }


        //レジストリへ保存
        public virtual void Save(IniDb iniDb)
        {
            iniDb.Save(NameTag, ListVal);//レジストリへ保存
        }



        //レジストリからの読み込み
        public virtual void Read(IniDb iniDb)
        {
            iniDb.Read(NameTag, ListVal);
        }

        protected OnePage PageAcl()
        {
            var onePage = new OnePage("ACL", "ACL");
            //onePage.Add(new OneVal("enableAcl", 0, Crlf.Nextline));
            Add(new OneVal(_kernel, CtrlType.Radio, "enableAcl", 0, Crlf.Nextline));
            var list = new ListVal();
            list.Add(new OneVal(_kernel, CtrlType.TextBox, "aclName", "", Crlf.Nextline));
            list.Add(new OneVal(_kernel, CtrlType.TextBox, "aclAddress", "", Crlf.Nextline));
            //onePage.Add(new OneVal("acl", new Dat(list), Crlf.Nextline));
            Add(new OneVal(_kernel, CtrlType.Dat, "acl", new Dat(list), Crlf.Nextline));

            return onePage;
        }


        //OneValとしてサーバ基本設定を作成する
        //protected OneVal CreateServerOption(ProtocolKind protocolKind, int port, int timeout, int multiple)
        //{
        //    var list = new ListVal();
        //    list.Add(new OneVal("protocolKind", protocolKind, Crlf.Contonie));
        //    list.Add(new OneVal("port", port, Crlf.Nextline));
        //    var localAddress = LocalAddress.GetInstance();
        //    var v4 = localAddress.V4;
        //    var v6 = localAddress.V6;
        //    list.Add(new OneVal("bindAddress2", new BindAddr(), Crlf.Nextline));
        //    list.Add(new OneVal("useResolve", false, Crlf.Nextline));
        //    list.Add(new OneVal("useDetailsLog", true, Crlf.Contonie));
        //    list.Add(new OneVal("multiple", multiple, Crlf.Contonie));
        //    list.Add(new OneVal("timeOut", timeout, Crlf.Nextline));
        //    return new OneVal("GroupServer", new Dat(list), Crlf.Nextline);
        //}
        protected void CreateServerOption(ProtocolKind protocolKind, int port, int timeout, int multiple)
        {
            Add(new OneVal(_kernel, CtrlType.ComboBox, "protocolKind", protocolKind, Crlf.Contonie));
            Add(new OneVal(_kernel, CtrlType.Int, "port", port, Crlf.Nextline));
            Add(new OneVal(_kernel, CtrlType.BindAddr, "bindAddress2", new BindAddr(), Crlf.Nextline));
            Add(new OneVal(_kernel, CtrlType.CheckBox, "useResolve", false, Crlf.Nextline));
            Add(new OneVal(_kernel, CtrlType.CheckBox, "useDetailsLog", true, Crlf.Contonie));
            Add(new OneVal(_kernel, CtrlType.Int, "multiple", multiple, Crlf.Contonie));
            Add(new OneVal(_kernel, CtrlType.Int, "timeOut", timeout, Crlf.Nextline));
        }


        //OneValの追加
        public void Add(OneVal oneVal)
        {
            ListVal.Add(oneVal);
        }

        //値の設定
        public void SetVal(IniDb iniDb, string name, object value)
        {
            var oneVal = ListVal.Search(name);
            if (oneVal == null)
            {
                Util.RuntimeException(string.Format("名前が見つかりません name={0}", name));
                return;
            }
            //コントロールの値を変更
            //oneVal.OneCtrl.Write(value);

            //Ver6.0.0
            oneVal.SetValue(value);

            //レジストリへ保存
            Save(iniDb);

        }
        //値の取得
        public object GetValue(string name)
        {
            var oneVal = ListVal.Search(name);
            if (oneVal == null)
            {
                Util.RuntimeException(string.Format("名前が見つかりません name={0}", name));
                return null;
            }
            return oneVal.Value;
        }


        //「サーバを使用する」の状態取得
        public bool UseServer
        {
            get
            {
                var oneVal = ListVal.Search("useServer");
                if (oneVal == null)
                {
                    return false;
                }
                return (bool)oneVal.Value;
            }
        }

        public virtual void Dispose()
        {

        }
    }
}

