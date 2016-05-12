using System;
using System.Collections.Generic;

using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;

namespace Bjd.WebServer
{
    public class Option : OneOption
    {

        public override string MenuStr
        {
            get { return NameTag; }
        }

        public override char Mnemonic { get { return '0'; } }

        private Kernel _kernel; //仮装Webの重複を検出するため必要となる

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel.IsJp, path, nameTag)
        {

            _kernel = kernel;

            var protocol = 0;//HTTP
            //nameTagからポート番号を取得しセットする（変更不可）
            var tmp = NameTag.Split(':');
            if (tmp.Length == 2)
            {
                int port = Convert.ToInt32(tmp[1]);
                protocol = (port == 443) ? 1 : 0;
            }
            Add(new OneVal("useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1("Basic", Lang.Value("Basic"), kernel, protocol));
            pageList.Add(Page2("CGI", "CGI", kernel));
            pageList.Add(Page3("SSI", "SSI", kernel));
            pageList.Add(Page4("WebDAV", "WebDAV", kernel));
            pageList.Add(Page5("Alias", Lang.Value("Alias"), kernel));
            pageList.Add(Page6("MimeType", Lang.Value("MimeType"), kernel));
            pageList.Add(Page7("Certification", Lang.Value("Certification"), kernel));
            pageList.Add(Page8("CertUserList", Lang.Value("CertUserList"), kernel));
            pageList.Add(Page9("CertGroupList", Lang.Value("CertGroupList"), kernel));
            pageList.Add(Page10("ModelSentence", Lang.Value("ModelSentence"), kernel));
            pageList.Add(Page11("AutoACL", Lang.Value("AutoACL"), kernel));
            pageList.Add(PageAcl());
            Add(new OneVal("tab", null, Crlf.Nextline));

            Read(_kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel, int protocol)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(new OneVal("protocol", protocol, Crlf.Nextline));
            Add(new OneVal("protocol", protocol, Crlf.Nextline));

            var port = 80;
            //nameTagからポート番号を取得しセットする（変更不可）
            var tmp = NameTag.Split(':');
            if (tmp.Length == 2)
            {
                port = Convert.ToInt32(tmp[1]);
            }
            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, port, 3, 10)); //サーバ基本設定
            CreateServerOption(ProtocolKind.Tcp, port, 3, 10);

            //onePage.Add(new OneVal("documentRoot", "", Crlf.Nextline));
            //onePage.Add(new OneVal("welcomeFileName", "index.html", Crlf.Nextline));
            //onePage.Add(new OneVal("useHidden", false, Crlf.Nextline));
            //onePage.Add(new OneVal("useDot", false, Crlf.Nextline));
            //onePage.Add(new OneVal("useExpansion", false, Crlf.Nextline));
            //onePage.Add(new OneVal("useDirectoryEnum", false, Crlf.Nextline));
            //onePage.Add(new OneVal("serverHeader", "BlackJumboDog Version $v", Crlf.Nextline));
            //onePage.Add(new OneVal("useEtag", false, Crlf.Contonie));
            //onePage.Add(new OneVal("serverAdmin", "", Crlf.Contonie));

            Add(new OneVal("documentRoot", "", Crlf.Nextline));
            Add(new OneVal("welcomeFileName", "index.html", Crlf.Nextline));
            Add(new OneVal("useHidden", false, Crlf.Nextline));
            Add(new OneVal("useDot", false, Crlf.Nextline));
            Add(new OneVal("useExpansion", false, Crlf.Nextline));
            Add(new OneVal("useDirectoryEnum", false, Crlf.Nextline));
            Add(new OneVal("serverHeader", "BlackJumboDog Version $v", Crlf.Nextline));
            Add(new OneVal("useEtag", false, Crlf.Contonie));
            Add(new OneVal("serverAdmin", "", Crlf.Contonie));

            return onePage;
        }

        private OnePage Page2(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(new OneVal("useCgi", false, Crlf.Nextline));
            Add(new OneVal("useCgi", false, Crlf.Nextline));
            {//DAT
                var l = new ListVal();
                l.Add(new OneVal("cgiExtension", "", Crlf.Contonie));
                l.Add(new OneVal("Program", "", Crlf.Nextline));
                //onePage.Add(new OneVal("cgiCmd", "", Crlf.Nextline));
                Add(new OneVal("cgiCmd", new Dat(l), Crlf.Nextline));
            }//DAT
            //onePage.Add(new OneVal("cgiTimeout", 10, Crlf.Nextline));
            Add(new OneVal("cgiTimeout", 10, Crlf.Nextline));
            {//DAT
                var l = new ListVal();
                l.Add(new OneVal("CgiPath", "", Crlf.Nextline));
                l.Add(new OneVal("cgiDirectory", "", Crlf.Nextline));
                //onePage.Add(new OneVal("cgiPath", "", Crlf.Nextline));
                Add(new OneVal("cgiPath", new Dat(l), Crlf.Nextline));
            }//DAT
            return onePage;
        }

        private OnePage Page3(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(new OneVal("useSsi", false, Crlf.Nextline));
            //onePage.Add(new OneVal("ssiExt", "html,htm", Crlf.Nextline));
            //onePage.Add(new OneVal("useExec", false, Crlf.Nextline));
            Add(new OneVal("useSsi", false, Crlf.Nextline));
            Add(new OneVal("ssiExt", "html,htm", Crlf.Nextline));
            Add(new OneVal("useExec", false, Crlf.Nextline));
            return onePage;
        }
        private OnePage Page4(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(new OneVal("useWebDav", false, Crlf.Nextline));
            Add(new OneVal("useWebDav", false, Crlf.Nextline));
            var l = new ListVal();
            l.Add(new OneVal("WebDAV Path", "", Crlf.Nextline));
            l.Add(new OneVal("Writing permission", false, Crlf.Nextline));
            l.Add(new OneVal("webDAVDirectory", "", Crlf.Nextline));
            //onePage.Add(new OneVal("webDavPath", new Dat(l), Crlf.Nextline));
            Add(new OneVal("webDavPath", new Dat(l), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page5(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            l.Add(new OneVal("aliasName", "", Crlf.Nextline));
            l.Add(new OneVal("aliasDirectory", "", Crlf.Nextline));
            //onePage.Add(new OneVal("aliaseList", new Dat(l), Crlf.Nextline));
            Add(new OneVal("aliaseList", new Dat(l), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page6(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            l.Add(new OneVal("mimeExtension", "", Crlf.Nextline));
            l.Add(new OneVal("mimeType", "", Crlf.Nextline));
            //onePage.Add(new OneVal("mime", new Dat(l), Crlf.Nextline));
            Add(new OneVal("mime", new Dat(l), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page7(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            l.Add(new OneVal("authDirectory", "", Crlf.Nextline));
            l.Add(new OneVal("AuthName", "", Crlf.Nextline));
            l.Add(new OneVal("Require", "", Crlf.Nextline));
            //onePage.Add(new OneVal("authList", new Dat(l), Crlf.Nextline));
            Add(new OneVal("authList", new Dat(l), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page8(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            l.Add(new OneVal("user", "", Crlf.Nextline));
            l.Add(new OneVal("pass", "", Crlf.Nextline));
            //onePage.Add(new OneVal("userList", new Dat(l), Crlf.Nextline));
            Add(new OneVal("userList", new Dat(l), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page9(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            l.Add(new OneVal("group", "", Crlf.Nextline));
            l.Add(new OneVal("userName", "", Crlf.Nextline));
            //onePage.Add(new OneVal("groupList", new Dat(new CtrlType[] { }), Crlf.Nextline));
            Add(new OneVal("groupList", new Dat(l), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page10(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(new OneVal("encode", 0, Crlf.Nextline));
            //onePage.Add(new OneVal("indexDocument", "", Crlf.Nextline));
            //onePage.Add(new OneVal("errorDocument", "", Crlf.Nextline));
            Add(new OneVal("encode", "utf-8", Crlf.Nextline));
            Add(new OneVal("indexDocument", "", Crlf.Nextline));
            Add(new OneVal("errorDocument", "", Crlf.Nextline));
            return onePage;
        }
        private OnePage Page11(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(new OneVal("useAutoAcl", false, Crlf.Nextline));
            //onePage.Add(new OneVal("autoAclLabel", Lang.Value("autoAclLabel" + "1"), Crlf.Nextline));
            Add(new OneVal("useAutoAcl", false, Crlf.Nextline));
            Add(new OneVal("autoAclLabel", Lang.Value("autoAclLabel" + "1"), Crlf.Nextline));
            var l = new ListVal();
            l.Add(new OneVal("AutoAclApacheKiller", false, Crlf.Nextline));
            //onePage.Add(new OneVal("autoAclGroup", new Dat(l), Crlf.Nextline));
            Add(new OneVal("autoAclGroup", new Dat(l), Crlf.Nextline));
            return onePage;
        }

    }
}
