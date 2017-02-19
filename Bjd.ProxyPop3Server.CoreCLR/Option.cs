using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;

namespace Bjd.ProxyPop3Server
{
    internal class Option : OneOption
    {
        public override char Mnemonic
        {
            get { return 'P'; }
        }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            //var key = "useServer";
            //Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            //var pageList = new List<OnePage>();
            //key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //key = "Expansion";
            //pageList.Add(Page2(key, Lang.Value(key), kernel));
            //pageList.Add(PageAcl());
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            Add(new OneVal(kernel, CtrlType.CheckBox, "useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1(kernel, "Basic", Lang.Value("Basic")));
            pageList.Add(Page2(kernel, "Expansion", Lang.Value("Expansion")));
            pageList.Add(PageAcl());
            Add(new OneVal(kernel, CtrlType.TabPage, "tab", null, Crlf.Nextline));


            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 8110, 60, 10)); //サーバ基本設定
            //var key = "targetPort";
            //onePage.Add(new OneVal(key, 110, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "targetServer";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "idleTime";
            //onePage.Add(new OneVal(key, 1, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));

            CreateServerOption(ProtocolKind.Tcp, 8110, 60, 10); //サーバ基本設定
            Add(new OneVal(kernel, CtrlType.Int, "targetPort", 110, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.TextBox, "targetServer", "", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Int, "idleTime", 1, Crlf.Nextline));

            return onePage;
        }

        private OnePage Page2(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);

            //var l = new ListVal();
            //var key = "specialUser";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 20)));
            //key = "specialServer";
            //l.Add(new OneVal(key, "", Crlf.Contonie, new CtrlTextBox(Lang.Value(key), 20)));
            //key = "specialPort";
            //l.Add(new OneVal(key, 110, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "specialName";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 20)));
            //key = "specialUserList";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), l, 360, Lang.LangKind)));

            var l = new ListVal();
            l.Add(new OneVal(kernel, CtrlType.TextBox, "specialUser", "", Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.TextBox, "specialServer", "", Crlf.Contonie));
            l.Add(new OneVal(kernel, CtrlType.Int, "specialPort", 110, Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.TextBox, "specialName", "", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Dat, "specialUserList", new Dat(l), Crlf.Nextline));

            return onePage;
        }

    }
}


