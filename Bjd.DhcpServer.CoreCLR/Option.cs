
using Bjd;
using Bjd.ctrl;
using Bjd.net;
using Bjd.option;
using System.Collections.Generic;

namespace Bjd.DhcpServer
{
    class Option : OneOption
    {
        public override char Mnemonic { get { return 'H'; } }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel.IsJp(), path, nameTag)
        {

            //var key = "useServer";
            //Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox((Lang.Value(key)))));

            //var pageList = new List<OnePage>();
            //key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //pageList.Add(Page2("Acl", "ACL(MAC)", kernel));
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            Add(new OneVal("useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1("Basic", Lang.Value("Basic"), kernel));
            pageList.Add(Page2("Acl", "ACL(MAC)", kernel));
            Add(new OneVal("tab", null, Crlf.Nextline));


            Read(kernel.IniDb); //�@���W�X�g������̓ǂݍ���
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Udp, 67, 10, 10)); //�T�[�o��{�ݒ�

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

            Add(new OneVal("leaseTime", 18000, Crlf.Nextline));
            Add(new OneVal("startIp", new Ip(IpKind.V4_0), Crlf.Nextline));
            Add(new OneVal("endIp", new Ip(IpKind.V4_0), Crlf.Nextline));
            Add(new OneVal("maskIp", new Ip("255.255.255.0"), Crlf.Nextline));
            Add(new OneVal("gwIp", new Ip(IpKind.V4_0), Crlf.Nextline));
            Add(new OneVal("dnsIp0", new Ip(IpKind.V4_0), Crlf.Nextline));
            Add(new OneVal("dnsIp1", new Ip(IpKind.V4_0), Crlf.Nextline));
            Add(new OneVal("useWpad", false, Crlf.Contonie));
            Add(new OneVal("wpadUrl", "http://", Crlf.Nextline));


            return onePage;
        }

        private OnePage Page2(string name, string title, Kernel kernel)
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

            Add(new OneVal("useMacAcl", false, Crlf.Nextline));
            var l = new ListVal();
            l.Add(new OneVal("macAddress", "", Crlf.Nextline));
            l.Add(new OneVal("v4Address", new Ip(IpKind.V4_0), Crlf.Nextline));
            l.Add(new OneVal("macName", "", Crlf.Nextline));
            Add(new OneVal("macAcl", new Dat(l), Crlf.Nextline));


            return onePage;
        }

    }

}
