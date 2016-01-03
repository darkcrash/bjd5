using System.Collections.Generic;
using Bjd;
using Bjd.ctrl;
using Bjd.option;

namespace BJD.SmtpServer
{
    class OptionOneMl : OneOption {
        public override string MenuStr
        {
            get { return NameTag; }
        }
        public override char Mnemonic { get { return '0'; } }

        public OptionOneMl(Kernel kernel, string path, string nameTag)
            : base(kernel.IsJp(), path, nameTag){
            var pageList = new List<OnePage>();

            //var key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key),kernel));
            //key = "Member";
            //pageList.Add(Page2(key, Lang.Value(key),kernel));
            //pageList.Add(Page3("Guide", "Guide", kernel));
            //pageList.Add(Page4("Deny", "Deny", kernel));
            //pageList.Add(Page5("Confirm", "Confirm", kernel));
            //pageList.Add(Page6("Welcome", "Welcome", kernel));
            //pageList.Add(Page7("Append", "Append", kernel));
            //pageList.Add(Page8("Help", "Help", kernel));
            //pageList.Add(Page9("Admin", "Admin", kernel));
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            pageList.Add(Page1("Basic", Lang.Value("Basic"), kernel));
            pageList.Add(Page2("Member", Lang.Value("Member"), kernel));
            pageList.Add(Page3("Guide", "Guide", kernel));
            pageList.Add(Page4("Deny", "Deny", kernel));
            pageList.Add(Page5("Confirm", "Confirm", kernel));
            pageList.Add(Page6("Welcome", "Welcome", kernel));
            pageList.Add(Page7("Append", "Append", kernel));
            pageList.Add(Page8("Help", "Help", kernel));
            pageList.Add(Page9("Admin", "Admin", kernel));
            Add(new OneVal("tab", null, Crlf.Nextline));


            Read(kernel.IniDb); //　レジストリからの読み込み
        }


        private OnePage Page1(string name, string title, Kernel kernel) {
            var onePage = new OnePage(name, title);
            //var key = "manageDir";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlFolder(Lang.Value(key),80, kernel)));
            //key = "useDetailsLog";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            //key = "title";
            //onePage.Add(new OneVal(key, 5, Crlf.Nextline, new CtrlComboBox(Lang.Value(key), new[] { "(NAME)", "[NAME]", "(00000)", "[00000]", "(NAME:00000)", "[NAME:00000]", "none" }, 100)));
            //key = "maxGet";
            //onePage.Add(new OneVal(key, 10, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "maxSummary";
            //onePage.Add(new OneVal(key, 100, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "autoRegistration";
            //onePage.Add(new OneVal(key, true, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            Add(new OneVal("manageDir", "", Crlf.Nextline));
            Add(new OneVal("useDetailsLog", false, Crlf.Nextline));
            Add(new OneVal("title", 5, Crlf.Nextline));
            Add(new OneVal("maxGet", 10, Crlf.Nextline));
            Add(new OneVal("maxSummary", 100, Crlf.Nextline));
            Add(new OneVal("autoRegistration", true, Crlf.Nextline));

            return onePage;
        }
        private OnePage Page2(string name, string title, Kernel kernel) {
            var onePage = new OnePage(name, title);
            //var l = new ListVal();
            //var key = "name";
            //l.Add(new OneVal(key, "", Crlf.Contonie, new CtrlTextBox(Lang.Value(key), 20)));
            //key = "address";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 20)));
            //key = "manager";
            //l.Add(new OneVal(key, false, Crlf.Contonie, new CtrlCheckBox(Lang.Value(key))));
            //key = "reacer";
            //l.Add(new OneVal(key, true, Crlf.Contonie, new CtrlCheckBox(Lang.Value(key))));
            //key = "contributor";
            //l.Add(new OneVal(key, true, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            //key = "pass";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlHidden(Lang.Value(key), 10)));
            //key = "memberList";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlOrgMemberDat(Lang.Value(key), l, 390,Lang.LangKind)));

            var l = new ListVal();
            l.Add(new OneVal("name", "", Crlf.Contonie));
            l.Add(new OneVal("address", "", Crlf.Nextline));
            l.Add(new OneVal("manager", false, Crlf.Contonie));
            l.Add(new OneVal("reacer", true, Crlf.Contonie));
            l.Add(new OneVal("contributor", true, Crlf.Nextline));
            l.Add(new OneVal("pass", "", Crlf.Nextline, true));
            Add(new OneVal("memberList", new Dat(l), Crlf.Nextline));

            return onePage;
        }
        private OnePage Page3(string name, string title, Kernel kernel) {
            var onePage = new OnePage(name, title);
            //var key = "guideDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal("guideDocument", "", Crlf.Nextline));

            return onePage;
        }
        private OnePage Page4(string name, string title, Kernel kernel) {
            var onePage = new OnePage(name, title);
            //var key = "denyDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal("denyDocument", "", Crlf.Nextline));

            return onePage;
        }
        private OnePage Page5(string name, string title, Kernel kernel) {
            var onePage = new OnePage(name, title);
            //var key = "confirmDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal("confirmDocument", "", Crlf.Nextline));

            return onePage;
        }
        private OnePage Page6(string name, string title, Kernel kernel) {
            var onePage = new OnePage(name, title);
            //var key = "welcomeDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal("welcomeDocument", "", Crlf.Nextline));

            return onePage;
        }
        private OnePage Page7(string name, string title, Kernel kernel) {
            var onePage = new OnePage(name, title);
            //var key = "appendDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal("appendDocument", "", Crlf.Nextline));

            return onePage;
        }
        private OnePage Page8(string name, string title, Kernel kernel) {
            var onePage = new OnePage(name, title);
            //var key = "helpDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal("helpDocument", "", Crlf.Nextline));

            return onePage;
        }
        private OnePage Page9(string name, string title, Kernel kernel) {
            var onePage = new OnePage(name, title);
            //var key = "adminDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal("adminDocument", "", Crlf.Nextline));

            return onePage;
        }
    }
}
