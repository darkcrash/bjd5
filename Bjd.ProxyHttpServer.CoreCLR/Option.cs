using System.Collections.Generic;

using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;

namespace Bjd.ProxyHttpServer
{
    class Option : OneOption
    {

        public override char Mnemonic { get { return 'B'; } }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            //var key = "useServer";
            //Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            //var pageList = new List<OnePage>();
            //key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //key = "HigherProxy";
            //pageList.Add(Page2(key, Lang.Value(key), kernel));
            //key = "Cache1";
            //pageList.Add(Page3(key, Lang.Value(key), kernel));
            //key = "Cache2";
            //pageList.Add(Page4(key, Lang.Value(key), kernel));
            //key = "LimitUrl";
            //pageList.Add(Page5(key, Lang.Value(key), kernel));
            //key = "LimitContents";
            //pageList.Add(Page6(key, Lang.Value(key), kernel));
            //pageList.Add(PageAcl());
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            Add(new OneVal(CtrlType.CheckBox, "useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1("Basic", Lang.Value("Basic"), kernel));
            pageList.Add(Page2("HigherProxy", Lang.Value("HigherProxy"), kernel));
            pageList.Add(Page3("Cache1", Lang.Value("Cache1"), kernel));
            pageList.Add(Page4("Cache2", Lang.Value("Cache2"), kernel));
            pageList.Add(Page5("LimitUrl", Lang.Value("LimitUrl"), kernel));
            pageList.Add(Page6("LimitContents", Lang.Value("LimitContents"), kernel));
            pageList.Add(PageAcl());
            Add(new OneVal(CtrlType.TabPage, "tab", null, Crlf.Nextline));


            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 8080, 60, 300)); //サーバ基本設定

            //var key = "useRequestLog";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            //var list1 = new ListVal();
            //key = "anonymousAddress";
            //list1.Add(new OneVal(key, "BlackJumboDog@", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 70)));
            //key = "serverHeader";
            //list1.Add(new OneVal(key, "BlackJumboDog Version $v", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 70)));
            //key = "anonymousFtp";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlGroup(Lang.Value(key), list1)));

            //key = "useBrowserHedaer";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            //var list2 = new ListVal();
            //list2.Add(new OneVal("addHeaderRemoteHost", false, Crlf.Contonie, new CtrlCheckBox("Remote-Host:")));
            //list2.Add(new OneVal("addHeaderXForwardedFor", false, Crlf.Contonie, new CtrlCheckBox("X-Forwarded-For:")));
            //list2.Add(new OneVal("addHeaderForwarded", false, Crlf.Nextline, new CtrlCheckBox("Forwarded:")));
            //key = "groupAddHeader";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlGroup(Lang.Value(key), list2)));


            CreateServerOption(ProtocolKind.Tcp, 8080, 60, 300); //サーバ基本設定

            Add(new OneVal(CtrlType.CheckBox, "useRequestLog", false, Crlf.Nextline));

            var list1 = new ListVal();
            Add(new OneVal(CtrlType.TextBox, "anonymousAddress", "BlackJumboDog@", Crlf.Nextline));
            Add(new OneVal(CtrlType.TextBox, "serverHeader", "BlackJumboDog .NET Core Version $v", Crlf.Nextline));
            Add(new OneVal(CtrlType.Group, "anonymousFtp", new Dat(list1), Crlf.Nextline));

            Add(new OneVal(CtrlType.CheckBox, "useBrowserHedaer", false, Crlf.Nextline));

            var list2 = new ListVal();
            Add(new OneVal(CtrlType.CheckBox, "addHeaderRemoteHost", false, Crlf.Contonie));
            Add(new OneVal(CtrlType.CheckBox, "addHeaderXForwardedFor", false, Crlf.Contonie));
            Add(new OneVal(CtrlType.CheckBox, "addHeaderForwarded", false, Crlf.Nextline));
            Add(new OneVal(CtrlType.Group, "groupAddHeader", new Dat(list2), Crlf.Nextline));


            return onePage;
        }

        private OnePage Page2(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //var key = "useUpperProxy";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline,
            //                       new CtrlCheckBox(Lang.Value(key))));
            //key = "upperProxyServer";
            //onePage.Add(new OneVal(key, "", Crlf.Contonie, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "upperProxyPort";
            //onePage.Add(new OneVal(key, 8080, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "upperProxyUseAuth";
            //onePage.Add(new OneVal(key, false, Crlf.Contonie,
            //                     new CtrlCheckBox(Lang.Value(key))));
            //key = "upperProxyAuthName";
            //onePage.Add(new OneVal(key, "", Crlf.Contonie, new CtrlTextBox(Lang.Value(key), 20)));
            //key = "upperProxyAuthPass";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlHidden(Lang.Value(key), 20)));

            //var list2 = new ListVal();

            //key = "address";
            //list2.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));

            //key = "disableAddress";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), list2, 200, Lang.LangKind)));

            Add(new OneVal(CtrlType.CheckBox, "useUpperProxy", false, Crlf.Nextline));
            Add(new OneVal(CtrlType.TextBox, "upperProxyServer", "", Crlf.Contonie));
            Add(new OneVal(CtrlType.Int, "upperProxyPort", 8080, Crlf.Nextline));
            Add(new OneVal(CtrlType.CheckBox, "upperProxyUseAuth", false, Crlf.Contonie));
            Add(new OneVal(CtrlType.TextBox, "upperProxyAuthName", "", Crlf.Contonie));
            Add(new OneVal(CtrlType.Hidden, "upperProxyAuthPass", "", Crlf.Nextline, true));

            var list2 = new ListVal();
            list2.Add(new OneVal(CtrlType.TextBox, "address", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "disableAddress", new Dat(list2), Crlf.Nextline));

            return onePage;
        }

        private OnePage Page3(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //var key = "useCache";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            //key = "cacheDir";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlFolder(Lang.Value(key), 60, kernel)));
            //key = "testTime";
            //onePage.Add(new OneVal(key, 3, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "memorySize";
            //onePage.Add(new OneVal(key, 1000, Crlf.Nextline, new CtrlInt(Lang.Value(key), 10)));
            //key = "diskSize";
            //onePage.Add(new OneVal(key, 5000, Crlf.Nextline, new CtrlInt(Lang.Value(key), 10)));
            //key = "expires";
            //onePage.Add(new OneVal(key, 24, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "maxSize";
            //onePage.Add(new OneVal(key, 1200, Crlf.Nextline, new CtrlInt(Lang.Value(key), 10)));

            Add(new OneVal(CtrlType.CheckBox, "useCache", false, Crlf.Nextline));
            Add(new OneVal(CtrlType.Folder, "cacheDir", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Int, "testTime", 3, Crlf.Nextline));
            Add(new OneVal(CtrlType.Int, "memorySize", 1000, Crlf.Nextline));
            Add(new OneVal(CtrlType.Int, "diskSize", 5000, Crlf.Nextline));
            Add(new OneVal(CtrlType.Int, "expires", 24, Crlf.Nextline));
            Add(new OneVal(CtrlType.Int, "maxSize", 1200, Crlf.Nextline));

            return onePage;
        }
        private OnePage Page4(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //var key = "enableHost";
            //onePage.Add(new OneVal(key, 1, Crlf.Nextline, new CtrlRadio(Lang.Value(key), new[] { Lang.Value(key + "1"), Lang.Value(key + "2") }, 600, 2)));
            //var list1 = new ListVal();
            //key = "host";
            //list1.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 30)));
            //key = "cacheHost";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), list1, 135, Lang.LangKind)));
            //key = "enableExt";
            //onePage.Add(new OneVal(key, 1, Crlf.Nextline, new CtrlRadio(Lang.Value(key), new[] { Lang.Value(key + "1"), Lang.Value(key + "2") }, 600, 2)));
            //var list2 = new ListVal();
            //key = "ext";
            //list2.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 10)));
            //key = "cacheExt";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), list2, 135, Lang.LangKind)));

            Add(new OneVal(CtrlType.Radio, "enableHost", 1, Crlf.Nextline));

            var list1 = new ListVal();
            list1.Add(new OneVal(CtrlType.TextBox, "host", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "cacheHost", new Dat(list1), Crlf.Nextline));
            Add(new OneVal(CtrlType.Radio, "enableExt", 1, Crlf.Nextline));

            var list2 = new ListVal();
            list2.Add(new OneVal(CtrlType.TextBox, "ext", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "cacheExt", new Dat(list2), Crlf.Nextline));


            return onePage;
        }

        private OnePage Page5(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //var list1 = new ListVal();
            //list1.Add(new OneVal("allowUrl", "", Crlf.Contonie, new CtrlTextBox("URL", 30)));
            //var key = "allowMatching";
            //list1.Add(new OneVal(key, 0, Crlf.Nextline, new CtrlComboBox(Lang.Value(key), new[] { Lang.Value(key + "1"), Lang.Value(key + "2"), Lang.Value(key + "3"), Lang.Value(key + "4") }, 100)));
            //key = "limitUrlAllow";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), list1, 185, Lang.LangKind)));
            //var list2 = new ListVal();
            //list2.Add(new OneVal("denyUrl", "", Crlf.Contonie, new CtrlTextBox("URL", 30)));
            //key = "denyMatching";
            //list2.Add(new OneVal(key, 0, Crlf.Nextline, new CtrlComboBox(Lang.Value(key), new[] { Lang.Value(key + "1"), Lang.Value(key + "2"), Lang.Value(key + "3"), Lang.Value(key + "4") }, 100)));
            //key = "limitUrlDeny";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), list2, 185, Lang.LangKind)));

            var list1 = new ListVal();
            list1.Add(new OneVal(CtrlType.TextBox, "allowUrl", "", Crlf.Contonie));
            list1.Add(new OneVal(CtrlType.ComboBox, "allowMatching", Matching.beginsWithMatch, Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "limitUrlAllow", new Dat(list1), Crlf.Nextline));

            var list2 = new ListVal();
            list2.Add(new OneVal(CtrlType.TextBox, "denyUrl", "", Crlf.Contonie));
            list2.Add(new OneVal(CtrlType.ComboBox, "denyMatching", Matching.beginsWithMatch, Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "limitUrlDeny", new Dat(list2), Crlf.Nextline));

            return onePage;
        }
        private OnePage Page6(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //var l = new ListVal();
            //var key = "string";
            //l.Add(new OneVal(key, "", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 50)));
            //key = "limitString";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlDat(Lang.Value(key), l, 300, Lang.LangKind)));

            var l = new ListVal();
            l.Add(new OneVal(CtrlType.TextBox, "string", "", Crlf.Nextline));
            Add(new OneVal(CtrlType.Dat, "limitString", new Dat(l), Crlf.Nextline));

            return onePage;
        }

    }
}
