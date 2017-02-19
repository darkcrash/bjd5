﻿using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Options;
using Bjd.Net;

namespace Bjd.WebServer.Configurations
{
    public class OptionVirtualHost : OneOption
    {
        //public override string JpMenu { get { return "Webの追加と削除"; } }
        //public override string EnMenu { get { return "Add or Remove VirtualHost"; } }
        public override char Mnemonic { get { return 'A'; } }

        public OptionVirtualHost(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            //var pageList = new List<OnePage>();

            //var key = "VirtualHost";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //Add(new OneVal("tab", null, Crlf.Nextline));

            var list1 = new ListVal();
            list1.Add(new OneVal(kernel, CtrlType.ComboBox, "protocol", ProtocolKind.Tcp, Crlf.Nextline));
            list1.Add(new OneVal(kernel, CtrlType.TextBox, "host", "", Crlf.Contonie));
            list1.Add(new OneVal(kernel, CtrlType.Int, "port", 80, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Dat, "hostList", new Dat(list1), Crlf.Nextline));

            var list2 = new ListVal();
            Add(new OneVal(kernel, CtrlType.File, "certificate", "", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Hidden, "privateKeyPassword", "", Crlf.Nextline, true));
            Add(new OneVal(kernel, CtrlType.Group, "groupHttps", new Dat(list2), Crlf.Nextline));


            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            //var onePage = new OnePage(name, title);
            //var list1 = new ListVal();
            //var key = "protocol";
            //list1.Add(new OneVal(key, 0, Crlf.Nextline));
            //key = "host";
            //list1.Add(new OneVal(key, "", Crlf.Contonie));
            //key = "port";
            //list1.Add(new OneVal(key, 80, Crlf.Nextline));
            //onePage.Add(new OneVal("hostList", null, Crlf.Nextline));
            //var list2 = new ListVal();
            //key = "certificate";
            //list2.Add(new OneVal(key, "", Crlf.Nextline));
            //key = "privateKeyPassword";
            //list2.Add(new OneVal(key, "", Crlf.Nextline));
            //key = "groupHttps";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline));

            //return onePage;
            return null;
        }

    }
}
