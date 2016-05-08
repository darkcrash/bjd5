
using System.Collections.Generic;
using Bjd;
using Bjd.Ctrl;
using Bjd.Net;
using Bjd.Option;

namespace Bjd.DnsServer
{
    class Option : OneOption
    {
        //public override string JpMenu { get { return "DNSサーバ"; } }
        //public override string EnMenu { get { return "DNS Server"; } }
        public override char Mnemonic { get { return 'D'; } }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel.IsJp(), path, nameTag)
        {

            //var key = "useServer";
            //Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            Add(new OneVal("useServer", false, Crlf.Nextline));
            var pageList = new List<OnePage>();
            //key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            pageList.Add(Page1("Basic", Lang.Value("Basic"), kernel));
            pageList.Add(PageAcl());
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));
            Add(new OneVal("tab", null, Crlf.Nextline));

            Read(kernel.IniDb); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Udp, 53, 10, 30)); //サーバ基本設定
            CreateServerOption(ProtocolKind.Udp, 53, 10, 30); //サーバ基本設定

            //var key = "rootCache";
            //onePage.Add(new OneVal(key, "named.ca", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            Add(new OneVal("rootCache", "named.ca", Crlf.Nextline));
            //key = "useRD";
            //onePage.Add(new OneVal(key, true, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            Add(new OneVal("useRD", true, Crlf.Nextline));

            var list = new ListVal();
            //key = "soaMail";
            //list.Add(new OneVal(key, "postmaster", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "soaSerial";
            //list.Add(new OneVal(key, 1, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "soaRefresh";
            //list.Add(new OneVal(key, 3600, Crlf.Contonie, new CtrlInt(Lang.Value(key), 5)));
            //key = "soaRetry";
            //list.Add(new OneVal(key, 300, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "soaExpire";
            //list.Add(new OneVal(key, 360000, Crlf.Contonie, new CtrlInt(Lang.Value(key), 5)));
            //key = "soaMinimum";
            //list.Add(new OneVal(key, 3600, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "GroupSoa";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlGroup(Lang.Value(key), list)));
            Add(new OneVal("soaMail", "postmaster", Crlf.Nextline));
            Add(new OneVal("soaSerial", 1, Crlf.Nextline));
            Add(new OneVal("soaRefresh", 3600, Crlf.Contonie));
            Add(new OneVal("soaRetry", 300, Crlf.Nextline));
            Add(new OneVal("soaExpire", 360000, Crlf.Contonie));
            Add(new OneVal("soaMinimum", 3600, Crlf.Nextline));
            Add(new OneVal("GroupSoa", new Dat(list), Crlf.Nextline));

            return onePage;
        }

    }
}
