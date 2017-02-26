using System.Collections.Generic;


using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;
using Bjd.Options.Attributes;

namespace Bjd.SmtpServer
{
    public class SmtpOption : SmartOption
    {

        public override char Mnemonic
        {
            get { return 'S'; }
        }

        public SmtpOption(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {
            PageAcl();
            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        [CheckBox]
        public bool useServer = false;

        [ComboBox(Crlf.Contonie)]
        public ProtocolKind protocolKind = ProtocolKind.Tcp;
        [Int]
        public int port = 25;
        [BindAddr]
        public BindAddr bindAddress2 = new BindAddr();
        [CheckBox]
        public bool useResolve = false;
        [CheckBox(Crlf.Contonie)]
        public bool useDetailsLog = true;
        [Int(Crlf.Contonie)]
        public int multiple = 10;
        [Int]
        public int timeOut = 30;

        [TabPage]
        public string tab = "";

        [TextBox]
        public string domainName = "example.com";
        [TextBox]
        public string bannerMessage = "$s SMTP $p $v; $d";
        [TextBox]
        public string receivedHeader = "from $h ([$a]) by $s with SMTP id $i for <$t>; $d";
        [Int]
        public int sizeLimit = 5000;
        [TextBox]
        public string errorFrom = "root@local";
        [CheckBox(Crlf.Contonie)]
        public bool useNullFrom = false;
        [CheckBox]
        public bool useNullDomain = false;
        [CheckBox(Crlf.Contonie)]
        public bool usePopBeforeSmtp = false;
        [Int]
        public int timePopBeforeSmtp = 10;
        [CheckBox]
        public bool useCheckFrom = false;



        [CheckBox]
        public bool useEsmtp = false;
        [CheckBox(Crlf.Contonie)]
        public bool useAuthCramMD5 = true;
        [CheckBox(Crlf.Contonie)]
        public bool useAuthPlain = true;
        [CheckBox]
        public bool useAuthLogin = true;
        [Group]
        public string groupAuthKind = "";
        [CheckBox]
        public bool usePopAcount = false;

        [Dat]
        public List<esmtpUserListClass> esmtpUserList = new List<esmtpUserListClass>() { new esmtpUserListClass() };
        public class esmtpUserListClass
        {
            [TextBox(Crlf.Contonie)]
            public string user = "";
            [Hidden(Crlf.Contonie)]
            public string pass = "";
            [TextBox]
            public string comment = "";
        }

        [Radio]
        public int enableEsmtp = 0;

        [Dat]
        public List<rangeClass> range = new List<rangeClass>() { new rangeClass() };
        public class rangeClass
        {
            [TextBox(Crlf.Contonie)]
            public string rangeName = "";
            [TextBox]
            public string rangeAddress = "";
        }




        [Radio]
        public int order = 0;

        [Dat]
        public List<allowListClass> allowList = new List<allowListClass>() { new allowListClass() };
        public class allowListClass
        {
            [TextBox]
            public string allowAddress = "";
        }
        [Dat]
        public List<denyListClass> denyList = new List<denyListClass>() { new denyListClass() };
        public class denyListClass
        {
            [TextBox]
            public string denyAddress = "";
        }



        [CheckBox]
        public bool always = true;
        [Int]
        public int threadSpan = 300;
        [Int]
        public int retryMax = 5;
        [Int]
        public int threadMax = 5;
        [CheckBox]
        public bool mxOnly = false;


        [Dat]
        public List<hostListClass> hostList = new List<hostListClass>() { new hostListClass() };
        public class hostListClass
        {
            [TextBox]
            public string transferTarget = "";
            [TextBox(Crlf.Contonie)]
            public string transferServer = "";
            [Int]
            public int transferPort = 25;
            [CheckBox(Crlf.Contonie)]
            public bool transferSmtpAuth = false;
            [TextBox(Crlf.Contonie)]
            public string transferUser = "";
            [Hidden]
            public string transferPass = "";
            [CheckBox]
            public bool transferSsl = false;
        }


        [Dat]
        public List<patternListClass> patternList = new List<patternListClass>() { new patternListClass() };
        public class patternListClass
        {
            [TextBox]
            public string pattern = "";
            [TextBox]
            public string Substitution = "";
        }
        [Dat]
        public List<appendListClass> appendList = new List<appendListClass>() { new appendListClass() };
        public class appendListClass
        {
            [TextBox]
            public string tag = "";
            [TextBox]
            public string @string = "";
        }


        [Dat]
        public List<aliasListClass> aliasList = new List<aliasListClass>() { new aliasListClass() };
        public class aliasListClass
        {
            [TextBox]
            public string aliasUser = "";
            [TextBox]
            public string aliasName = "";
        }


        [Dat]
        public List<fetchListClass> fetchList = new List<fetchListClass>() { new fetchListClass() };
        public class fetchListClass
        {
            [Int]
            public int fetchReceptionInterval = 60;
            [TextBox(Crlf.Contonie)]
            public string fetchServer = "";
            [Int]
            public int fetchPort = 110;
            [TextBox(Crlf.Contonie)]
            public string fetchUser = "";
            [Hidden]
            public string fetchPass = "";
            [TextBox]
            public string fetchLocalUser = "";
            [ComboBox(Crlf.Contonie)]
            public FetchSynchronizeKind fetchSynchronize = FetchSynchronizeKind.KeepEmailOnServer;
            [Int]
            public int fetchTime = 0;
        }


    }
}
