﻿using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;

namespace Bjd.ProxySmtpServer
{
    class Option : OneOption
    {

        public override char Mnemonic { get { return 'S'; } }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            //var key = "useServer";
            //Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            //var pageList = new List<OnePage>();
            //key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //key = "Expansion";
            //pageList.Add(Page2(key, Lang.Value(key), kernel));
            //pageList.Add(PageAcl());
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            Add(new OneVal(CtrlType.CheckBox, "useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1("Basic", Lang.Value("Basic"), kernel));
            pageList.Add(Page2("Expansion", Lang.Value("Expansion"), kernel));
            pageList.Add(PageAcl());
            Add(new OneVal(CtrlType.TabPage, "tab", null, Crlf.Nextline));


            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 8025, 60, 10)); //サーバ基本設定
            //var key = "targetPort";
            //onePage.Add(new OneVal(key, 25, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "targetServer";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 50)));
            //key = "idleTime";
            //onePage.Add(new OneVal(key, 1, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));

            CreateServerOption(ProtocolKind.Tcp, 8025, 60, 10); //サーバ基本設定
            Add(new OneVal(CtrlType.Int, "targetPort", 25, Crlf.Nextline));
            Add(new OneVal(CtrlType.TextBox, "targetServer", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Int, "idleTime", 1, Crlf.Nextline));

            return onePage;
        }
        private OnePage Page2(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //var l = new ListVal();
            //var key = "mail";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "server";
            //l.Add(new OneVal(key, "", Crlf.Contonie, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "dstPort";
            //l.Add(new OneVal(key, 25, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "address";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "specialUser";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), l, 360, Lang.LangKind)));

            var l = new ListVal();
            l.Add(new OneVal(CtrlType.TextBox, "mail", "", Crlf.Nextline));
            l.Add(new OneVal(CtrlType.TextBox, "server", "", Crlf.Contonie));
            l.Add(new OneVal(CtrlType.Int, "dstPort", 25, Crlf.Nextline));
            l.Add(new OneVal(CtrlType.TextBox, "address", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "specialUser", new Dat(l), Crlf.Nextline));

            return onePage;
        }


    }
}



