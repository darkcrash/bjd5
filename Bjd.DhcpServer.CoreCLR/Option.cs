
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;
using System.Collections.Generic;

namespace Bjd.DhcpServer
{
    class Option : OneOption
    {
        public override char Mnemonic { get { return 'H'; } }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            //var key = "useServer";
            //Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox((Lang.Value(key)))));

            //var pageList = new List<OnePage>();
            //key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //pageList.Add(Page2("Acl", "ACL(MAC)", kernel));
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            Add(new OneVal(kernel, CtrlType.CheckBox, "useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1(kernel, "Basic", Lang.Value("Basic")));
            pageList.Add(Page2(kernel, "Acl", "ACL(MAC)"));
            Add(new OneVal(kernel, CtrlType.TabPage, "tab", null, Crlf.Nextline));


            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Udp, 67, 10, 10)); //サーバ基本設定

            //var key = "leaseTime";
            //onePage.Add(new OneVal(key, 18000, Crlf.Nextline, new CtrlInt(Lang.Value(key), 8)));
            //key = "startIp";
            //onePage.Add(new OneVal(key, new Ip(IpKind.V4_0), Crlf.Nextline, new CtrlAddress(Lang.Value(key))));
            //key = "endIp";
            //onePage.Add(new OneVal(key, new Ip(IpKind.V4_0), Crlf.Nextline, new CtrlAddress(Lang.Value(key))));
            //key = "maskIp";
            //onePage.Add(new OneVal(key, new Ip("255.255.255.0"), Crlf.Nextline, new CtrlAddress(Lang.Value(key))));
            //key = "gwIp";
            //onePage.Add(new OneVal(key, new Ip(IpKind.V4_0), Crlf.Nextline, new CtrlAddress(Lang.Value(key))));
            //key = "dnsIp0";
            //onePage.Add(new OneVal(key, new Ip(IpKind.V4_0), Crlf.Nextline, new CtrlAddress(Lang.Value(key))));
            //key = "dnsIp1";
            //onePage.Add(new OneVal(key, new Ip(IpKind.V4_0), Crlf.Nextline, new CtrlAddress(Lang.Value(key))));
            //key = "useWpad";
            //onePage.Add(new OneVal(key, false, Crlf.Contonie, new CtrlCheckBox(Lang.Value(key))));
            //onePage.Add(new OneVal("wpadUrl", "http://", Crlf.Nextline, new CtrlTextBox("URL", 37)));


            CreateServerOption(ProtocolKind.Udp, 67, 10, 10);

            Add(new OneVal(kernel, CtrlType.Int, "leaseTime", 18000, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.AddressV4, "startIp", new Ip(IpKind.V4_0), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.AddressV4, "endIp", new Ip(IpKind.V4_0), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.AddressV4, "maskIp", new Ip("255.255.255.0"), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.AddressV4, "gwIp", new Ip(IpKind.V4_0), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.AddressV4, "dnsIp0", new Ip(IpKind.V4_0), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.AddressV4, "dnsIp1", new Ip(IpKind.V4_0), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useWpad", false, Crlf.Contonie));
            Add(new OneVal(kernel, CtrlType.TextBox, "wpadUrl", "http://", Crlf.Nextline));


            return onePage;
        }

        private OnePage Page2(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);

            //var key = "useMacAcl";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            //var l = new ListVal();
            //key = "macAddress";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 50)));
            //key = "v4Address";
            //l.Add(new OneVal(key, new Ip(IpKind.V4_0), Crlf.Nextline, new CtrlAddress(Lang.Value(key))));
            //key = "macName";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 50)));
            //key = "macAcl";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), l, 250, Lang.LangKind)));

            Add(new OneVal(kernel, CtrlType.CheckBox, "useMacAcl", false, Crlf.Nextline));
            var l = new ListVal();
            l.Add(new OneVal(kernel, CtrlType.TextBox, "macAddress", "", Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.AddressV4, "v4Address", new Ip(IpKind.V4_0), Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.TextBox, "macName", "", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Dat, "macAcl", new Dat(l), Crlf.Nextline));


            return onePage;
        }

    }

}
