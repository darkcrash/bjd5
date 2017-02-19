﻿using System.Collections.Generic;
using Bjd.Controls;

namespace Bjd.Options
{
    public class OptionMailBox : OneOption
    {
        public override char Mnemonic { get { return 'B'; } }

        public OptionMailBox(Kernel kernel, string path)
            : base(kernel, path, "MailBox")
        {
            var pageList = new List<OnePage>();
            //var key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key),kernel));
            //key = "User";
            //pageList.Add(Page2(key, Lang.Value(key)));
            //Add(new OneVal("tab", null, Crlf.Nextline));

            pageList.Add(Page1(kernel, "Basic", Lang.Value("Basic")));
            pageList.Add(Page2(kernel, "User", Lang.Value("User")));
            //Add(new OneVal("tab", null, Crlf.Nextline));

            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //var key = "dir";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline));
            //key = "useDetailsLog";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline));

            Add(new OneVal(kernel, CtrlType.Folder, "dir", "", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useDetailsLog", false, Crlf.Nextline));

            return onePage;
        }

        private OnePage Page2(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //var listVal = new ListVal();
            //var key = "userName";
            //listVal.Add(new OneVal(key, "", Crlf.Nextline));
            //key = "password";
            //listVal.Add(new OneVal(key, "", Crlf.Nextline));
            //key = "user";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline));

            var listVal = new ListVal();
            listVal.Add(new OneVal(kernel, CtrlType.TextBox, "userName", "", Crlf.Nextline));
            listVal.Add(new OneVal(kernel, CtrlType.TextBox, "password", "", Crlf.Nextline, true));
            Add(new OneVal(kernel, CtrlType.Dat, "user", new Dat(listVal), Crlf.Nextline));

            return onePage;
        }
    }
}
