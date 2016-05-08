using System.Collections.Generic;

using Bjd;
using Bjd.Ctrl;
using Bjd.Net;
using Bjd.Option;

namespace Bjd.ProxyHttpServer
{
    class Option : OneOption
    {

        public override char Mnemonic { get { return 'B'; } }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel.IsJp(), path, nameTag)
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

            Add(new OneVal("useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1("Basic", Lang.Value("Basic"), kernel));
            pageList.Add(Page2("HigherProxy", Lang.Value("HigherProxy"), kernel));
            pageList.Add(Page3("Cache1", Lang.Value("Cache1"), kernel));
            pageList.Add(Page4("Cache2", Lang.Value("Cache2"), kernel));
            pageList.Add(Page5("LimitUrl", Lang.Value("LimitUrl"), kernel));
            pageList.Add(Page6("LimitContents", Lang.Value("LimitContents"), kernel));
            pageList.Add(PageAcl());
            Add(new OneVal("tab", null, Crlf.Nextline));


            Read(kernel.IniDb); //　レジストリからの読み込み
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

            Add(new OneVal("useRequestLog", false, Crlf.Nextline));

            var list1 = new ListVal();
            list1.Add(new OneVal("anonymousAddress", "BlackJumboDog@", Crlf.Nextline));
            list1.Add(new OneVal("serverHeader", "BlackJumboDog Version $v", Crlf.Nextline));
            Add(new OneVal("anonymousFtp", new Dat(list1), Crlf.Nextline));

            Add(new OneVal("useBrowserHedaer", false, Crlf.Nextline));

            var list2 = new ListVal();
            Add(new OneVal("addHeaderRemoteHost", false, Crlf.Contonie));
            Add(new OneVal("addHeaderXForwardedFor", false, Crlf.Contonie));
            Add(new OneVal("addHeaderForwarded", false, Crlf.Nextline));
            Add(new OneVal("groupAddHeader", new Dat(list2), Crlf.Nextline));


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

            Add(new OneVal("useUpperProxy", false, Crlf.Nextline));
            Add(new OneVal("upperProxyServer", "", Crlf.Contonie));
            Add(new OneVal("upperProxyPort", 8080, Crlf.Nextline));
            Add(new OneVal("upperProxyUseAuth", false, Crlf.Contonie));
            Add(new OneVal("upperProxyAuthName", "", Crlf.Contonie));
            Add(new OneVal("upperProxyAuthPass", "", Crlf.Nextline, true));

            var list2 = new ListVal();
            list2.Add(new OneVal("address", "", Crlf.Nextline));
            Add(new OneVal("disableAddress", new Dat(list2), Crlf.Nextline));

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

            Add(new OneVal("useCache", false, Crlf.Nextline));
            Add(new OneVal("cacheDir", "", Crlf.Nextline));
            Add(new OneVal("testTime", 3, Crlf.Nextline));
            Add(new OneVal("memorySize", 1000, Crlf.Nextline));
            Add(new OneVal("diskSize", 5000, Crlf.Nextline));
            Add(new OneVal("expires", 24, Crlf.Nextline));
            Add(new OneVal("maxSize", 1200, Crlf.Nextline));

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

            Add(new OneVal("enableHost", 1, Crlf.Nextline));

            var list1 = new ListVal();
            list1.Add(new OneVal("host", "", Crlf.Nextline));
            Add(new OneVal("cacheHost", new Dat(list1), Crlf.Nextline));
            Add(new OneVal("enableExt", 1, Crlf.Nextline));

            var list2 = new ListVal();
            list2.Add(new OneVal("ext", "", Crlf.Nextline));
            Add(new OneVal("cacheExt", new Dat(list2), Crlf.Nextline));


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
            list1.Add(new OneVal("allowUrl", "", Crlf.Contonie));
            list1.Add(new OneVal("allowMatching", 0, Crlf.Nextline));
            Add(new OneVal("limitUrlAllow", new Dat(list1), Crlf.Nextline));

            var list2 = new ListVal();
            list2.Add(new OneVal("denyUrl", "", Crlf.Contonie));
            list2.Add(new OneVal("denyMatching", 0, Crlf.Nextline));
            Add(new OneVal("limitUrlDeny", new Dat(list2), Crlf.Nextline));

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
            l.Add(new OneVal("string", "", Crlf.Nextline));
            Add(new OneVal("limitString", new Dat(l), Crlf.Nextline));

            return onePage;
        }

    }
}
