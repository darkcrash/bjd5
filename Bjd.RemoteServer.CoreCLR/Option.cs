using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;

namespace Bjd.RemoteServer
{
    class Option : OneOption
    {
        public override char Mnemonic { get { return 'R'; } }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {
            //var key = "useServer";
            //Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            //var pageList = new List<OnePage>();
            //key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //pageList.Add(PageAcl());
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            Add(new OneVal(kernel, CtrlType.CheckBox, "useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1(kernel, "Basic", Lang.Value("Basic")));
            pageList.Add(PageAcl());
            Add(new OneVal(kernel, CtrlType.TabPage, "tab", null, Crlf.Nextline));


            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 10001, 60, 1)); //サーバ基本設定
            //var key = "password";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlHidden(Lang.Value(key), 20)));

            CreateServerOption(ProtocolKind.Tcp, 10001, 60, 1);
            Add(new OneVal(kernel, CtrlType.Hidden, "password", "", Crlf.Nextline, true));

            return onePage;
        }

    }
}
