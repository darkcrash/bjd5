using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Configurations;

namespace Bjd.SipServer
{
    public class Option : OneOption
    {

        //メニューに表示される文字列
        //public override string JpMenu { get { return "SIPサーバ"; } }
        //public override string EnMenu { get { return "Sip Server"; } }
        public override char Mnemonic { get { return 'Z'; } }

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
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));
            Add(new OneVal(kernel, CtrlType.TabPage, "tab", null, Crlf.Nextline));


            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 5060, 30, 30)); //サーバ基本設定
            //var key = "sampleText";
            //onePage.Add(new OneVal(key, "Sample Server : ", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 60)));

            CreateServerOption(ProtocolKind.Tcp, 5060, 30, 30); //サーバ基本設定
            Add(new OneVal(kernel, CtrlType.TextBox, "sampleText", "Sample Server : ", Crlf.Nextline));

            return onePage;
        }


    }
}
