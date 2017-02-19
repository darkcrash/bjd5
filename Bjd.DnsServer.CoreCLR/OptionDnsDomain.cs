using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Options;

namespace Bjd.DnsServer
{
    public class OptionDnsDomain : OneOption
    {

        public override char Mnemonic { get { return 'A'; } }

        public OptionDnsDomain(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            var pageList = new List<OnePage>();
            //var key = "Basic";
            //pageList.Add(Page1(key,Lang.Value(key),kernel));
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            pageList.Add(Page1(kernel, "Basic", Lang.Value("Basic")));
            Add(new OneVal(kernel, CtrlType.TabPage, "tab", null, Crlf.Nextline));

            Read(kernel.Configuration); //�@���W�X�g������̓ǂݍ���
        }

        private OnePage Page1(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            var list = new ListVal();
            //var key = "name";
            //list.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 80)));
            //key = "authority";
            //list.Add(new OneVal(key, true, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            //onePage.Add(new OneVal("domainList", null, Crlf.Nextline, new CtrlDat("", list, 400, Lang.LangKind)));

            list.Add(new OneVal(kernel, CtrlType.TextBox, "name", "", Crlf.Nextline));
            list.Add(new OneVal(kernel, CtrlType.CheckBox, "authority", true, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Dat, "domainList", new Dat(list), Crlf.Nextline));

            return onePage;

        }
    }
}
