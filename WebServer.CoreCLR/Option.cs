using System;
using System.Collections.Generic;

using Bjd;
using Bjd.ctrl;
using Bjd.net;
using Bjd.option;

namespace WebServer
{
    public class Option : OneOption
    {

        public override string MenuStr
        {
            get { return NameTag; }
        }

        public override char Mnemonic { get { return '0'; } }

        private Kernel _kernel; //����Web�̏d������o���邽�ߕK�v�ƂȂ�

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel.IsJp(), path, nameTag)
        {

            _kernel = kernel;

            var protocol = 0;//HTTP
            //nameTag����|�[�g�ԍ���擾���Z�b�g����i�ύX�s�j
            var tmp = NameTag.Split(':');
            if (tmp.Length == 2)
            {
                int port = Convert.ToInt32(tmp[1]);
                protocol = (port == 443) ? 1 : 0;
            }
            var key = "useServer";
            Add(new OneVal(key, false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            key = "Basic";
            pageList.Add(Page1(key, Lang.Value(key), kernel, protocol));
            pageList.Add(Page2("CGI", "CGI", kernel));
            pageList.Add(Page3("SSI", "SSI", kernel));
            pageList.Add(Page4("WebDAV", "WebDAV", kernel));
            key = "Alias";
            pageList.Add(Page5(key, Lang.Value(key), kernel));
            key = "MimeType";
            pageList.Add(Page6(key, Lang.Value(key), kernel));
            key = "Certification";
            pageList.Add(Page7(key, Lang.Value(key), kernel));
            key = "CertUserList";
            pageList.Add(Page8(key, Lang.Value(key), kernel));
            key = "CertGroupList";
            pageList.Add(Page9(key, Lang.Value(key), kernel));
            key = "ModelSentence";
            pageList.Add(Page10(key, Lang.Value(key), kernel));
            key = "AutoACL";
            pageList.Add(Page11(key, Lang.Value(key), kernel));
            pageList.Add(PageAcl());
            Add(new OneVal("tab", null, Crlf.Nextline));

            Read(_kernel.IniDb); //�@���W�X�g������̓ǂݍ���
        }

        private OnePage Page1(string name, string title, Kernel kernel, int protocol)
        {
            var onePage = new OnePage(name, title);

            var key = "protocol";
            onePage.Add(new OneVal(key, protocol, Crlf.Nextline));

            var port = 80;
            //nameTag����|�[�g�ԍ���擾���Z�b�g����i�ύX�s�j
            var tmp = NameTag.Split(':');
            if (tmp.Length == 2)
            {
                port = Convert.ToInt32(tmp[1]);
            }
            onePage.Add(CreateServerOption(ProtocolKind.Tcp, port, 3, 10)); //�T�[�o��{�ݒ�

            key = "documentRoot";
            onePage.Add(new OneVal(key, "", Crlf.Nextline));
            key = "welcomeFileName";
            onePage.Add(new OneVal(key, "index.html", Crlf.Nextline));
            key = "useHidden";
            onePage.Add(new OneVal(key, false, Crlf.Nextline));
            key = "useDot";
            onePage.Add(new OneVal(key, false, Crlf.Nextline));
            key = "useExpansion";
            onePage.Add(new OneVal(key, false, Crlf.Nextline));
            key = "useDirectoryEnum";
            onePage.Add(new OneVal(key, false, Crlf.Nextline));
            key = "serverHeader";
            onePage.Add(new OneVal(key, "BlackJumboDog Version $v", Crlf.Nextline));
            key = "useEtag";
            onePage.Add(new OneVal(key, false, Crlf.Contonie));
            key = "serverAdmin";
            onePage.Add(new OneVal(key, "", Crlf.Contonie));

            return onePage;
        }

        private OnePage Page2(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var key = "useCgi";
            onePage.Add(new OneVal(key, false, Crlf.Nextline));
            {//DAT
                var l = new ListVal();
                key = "cgiExtension";
                l.Add(new OneVal(key, "", Crlf.Contonie));
                key = "Program";
                l.Add(new OneVal(key, "", Crlf.Nextline));
                onePage.Add(new OneVal("cgiCmd", "", Crlf.Nextline));
            }//DAT
            key = "cgiTimeout";
            onePage.Add(new OneVal(key, 10, Crlf.Nextline));
            {//DAT
                var l = new ListVal();
                key = "CgiPath";
                l.Add(new OneVal(key, "", Crlf.Nextline));
                key = "cgiDirectory";
                l.Add(new OneVal(key, "", Crlf.Nextline));
                key = "cgiPath";
                onePage.Add(new OneVal(key, "", Crlf.Nextline));
            }//DAT
            return onePage;
        }

        private OnePage Page3(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var key = "useSsi";
            onePage.Add(new OneVal(key, false, Crlf.Nextline));
            key = "ssiExt";
            onePage.Add(new OneVal(key, "html,htm", Crlf.Nextline));
            key = "useExec";
            onePage.Add(new OneVal(key, false, Crlf.Nextline));
            return onePage;
        }
        private OnePage Page4(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var key = "useWebDav";
            onePage.Add(new OneVal(key, false, Crlf.Nextline));
            var l = new ListVal();
            key = "WebDAV Path";
            l.Add(new OneVal(key, "", Crlf.Nextline));
            key = "Writing permission";
            l.Add(new OneVal(key, false, Crlf.Nextline));
            key = "webDAVDirectory";
            l.Add(new OneVal(key, "", Crlf.Nextline));
            key = "webDavPath";
            onePage.Add(new OneVal(key, new Dat(new CtrlType[] { }), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page5(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            var key = "aliasName";
            l.Add(new OneVal(key, "", Crlf.Nextline));
            key = "aliasDirectory";
            l.Add(new OneVal(key, "", Crlf.Nextline));
            key = "aliaseList";
            onePage.Add(new OneVal(key, new Dat(new CtrlType[] { }), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page6(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            var key = "mimeExtension";
            l.Add(new OneVal(key, "", Crlf.Nextline));
            key = "mimeType";
            l.Add(new OneVal(key, "", Crlf.Nextline));
            key = "mime";
            onePage.Add(new OneVal(key, new Dat(new CtrlType[] { }), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page7(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            var key = "authDirectory";
            l.Add(new OneVal(key, "", Crlf.Nextline));
            key = "AuthName";
            l.Add(new OneVal(key, "", Crlf.Nextline));
            key = "Require";
            l.Add(new OneVal(key, "", Crlf.Nextline));
            key = "authList";
            onePage.Add(new OneVal(key, new Dat(new CtrlType[] { }), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page8(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            var key = "user";
            l.Add(new OneVal(key, "", Crlf.Nextline));
            key = "pass";
            l.Add(new OneVal(key, "", Crlf.Nextline));
            key = "userList";
            onePage.Add(new OneVal(key, new Dat(new CtrlType[] { }), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page9(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var l = new ListVal();
            var key = "group";
            l.Add(new OneVal(key, "", Crlf.Nextline));
            key = "userName";
            l.Add(new OneVal(key, "", Crlf.Nextline));
            key = "groupList";
            onePage.Add(new OneVal(key, new Dat(new CtrlType[] { }), Crlf.Nextline));
            return onePage;
        }
        private OnePage Page10(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var key = "encode";
            onePage.Add(new OneVal(key, 0, Crlf.Nextline));
            key = "indexDocument";
            onePage.Add(new OneVal(key, "", Crlf.Nextline));
            key = "errorDocument";
            onePage.Add(new OneVal(key, "", Crlf.Nextline));
            return onePage;
        }
        private OnePage Page11(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var key = "useAutoAcl";
            onePage.Add(new OneVal(key, false, Crlf.Nextline));
            key = "autoAclLabel";
            onePage.Add(new OneVal(key, Lang.Value(key + "1"), Crlf.Nextline));
            var l = new ListVal();
            key = "AutoAclApacheKiller";
            l.Add(new OneVal(key, false, Crlf.Nextline));
            key = "autoAclGroup";
            onePage.Add(new OneVal(key, null, Crlf.Nextline));
            return onePage;
        }

    }
}
