using System.Collections.Generic;
using Bjd;
using Bjd.ctrl;
using Bjd.net;
using Bjd.option;

namespace BJD.WebApiServer
{
    public class Option : OneOption
    {

        //メニューに表示される文字列
        //public override string JpMenu { get { return "WebAPIサーバ"; } }
        //public override string EnMenu { get { return "WebAPI Server"; } }
        public override char Mnemonic { get { return 'A'; } }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel.IsJp(), path, nameTag)
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
            Add(new OneVal("tab", null, Crlf.Nextline));

            Read(kernel.IniDb); //　レジストリからの読み込み

        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 5050, 30, 10)); //サーバ基本設定
            CreateServerOption(ProtocolKind.Tcp, 5050, 30, 10); //サーバ基本設定

            return onePage;
        }



    }
}
