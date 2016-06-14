using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Options;


namespace Bjd.SmtpServer
{
    class OptionMl : OneOption
    {

        public override char Mnemonic { get { return 'A'; } }

        public OptionMl(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            var pageList = new List<OnePage>();
            //var key = "Mailing List";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            pageList.Add(Page1("Mailing List", Lang.Value("Mailing List"), kernel));
            Add(new OneVal(CtrlType.TabPage, "tab", null, Crlf.Nextline));

            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);

            var list = new ListVal();
            //var key = "user";
            //list.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 250)));
            //key = "mlList";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), list, 250, Lang.LangKind)));

            list.Add(new OneVal(CtrlType.TextBox, "user", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "mlList", new Dat(list), Crlf.Nextline));

            return onePage;
        }


    }
}
