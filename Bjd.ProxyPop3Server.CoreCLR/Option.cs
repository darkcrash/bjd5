﻿using System.Collections.Generic;
using Bjd;
using Bjd.Ctrl;
using Bjd.Net;
using Bjd.Option;

namespace Bjd.ProxyPop3Server
{
    internal class Option : OneOption
    {
        public override char Mnemonic
        {
            get { return 'P'; }
        }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel.IsJp(), path, nameTag)
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

            Add(new OneVal("useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1("Basic", Lang.Value("Basic"), kernel));
            pageList.Add(Page2("Expansion", Lang.Value("Expansion"), kernel));
            pageList.Add(PageAcl());
            Add(new OneVal("tab", null, Crlf.Nextline));


            Read(kernel.IniDb); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 8110, 60, 10)); //サーバ基本設定
            //var key = "targetPort";
            //onePage.Add(new OneVal(key, 110, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "targetServer";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "idleTime";
            //onePage.Add(new OneVal(key, 1, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));

            CreateServerOption(ProtocolKind.Tcp, 8110, 60, 10); //サーバ基本設定
            Add(new OneVal("targetPort", 110, Crlf.Nextline));
            Add(new OneVal("targetServer", "", Crlf.Nextline));
            Add(new OneVal("idleTime", 1, Crlf.Nextline));

            return onePage;
        }

        private OnePage Page2(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);

            //var l = new ListVal();
            //var key = "specialUser";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 20)));
            //key = "specialServer";
            //l.Add(new OneVal(key, "", Crlf.Contonie, new CtrlTextBox(Lang.Value(key), 20)));
            //key = "specialPort";
            //l.Add(new OneVal(key, 110, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "specialName";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 20)));
            //key = "specialUserList";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), l, 360, Lang.LangKind)));

            var l = new ListVal();
            l.Add(new OneVal("specialUser", "", Crlf.Nextline));
            l.Add(new OneVal("specialServer", "", Crlf.Contonie));
            l.Add(new OneVal("specialPort", 110, Crlf.Nextline));
            l.Add(new OneVal("specialName", "", Crlf.Nextline));
            Add(new OneVal("specialUserList", new Dat(l), Crlf.Nextline));

            return onePage;
        }

    }
}


