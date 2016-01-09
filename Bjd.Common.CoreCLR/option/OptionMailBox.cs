﻿using System.Collections.Generic;
using Bjd.ctrl;

namespace Bjd.option {
    public class OptionMailBox : OneOption {
        public override char Mnemonic { get { return 'B'; } }
       
        public OptionMailBox(Kernel kernel, string path)
            : base(kernel.IsJp(), path, "MailBox"){
            var pageList = new List<OnePage>();
            //var key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key),kernel));
            //key = "User";
            //pageList.Add(Page2(key, Lang.Value(key)));
            //Add(new OneVal("tab", null, Crlf.Nextline));

            pageList.Add(Page1("Basic", Lang.Value("Basic"), kernel));
            pageList.Add(Page2("User", Lang.Value("User")));
            Add(new OneVal("tab", null, Crlf.Nextline));

            Read(kernel.IniDb); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title,Kernel kernel){
            var onePage = new OnePage(name, title);
            //var key = "dir";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline));
            //key = "useDetailsLog";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline));

            Add(new OneVal("dir", "", Crlf.Nextline));
            Add(new OneVal("useDetailsLog", false, Crlf.Nextline));

            return onePage;
        }

        private OnePage Page2(string name, string title){
            var onePage = new OnePage(name, title);
            //var listVal = new ListVal();
            //var key = "userName";
            //listVal.Add(new OneVal(key, "", Crlf.Nextline));
            //key = "password";
            //listVal.Add(new OneVal(key, "", Crlf.Nextline));
            //key = "user";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline));

            var listVal = new ListVal();
            listVal.Add(new OneVal("userName", "", Crlf.Nextline));
            listVal.Add(new OneVal("password", "", Crlf.Nextline, true));
            Add(new OneVal("user", new Dat(ListVal), Crlf.Nextline));

            return onePage;
        }
    }
}