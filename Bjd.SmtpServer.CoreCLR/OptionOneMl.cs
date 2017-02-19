using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Options;

namespace Bjd.SmtpServer
{
    class OptionOneMl : OneOption
    {
        public override string MenuStr
        {
            get { return NameTag; }
        }
        public override char Mnemonic { get { return '0'; } }

        public OptionOneMl(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {
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

            pageList.Add(Page1(kernel, "Basic", Lang.Value("Basic")));
            pageList.Add(Page2(kernel, "Member", Lang.Value("Member")));
            pageList.Add(Page3(kernel, "Guide", "Guide"));
            pageList.Add(Page4(kernel, "Deny", "Deny"));
            pageList.Add(Page5(kernel, "Confirm", "Confirm"));
            pageList.Add(Page6(kernel, "Welcome", "Welcome"));
            pageList.Add(Page7(kernel, "Append", "Append"));
            pageList.Add(Page8(kernel, "Help", "Help"));
            pageList.Add(Page9(kernel, "Admin", "Admin"));
            Add(new OneVal(kernel, CtrlType.TabPage, "tab", null, Crlf.Nextline));


            Read(kernel.Configuration); //　レジストリからの読み込み
        }


        private OnePage Page1(Kernel kernel, string name, string title)
        {
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

            Add(new OneVal(kernel, CtrlType.Folder, "manageDir", "", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useDetailsLog", false, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.ComboBox, "title", 5, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Int, "maxGet", 10, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Int, "maxSummary", 100, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "autoRegistration", true, Crlf.Nextline));

            return onePage;
        }
        private OnePage Page2(Kernel kernel, string name, string title)
        {
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
            l.Add(new OneVal(kernel, CtrlType.TextBox, "name", "", Crlf.Contonie));
            l.Add(new OneVal(kernel, CtrlType.TextBox, "address", "", Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.CheckBox, "manager", false, Crlf.Contonie));
            l.Add(new OneVal(kernel, CtrlType.CheckBox, "reacer", true, Crlf.Contonie));
            l.Add(new OneVal(kernel, CtrlType.CheckBox, "contributor", true, Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.Hidden, "pass", "", Crlf.Nextline, true));
            Add(new OneVal(kernel, CtrlType.Dat, "memberList", new Dat(l), Crlf.Nextline));

            return onePage;
        }
        private OnePage Page3(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //var key = "guideDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal(kernel, CtrlType.Memo, "guideDocument", "", Crlf.Nextline));

            return onePage;
        }
        private OnePage Page4(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //var key = "denyDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal(kernel, CtrlType.Memo, "denyDocument", "", Crlf.Nextline));

            return onePage;
        }
        private OnePage Page5(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //var key = "confirmDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal(kernel, CtrlType.Memo, "confirmDocument", "", Crlf.Nextline));

            return onePage;
        }
        private OnePage Page6(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //var key = "welcomeDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal(kernel, CtrlType.Memo, "welcomeDocument", "", Crlf.Nextline));

            return onePage;
        }
        private OnePage Page7(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //var key = "appendDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal(kernel, CtrlType.Memo, "appendDocument", "", Crlf.Nextline));

            return onePage;
        }
        private OnePage Page8(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //var key = "helpDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal(kernel, CtrlType.Memo, "helpDocument", "", Crlf.Nextline));

            return onePage;
        }
        private OnePage Page9(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //var key = "adminDocument";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlMemo(Lang.Value(key), 600, 360)));

            Add(new OneVal(kernel, CtrlType.Memo, "adminDocument", "", Crlf.Nextline));

            return onePage;
        }
    }
}
