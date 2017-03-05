using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Configurations;
using Bjd.Net;
using Bjd.Configurations.Attributes;

namespace Bjd.WebServer.Configurations
{
    public class VirtualHostOption : SmartOption
    {

        [Dat]
        public List<hostListClass> hostList = new List<hostListClass>() { new hostListClass() };
        public class hostListClass
        {
            [ComboBox]
            public ProtocolKind protocol = ProtocolKind.Tcp;
            [TextBox(Crlf.Contonie)]
            public string host = "";
            [Int]
            public int port = 80;
        }

        [File]
        public string certificate = "";
        [Hidden]
        public string privateKeyPassword = "";
        [Group]
        public List<groupHttpsClass> groupHttps = new List<groupHttpsClass>() { new groupHttpsClass() };
        public class groupHttpsClass
        {
        }

        public override char Mnemonic { get { return 'A'; } }

        public VirtualHostOption(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {
            //var pageList = new List<OnePage>();

            //var key = "VirtualHost";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //Add(new OneVal("tab", null, Crlf.Nextline));

            //var list1 = new ListVal();
            //list1.Add(new OneVal(CtrlType.ComboBox, "protocol", ProtocolKind.Tcp, Crlf.Nextline));
            //list1.Add(new OneVal(CtrlType.TextBox, "host", "", Crlf.Contonie));
            //list1.Add(new OneVal(CtrlType.Int, "port", 80, Crlf.Nextline));
            //Add(new OneVal(CtrlType.Dat, "hostList", new Dat(list1), Crlf.Nextline));

            //var list2 = new ListVal();
            //Add(new OneVal(CtrlType.File, "certificate", "", Crlf.Nextline));
            //Add(new OneVal(CtrlType.Hidden, "privateKeyPassword", "", Crlf.Nextline, true));
            //Add(new OneVal(CtrlType.Group, "groupHttps", new Dat(list2), Crlf.Nextline));


            Read(kernel.Configuration); //　レジストリからの読み込み
        }

    }
}
