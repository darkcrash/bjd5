using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;

namespace Bjd.SipServer
{
    public class Option : OneOption
    {

        //メニューに表示される文字列
        //public override string JpMenu { get { return "SIPサーバ"; } }
        //public override string EnMenu { get { return "Sip Server"; } }
        public override char Mnemonic { get { return 'Z'; } }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel.IsJp, path, nameTag)
        {

            //var key = "useServer";
            //Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            //var pageList = new List<OnePage>();
            //key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //pageList.Add(PageAcl());
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            Add(new OneVal("useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1("Basic", Lang.Value("Basic"), kernel));
            pageList.Add(PageAcl());
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));
            Add(new OneVal("tab", null, Crlf.Nextline));


            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 5060, 30, 30)); //サーバ基本設定
            //var key = "sampleText";
            //onePage.Add(new OneVal(key, "Sample Server : ", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 60)));

            CreateServerOption(ProtocolKind.Tcp, 5060, 30, 30); //サーバ基本設定
            Add(new OneVal("sampleText", "Sample Server : ", Crlf.Nextline));

            return onePage;
        }


    }
}
