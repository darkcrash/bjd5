using System;
using System.Collections.Generic;

using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;
using Bjd.Options.Attributes;

namespace Bjd.WebServer.Configurations
{
    public class WebServerOption : SmartOption
    {
        [TabPage]
        public string tab = null;

        [TabPage]
        public string Basic = null;
        [TabPage]
        public string CGI = "CGI";
        [TabPage]
        public string SSI = "SSI";
        [TabPage]
        public string WebDAV = "WebDAV";
        [TabPage]
        public string Alias = "Alias";
        [TabPage]
        public string MimeType = "MimeType";
        [TabPage]
        public string Certification = "Certification";
        [TabPage]
        public string CertUserList = "CertUserList";
        [TabPage]
        public string CertGroupList = "CertGroupList";
        [TabPage]
        public string ModelSentence = "ModelSentence";
        [TabPage]
        public string AutoACL = "AutoACL";

        [CheckBox]
        public bool useServer = false;


        public override char Mnemonic { get { return '0'; } }


        public WebServerOption(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {
            //nameTagからポート番号を取得しセットする（変更不可）
            var tmp = NameTag.Split(':');
            if (tmp.Length == 2)
            {
                port = Convert.ToInt32(tmp[1]);
                protocol = (port == 443) ? 1 : 0;
            }

            PageAcl();

            Basic = Lang.Value("Basic");
            Alias = Lang.Value("Alias");
            MimeType = Lang.Value("MimeType");
            Certification = Lang.Value("Certification");
            CertUserList = Lang.Value("CertUserList");
            CertGroupList = Lang.Value("CertGroupList");
            ModelSentence = Lang.Value("ModelSentence");
            AutoACL = Lang.Value("AutoACL");

            autoAclLabel = Lang.Value("autoAclLabel" + "1");


            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        [ComboBox]
        public int protocol = 0;

        [ComboBox(Crlf.Contonie)]
        public ProtocolKind protocolKind = ProtocolKind.Tcp;
        [Int]
        public int port = 80;
        [BindAddr]
        public BindAddr bindAddress2 = new BindAddr();
        [CheckBox]
        public bool useResolve = false;
        [CheckBox(Crlf.Contonie)]
        public bool useDetailsLog = true;
        [Int(Crlf.Contonie)]
        public int multiple = 10;
        [Int]
        public int timeOut = 3;

        [Folder]
        public string documentRoot = "";
        [File]
        public string welcomeFileName = "index.html";
        [CheckBox]
        public bool useHidden = false;
        [CheckBox]
        public bool useDot = false;
        [CheckBox]
        public bool useExpansion = false;
        [CheckBox]
        public bool useDirectoryEnum = false;
        [TextBox]
        public string serverHeader = "BlackJumboDog .NET Core Version $v";
        [CheckBox(Crlf.Contonie)]
        public bool useEtag = false;
        [TextBox(Crlf.Contonie)]
        public string serverAdmin = "";


        [CheckBox]
        public bool useCgi = false;
        [Dat]
        public List<cgiCmdClass> cgiCmd = new List<cgiCmdClass>() { new cgiCmdClass() };
        public class cgiCmdClass
        {
            [TextBox(Crlf.Contonie)]
            public string cgiExtension = "";
            [File]
            public string Program = "";
        }
        [Int]
        public int cgiTimeout = 10;
        [Dat]
        public List<cgiPathClass> cgiPath = new List<cgiPathClass>() { new cgiPathClass() };
        public class cgiPathClass
        {
            [File]
            public string CgiPath = "";
            [Folder]
            public string cgiDirectory = "";
        }


        [CheckBox]
        public bool useSsi = false;
        [TextBox]
        public string ssiExt = "shtml,shtm";
        [CheckBox]
        public bool useExec = false;


        [CheckBox]
        public bool useWebDav = false;
        [Dat]
        public List<webDavPathClass> webDavPath = new List<webDavPathClass>() { new webDavPathClass() };
        public class webDavPathClass
        {
            [TextBox]
            public string Path = "";
            [CheckBox]
            public bool Writing = false;
            [Folder]
            public string webDAVDirectory = "";
        }

        [Dat]
        public List<aliaseListClass> aliaseList = new List<aliaseListClass>() { new aliaseListClass() };
        public class aliaseListClass
        {
            [TextBox]
            public string aliasName = "";
            [Folder]
            public string aliasDirectory = "";
        }

        [Dat]
        public List<mimeClass> mime = new List<mimeClass>() { new mimeClass() };
        public class mimeClass
        {
            [TextBox]
            public string mimeExtension = "";
            [TextBox]
            public string mimeType = "";
        }

        [Dat]
        public List<authListClass> authList = new List<authListClass>() { new authListClass() };
        public class authListClass
        {
            [TextBox]
            public string authDirectory = "";
            [TextBox]
            public string AuthName = "";
            [TextBox]
            public string Require = "";
        }

        [Dat]
        public List<userListClass> userList = new List<userListClass>() { new userListClass() };
        public class userListClass
        {
            [TextBox]
            public string user = "";
            [Hidden]
            public string pass = "";
        }

        [Dat]
        public List<groupListClass> groupList = new List<groupListClass>() { new groupListClass() };
        public class groupListClass
        {
            [TextBox]
            public string group = "";
            [Hidden]
            public string userName = "";
        }

        [ComboBox]
        public int encode = 0;
        [Memo]
        public string indexDocument = "";
        [Memo]
        public string errorDocument = "";


        [CheckBox]
        public bool useAutoAcl = false;
        [Label]
        public string autoAclLabel = "";
        [Dat]
        public List<autoAclGroupClass> autoAclGroup = new List<autoAclGroupClass>() { new autoAclGroupClass() };
        public class autoAclGroupClass
        {
            [CheckBox]
            public bool AutoAclApacheKiller = false;
        }



    }
}
