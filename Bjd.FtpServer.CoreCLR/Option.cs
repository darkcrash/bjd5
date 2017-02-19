using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;

namespace Bjd.FtpServer
{
    public class Option : OneOption
    {

        //public override string JpMenu { get { return "FTPサーバ"; } }
        //public override string EnMenu { get { return "FTP Server"; } }
        public override char Mnemonic { get { return 'F'; } }


        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            Add(new OneVal(kernel, CtrlType.CheckBox, "useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1(kernel, "Basic", Lang.Value("Basic")));
            pageList.Add(Page2(kernel, "VirtualFolder", Lang.Value("VirtualFolder")));
            pageList.Add(Page3(kernel, "User", Lang.Value("User")));
            pageList.Add(PageAcl());
            //Add(new OneVal("tab", null, Crlf.Nextline));

            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 21, 30, 50)); //サーバ基本設定
            CreateServerOption(ProtocolKind.Tcp, 21, 30, 50);

            //var key = "bannerMessage";
            //onePage.Add(new OneVal(key, "FTP ( $p Version $v ) ready", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 80)));
            Add(new OneVal(kernel, CtrlType.TextBox, "bannerMessage", "FTP ( $p Version $v ) ready", Crlf.Nextline));
            //ライブドア特別仕様
            //onePage.Add(new OneVal(new ValType(CRLF.NEXTLINE, VTYPE.FILE, (IsJp()) ? "ファイル受信時に起動するスクリプト" : "auto run acript", 250,kernel), "autoRunScript","c:\\test.bat"));
            //key = "useSyst";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useSyst", false, Crlf.Nextline));
            //key = "reservationTime";
            //onePage.Add(new OneVal(key, 5000, Crlf.Nextline, new CtrlInt(Lang.Value(key), 6)));
            Add(new OneVal(kernel, CtrlType.Int, "reservationTime", 5000, Crlf.Nextline));
            return onePage;
        }

        private OnePage Page2(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            var listVal = new ListVal();
            //var key = "fromFolder";
            //listVal.Add(new OneVal(key, "", Crlf.Nextline, new CtrlFolder(Lang.Value(key), 70, kernel)));
            listVal.Add(new OneVal(kernel, CtrlType.Folder, "fromFolder", "", Crlf.Nextline));
            //key = "toFolder";
            //listVal.Add(new OneVal(key, "", Crlf.Nextline, new CtrlFolder(Lang.Value(key), 70, kernel)));
            listVal.Add(new OneVal(kernel, CtrlType.Folder, "toFolder", "", Crlf.Nextline));
            //key = "mountList";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), listVal, 360, Lang.LangKind)));
            Add(new OneVal(kernel, CtrlType.Dat, "mountList", new Dat(listVal), Crlf.Nextline));
            return onePage;
        }

        private OnePage Page3(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            var listVal = new ListVal();
            //var key = "accessControl";
            //listVal.Add(new OneVal(key, 0, Crlf.Nextline, new CtrlComboBox(Lang.Value(key), new[] { "FULL", "DOWN", "UP" }, 100)));
            listVal.Add(new OneVal(kernel, CtrlType.ComboBox, "accessControl", FtpAcl.Full, Crlf.Nextline));
            //key = "homeDirectory";
            //listVal.Add(new OneVal(key, "", Crlf.Nextline, new CtrlFolder(Lang.Value(key), 60, kernel)));
            listVal.Add(new OneVal(kernel, CtrlType.Folder, "homeDirectory", "", Crlf.Nextline));
            //key = "userName";
            //listVal.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 20)));
            listVal.Add(new OneVal(kernel, CtrlType.TextBox, "userName", "", Crlf.Nextline));
            //key = "password";
            //listVal.Add(new OneVal(key, "", Crlf.Nextline, new CtrlHidden(Lang.Value(key), 20)));
            listVal.Add(new OneVal(kernel, CtrlType.Hidden, "password", "", Crlf.Nextline, true));
            //key = "user";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), listVal, 360, Lang.LangKind)));
            Add(new OneVal(kernel, CtrlType.Dat, "user", new Dat(listVal), Crlf.Nextline));
            return onePage;
        }


    }
}
