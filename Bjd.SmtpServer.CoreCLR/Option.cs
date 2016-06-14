using System.Collections.Generic;


using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;

namespace Bjd.SmtpServer
{
    public class Option : OneOption
    {

        public override char Mnemonic
        {
            get { return 'S'; }
        }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            //var key = "useServer";
            //Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            Add(new OneVal(CtrlType.CheckBox, "useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            //key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //key = "ESMTP";
            //pageList.Add(Page2(key, Lang.Value(key), kernel));
            //key = "Relay";
            //pageList.Add(Page3(key, Lang.Value(key), kernel));
            //key = "Queue";
            //pageList.Add(Page4(key, Lang.Value(key), kernel));
            //key = "Host";
            //pageList.Add(Page5(key, Lang.Value(key), kernel));
            //key = "Heda";
            //pageList.Add(Page6(key, Lang.Value(key), kernel));
            //key = "Aliases";
            //pageList.Add(Page7(key, Lang.Value(key), kernel));
            //key = "AutoReception";
            //pageList.Add(Page8(key, Lang.Value(key), kernel));
            //pageList.Add(PageAcl());
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            pageList.Add(Page1("Basic", Lang.Value("Basic"), kernel));
            pageList.Add(Page2("ESMTP", Lang.Value("ESMTP"), kernel));
            pageList.Add(Page3("Relay", Lang.Value("Relay"), kernel));
            pageList.Add(Page4("Queue", Lang.Value("Queue"), kernel));
            pageList.Add(Page5("Host", Lang.Value("Host"), kernel));
            pageList.Add(Page6("Heda", Lang.Value("Heda"), kernel));
            pageList.Add(Page7("Aliases", Lang.Value("Aliases"), kernel));
            pageList.Add(Page8("AutoReception", Lang.Value("AutoReception"), kernel));
            pageList.Add(PageAcl());
            Add(new OneVal(CtrlType.TabPage, "tab", null, Crlf.Nextline));

            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 25, 30, 10)); //サーバ基本設定

            //var key = "domainName";
            //onePage.Add(new OneVal(key, "example.com", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 50)));
            //key = "bannerMessage";
            //onePage.Add(new OneVal(key, "$s SMTP $p $v; $d", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 50)));
            //key = "receivedHeader";
            //onePage.Add(new OneVal(key, "from $h ([$a]) by $s with SMTP id $i for <$t>; $d", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 50)));
            //key = "sizeLimit";
            //onePage.Add(new OneVal(key, 5000, Crlf.Nextline, new CtrlInt(Lang.Value(key), 8)));
            //key = "errorFrom";
            //onePage.Add(new OneVal(key, "root@local", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 50)));
            //key = "useNullFrom";
            //onePage.Add(new OneVal(key, false, Crlf.Contonie, new CtrlCheckBox(Lang.Value(key))));
            //key = "useNullDomain";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            //key = "usePopBeforeSmtp";
            //onePage.Add(new OneVal(key, false, Crlf.Contonie, new CtrlCheckBox(Lang.Value(key))));
            //key = "timePopBeforeSmtp";
            //onePage.Add(new OneVal(key, 10, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "useCheckFrom";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            CreateServerOption(ProtocolKind.Tcp, 25, 30, 10); //サーバ基本設定

            Add(new OneVal(CtrlType.TextBox, "domainName", "example.com", Crlf.Nextline));
            Add(new OneVal(CtrlType.TextBox, "bannerMessage", "$s SMTP $p $v; $d", Crlf.Nextline));
            Add(new OneVal(CtrlType.TextBox, "receivedHeader", "from $h ([$a]) by $s with SMTP id $i for <$t>; $d", Crlf.Nextline));
            Add(new OneVal(CtrlType.Int, "sizeLimit", 5000, Crlf.Nextline));
            Add(new OneVal(CtrlType.TextBox, "errorFrom", "root@local", Crlf.Nextline));
            Add(new OneVal(CtrlType.CheckBox, "useNullFrom", false, Crlf.Contonie));
            Add(new OneVal(CtrlType.CheckBox, "useNullDomain", false, Crlf.Nextline));
            Add(new OneVal(CtrlType.CheckBox, "usePopBeforeSmtp", false, Crlf.Contonie));
            Add(new OneVal(CtrlType.Int, "timePopBeforeSmtp", 10, Crlf.Nextline));
            Add(new OneVal(CtrlType.CheckBox, "useCheckFrom", false, Crlf.Nextline));

            return onePage;
        }



        private OnePage Page2(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //var key = "useEsmtp";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            //var list1 = new ListVal();
            //list1.Add(new OneVal("useAuthCramMD5", true, Crlf.Contonie, new CtrlCheckBox("CRAM-MD5")));
            //list1.Add(new OneVal("useAuthPlain", true, Crlf.Contonie, new CtrlCheckBox("PLAIN")));
            //list1.Add(new OneVal("useAuthLogin", true, Crlf.Nextline, new CtrlCheckBox("LOGIN")));
            //key = "groupAuthKind";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlGroup(Lang.Value(key), list1)));
            //key = "usePopAcount";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline,
            //                   new CtrlCheckBox(Lang.Value(key))));
            //var list2 = new ListVal();
            //key = "user";
            //list2.Add(new OneVal(key, "", Crlf.Contonie, new CtrlTextBox(Lang.Value(key), 15)));
            //key = "pass";
            //list2.Add(new OneVal(key, "", Crlf.Contonie, new CtrlHidden(Lang.Value(key), 15)));
            //key = "comment";
            //list2.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 20)));
            //key = "esmtpUserList";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), list2, 115, Lang.LangKind)));
            //key = "enableEsmtp";
            //onePage.Add(new OneVal(key, 0, Crlf.Nextline, new CtrlRadio(Lang.Value(key), new[] { Lang.Value(key + "1"), Lang.Value(key + "2") }, OptionDlg.Width() - 15, 2)));

            //var list3 = new ListVal();
            //key = "rangeName";
            //list3.Add(new OneVal(key, "", Crlf.Contonie, new CtrlTextBox(Lang.Value(key), 20)));
            //key = "rangeAddress";
            //list3.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 20)));
            //onePage.Add(new OneVal("range", null, Crlf.Nextline, new CtrlDat("", list3, 115, Lang.LangKind)));

            Add(new OneVal(CtrlType.CheckBox, "useEsmtp", false, Crlf.Nextline));
            var list1 = new ListVal();
            list1.Add(new OneVal(CtrlType.CheckBox, "useAuthCramMD5", true, Crlf.Contonie));
            list1.Add(new OneVal(CtrlType.CheckBox, "useAuthPlain", true, Crlf.Contonie));
            list1.Add(new OneVal(CtrlType.CheckBox, "useAuthLogin", true, Crlf.Nextline));
            Add(new OneVal(CtrlType.Group, "groupAuthKind", new Dat(list1), Crlf.Nextline));
            Add(new OneVal(CtrlType.CheckBox, "usePopAcount", false, Crlf.Nextline));

            var list2 = new ListVal();
            list2.Add(new OneVal(CtrlType.TextBox, "user", "", Crlf.Contonie));
            list2.Add(new OneVal(CtrlType.Hidden, "pass", "", Crlf.Contonie, true));
            list2.Add(new OneVal(CtrlType.TextBox, "comment", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "esmtpUserList", new Dat(list2), Crlf.Nextline));
            Add(new OneVal(CtrlType.Radio, "enableEsmtp", 0, Crlf.Nextline));

            var list3 = new ListVal();
            list3.Add(new OneVal(CtrlType.TextBox, "rangeName", "", Crlf.Contonie));
            list3.Add(new OneVal(CtrlType.TextBox, "rangeAddress", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "range", new Dat(list3), Crlf.Nextline));

            return onePage;
        }

        private OnePage Page3(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //var key = "order";
            //onePage.Add(new OneVal(key, 0, Crlf.Nextline, new CtrlRadio(Lang.Value(key), new[] { Lang.Value(key + "1"), Lang.Value(key + "2") }, 600, 2)));
            //var list1 = new ListVal();
            //key = "allowAddress";
            //list1.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "allowList";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), list1, 170, Lang.LangKind)));
            //var list2 = new ListVal();
            //key = "denyAddress";
            //list2.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "denyList";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), list2, 170, Lang.LangKind)));

            Add(new OneVal(CtrlType.Radio, "order", 0, Crlf.Nextline));

            var list1 = new ListVal();
            list1.Add(new OneVal(CtrlType.TextBox, "allowAddress", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "allowList", new Dat(list1), Crlf.Nextline));

            var list2 = new ListVal();
            list2.Add(new OneVal(CtrlType.TextBox, "denyAddress", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "denyList", new Dat(list2), Crlf.Nextline));

            return onePage;
        }

        private OnePage Page4(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //var key = "always";
            //onePage.Add(new OneVal(key, true, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            //key = "threadSpan";
            //onePage.Add(new OneVal(key, 300, Crlf.Nextline, new CtrlInt(Lang.Value(key), 10)));
            //key = "retryMax";
            //onePage.Add(new OneVal(key, 5, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "threadMax";
            //onePage.Add(new OneVal(key, 5, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "mxOnly";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            Add(new OneVal(CtrlType.CheckBox, "always", true, Crlf.Nextline));
            Add(new OneVal(CtrlType.Int, "threadSpan", 300, Crlf.Nextline));
            Add(new OneVal(CtrlType.Int, "retryMax", 5, Crlf.Nextline));
            Add(new OneVal(CtrlType.Int, "threadMax", 5, Crlf.Nextline));
            Add(new OneVal(CtrlType.CheckBox, "mxOnly", false, Crlf.Nextline));

            return onePage;
        }
        private OnePage Page5(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            //var key = "transferTarget";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "transferServer";
            //l.Add(new OneVal(key, "", Crlf.Contonie, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "transferPort";
            //l.Add(new OneVal(key, 25, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "transferSmtpAuth";
            //l.Add(new OneVal(key, false, Crlf.Contonie, new CtrlCheckBox(Lang.Value(key))));
            //key = "transferUser";
            //l.Add(new OneVal(key, "", Crlf.Contonie, new CtrlTextBox(Lang.Value(key), 25)));
            //key = "transferPass";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlHidden(Lang.Value(key), 25)));
            //key = "transferSsl";
            //l.Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            //onePage.Add(new OneVal("hostList", null, Crlf.Nextline, new CtrlOrgHostDat("", l, 370, Lang.LangKind)));

            l.Add(new OneVal(CtrlType.TextBox, "transferTarget", "", Crlf.Nextline));
            l.Add(new OneVal(CtrlType.TextBox, "transferServer", "", Crlf.Contonie));
            l.Add(new OneVal(CtrlType.Int, "transferPort", 25, Crlf.Nextline));
            l.Add(new OneVal(CtrlType.CheckBox, "transferSmtpAuth", false, Crlf.Contonie));
            l.Add(new OneVal(CtrlType.TextBox, "transferUser", "", Crlf.Contonie));
            l.Add(new OneVal(CtrlType.Hidden, "transferPass", "", Crlf.Nextline, true));
            l.Add(new OneVal(CtrlType.CheckBox, "transferSsl", false, Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "hostList", new Dat(l), Crlf.Nextline));

            return onePage;
        }
        private OnePage Page6(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //var list1 = new ListVal();
            //var key = "pattern";
            //list1.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 70)));
            //key = "Substitution";
            //list1.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 70)));
            //key = "patternList";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), list1, 185, Lang.LangKind)));
            //var list2 = new ListVal();
            //key = "tag";
            //list2.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "string";
            //list2.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 80)));
            //key = "appendList";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), list2, 185, Lang.LangKind)));

            var list1 = new ListVal();
            list1.Add(new OneVal(CtrlType.TextBox, "pattern", "", Crlf.Nextline));
            list1.Add(new OneVal(CtrlType.TextBox, "Substitution", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "patternList", new Dat(list1), Crlf.Nextline));

            var list2 = new ListVal();
            list2.Add(new OneVal(CtrlType.TextBox, "tag", "", Crlf.Nextline));
            list2.Add(new OneVal(CtrlType.TextBox, "string", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "appendList", new Dat(list2), Crlf.Nextline));

            return onePage;
        }
        private OnePage Page7(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //var l = new ListVal();
            //var key = "aliasUser";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "aliasName";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 80)));
            //key = "aliasList";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), l, 250, Lang.LangKind)));

            var l = new ListVal();
            l.Add(new OneVal(CtrlType.TextBox, "aliasUser", "", Crlf.Nextline));
            l.Add(new OneVal(CtrlType.TextBox, "aliasName", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "aliasList", new Dat(l), Crlf.Nextline));

            return onePage;
        }
        private OnePage Page8(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //var l = new ListVal();
            //var key = "fetchReceptionInterval";
            //l.Add(new OneVal(key, 60, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "fetchServer";
            //l.Add(new OneVal(key, "", Crlf.Contonie, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "fetchPort";
            //l.Add(new OneVal(key, 110, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "fetchUser";
            //l.Add(new OneVal(key, "", Crlf.Contonie, new CtrlTextBox(Lang.Value(key), 20)));
            //key = "fetchPass";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlHidden(Lang.Value(key), 20)));
            //key = "fetchLocalUser";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "fetchSynchronize";
            //l.Add(new OneVal(key, 0, Crlf.Contonie, new CtrlComboBox(Lang.Value(key), new[] { Lang.Value(key + "1"), Lang.Value(key + "2"), Lang.Value(key + "3") }, 180)));
            //key = "fetchTime";
            //l.Add(new OneVal(key, 0, Crlf.Nextline, new CtrlInt(Lang.Value(key), 6)));
            //onePage.Add(new OneVal("fetchList", null, Crlf.Nextline, new CtrlOrgAutoReceptionDat("", l, 370, Lang.LangKind)));

            var l = new ListVal();
            l.Add(new OneVal(CtrlType.Int, "fetchReceptionInterval", 60, Crlf.Nextline));
            l.Add(new OneVal(CtrlType.TextBox, "fetchServer", "", Crlf.Contonie));
            l.Add(new OneVal(CtrlType.Int, "fetchPort", 110, Crlf.Nextline));
            l.Add(new OneVal(CtrlType.TextBox, "fetchUser", "", Crlf.Contonie));
            l.Add(new OneVal(CtrlType.Hidden, "fetchPass", "", Crlf.Nextline, true));
            l.Add(new OneVal(CtrlType.TextBox, "fetchLocalUser", "", Crlf.Nextline));
            l.Add(new OneVal(CtrlType.ComboBox, "fetchSynchronize", 0, Crlf.Contonie));
            l.Add(new OneVal(CtrlType.Int, "fetchTime", 0, Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "fetchList", new Dat(l), Crlf.Nextline));

            return onePage;
        }

    }
}
