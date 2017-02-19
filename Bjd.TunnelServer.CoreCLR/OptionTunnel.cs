using System.Collections.Generic;

using Bjd;
using Bjd.Controls;
using Bjd.Options;
using Bjd.Net;

namespace Bjd.TunnelServer
{
    internal class OptionTunnel : OneOption
    {
        public override char Mnemonic { get { return 'A'; } }

        public OptionTunnel(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            var pageList = new List<OnePage>();
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            pageList.Add(Page1(kernel, "Basic", Lang.Value("Basic")));
            //pageList.Add(PageAcl());
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));
            Add(new OneVal(kernel, CtrlType.TabPage, "tab", null, Crlf.Nextline));

            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);

            //var l = new ListVal();
            //var key = "protocol";
            //l.Add(new OneVal(key, 0, Crlf.Nextline, new CtrlComboBox(Lang.Value(key), new[] { "TCP", "UDP" }, 100)));
            //key = "srcPort";
            //l.Add(new OneVal(key, 0, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "server";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "dstPort";
            //l.Add(new OneVal(key, 0, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //onePage.Add(new OneVal("tunnelList", null, Crlf.Nextline, new CtrlDat("", l, 380, Lang.LangKind)));

            var l = new ListVal();
            l.Add(new OneVal(kernel, CtrlType.ComboBox, "protocol", ProtocolKind.Tcp, Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.Int, "srcPort", 0, Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.TextBox, "server", "", Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.Int, "dstPort", 0, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Dat, "tunnelList", new Dat(l), Crlf.Nextline));

            return onePage;
        }

    }
}
