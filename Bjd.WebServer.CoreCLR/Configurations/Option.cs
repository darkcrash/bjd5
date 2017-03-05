using System;
using System.Collections.Generic;

using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Configurations;

namespace Bjd.WebServer.Configurations
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
            : base(kernel, path, nameTag)
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
            Add(new OneVal(kernel, CtrlType.CheckBox, "useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1(kernel, "Basic", Lang.Value("Basic"), protocol));
            pageList.Add(Page2(kernel, "CGI", "CGI"));
            pageList.Add(Page3(kernel, "SSI", "SSI"));
            pageList.Add(Page4(kernel, "WebDAV", "WebDAV"));
            pageList.Add(Page5(kernel, "Alias", Lang.Value("Alias")));
            pageList.Add(Page6(kernel, "MimeType", Lang.Value("MimeType")));
            pageList.Add(Page7(kernel, "Certification", Lang.Value("Certification")));
            pageList.Add(Page8(kernel, "CertUserList", Lang.Value("CertUserList")));
            pageList.Add(Page9(kernel, "CertGroupList", Lang.Value("CertGroupList")));
            pageList.Add(Page10(kernel, "ModelSentence", Lang.Value("ModelSentence")));
            pageList.Add(Page11(kernel, "AutoACL", Lang.Value("AutoACL")));
            pageList.Add(PageAcl());
            Add(new OneVal(kernel, CtrlType.TabPage, "tab", null, Crlf.Nextline));

            Read(_kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(Kernel kernel, string name, string title, int protocol)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(new OneVal("protocol", protocol, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.ComboBox, "protocol", protocol, Crlf.Nextline));

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

            Add(new OneVal(kernel, CtrlType.Folder, "documentRoot", "", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.File, "welcomeFileName", "index.html", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useHidden", false, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useDot", false, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useExpansion", false, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useDirectoryEnum", false, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.TextBox, "serverHeader", "BlackJumboDog .NET Core Version $v", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useEtag", false, Crlf.Contonie));
            Add(new OneVal(kernel, CtrlType.TextBox, "serverAdmin", "", Crlf.Contonie));

            return onePage;
        }

        private OnePage Page2(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(new OneVal("useCgi", false, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useCgi", false, Crlf.Nextline));
            {//DAT
                var l = new ListVal();
                l.Add(new OneVal(kernel, CtrlType.TextBox, "cgiExtension", "", Crlf.Contonie));
                l.Add(new OneVal(kernel, CtrlType.File, "Program", "", Crlf.Nextline));
                //onePage.Add(new OneVal("cgiCmd", "", Crlf.Nextline));
                Add(new OneVal(kernel, CtrlType.Dat, "cgiCmd", new Dat(l), Crlf.Nextline));
            }//DAT
            //onePage.Add(new OneVal("cgiTimeout", 10, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Int, "cgiTimeout", 10, Crlf.Nextline));
            {//DAT
                var l = new ListVal();
                l.Add(new OneVal(kernel, CtrlType.File, "CgiPath", "", Crlf.Nextline));
                l.Add(new OneVal(kernel, CtrlType.Folder, "cgiDirectory", "", Crlf.Nextline));
                //onePage.Add(new OneVal("cgiPath", "", Crlf.Nextline));
                Add(new OneVal(kernel, CtrlType.Dat, "cgiPath", new Dat(l), Crlf.Nextline));
            }//DAT
            return onePage;
        }

        private OnePage Page3(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(new OneVal("useSsi", false, Crlf.Nextline));
            //onePage.Add(new OneVal("ssiExt", "html,htm", Crlf.Nextline));
            //onePage.Add(new OneVal("useExec", false, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useSsi", false, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.TextBox, "ssiExt", "html,htm", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useExec", false, Crlf.Nextline));
            return onePage;
        }
        private OnePage Page4(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(new OneVal("useWebDav", false, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useWebDav", false, Crlf.Nextline));
            var l = new ListVal();
            l.Add(new OneVal(kernel, CtrlType.TextBox, "WebDAV Path", "", Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.CheckBox, "Writing permission", false, Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.Folder, "webDAVDirectory", "", Crlf.Nextline));
            //onePage.Add(new OneVal("webDavPath", new Dat(l), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Dat, "webDavPath", new Dat(l), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page5(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            l.Add(new OneVal(kernel, CtrlType.TextBox, "aliasName", "", Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.Folder, "aliasDirectory", "", Crlf.Nextline));
            //onePage.Add(new OneVal("aliaseList", new Dat(l), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Dat, "aliaseList", new Dat(l), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page6(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            l.Add(new OneVal(kernel, CtrlType.TextBox, "mimeExtension", "", Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.TextBox, "mimeType", "", Crlf.Nextline));
            //onePage.Add(new OneVal("mime", new Dat(l), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Dat, "mime", new Dat(l), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page7(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            l.Add(new OneVal(kernel, CtrlType.TextBox, "authDirectory", "", Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.TextBox, "AuthName", "", Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.TextBox, "Require", "", Crlf.Nextline));
            //onePage.Add(new OneVal("authList", new Dat(l), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Dat, "authList", new Dat(l), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page8(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            l.Add(new OneVal(kernel, CtrlType.TextBox, "user", "", Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.Hidden, "pass", "", Crlf.Nextline));
            //onePage.Add(new OneVal("userList", new Dat(l), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Dat, "userList", new Dat(l), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page9(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            l.Add(new OneVal(kernel, CtrlType.TextBox, "group", "", Crlf.Nextline));
            l.Add(new OneVal(kernel, CtrlType.TextBox, "userName", "", Crlf.Nextline));
            //onePage.Add(new OneVal("groupList", new Dat(new CtrlType[] { }), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.TextBox, "groupList", new Dat(l), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page10(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(new OneVal("encode", 0, Crlf.Nextline));
            //onePage.Add(new OneVal("indexDocument", "", Crlf.Nextline));
            //onePage.Add(new OneVal("errorDocument", "", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.ComboBox, "encode", "utf-8", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Memo, "indexDocument", "", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Memo, "errorDocument", "", Crlf.Nextline));
            return onePage;
        }
        private OnePage Page11(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(new OneVal("useAutoAcl", false, Crlf.Nextline));
            //onePage.Add(new OneVal("autoAclLabel", Lang.Value("autoAclLabel" + "1"), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useAutoAcl", false, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Label, "autoAclLabel", Lang.Value("autoAclLabel" + "1"), Crlf.Nextline));
            var l = new ListVal();
            Add(new OneVal(kernel, CtrlType.CheckBox, "AutoAclApacheKiller", false, Crlf.Nextline));
            //onePage.Add(new OneVal("autoAclGroup", new Dat(l), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Group, "autoAclGroup", new Dat(l), Crlf.Nextline));
            return onePage;
        }

    }
}
