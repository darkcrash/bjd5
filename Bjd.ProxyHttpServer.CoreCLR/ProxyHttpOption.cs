using System.Collections.Generic;

using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;
using Bjd.Options.Attributes;

namespace Bjd.ProxyHttpServer
{
    class ProxyHttpOption : SmartOption
    {

        public override char Mnemonic { get { return 'B'; } }

        public ProxyHttpOption(Kernel kernel, string path, string nameTag)
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
        public int port = 8080;
        [BindAddr]
        public BindAddr bindAddress2 = new BindAddr();
        [CheckBox]
        public bool useResolve = false;
        [CheckBox(Crlf.Contonie)]
        public bool useDetailsLog = true;
        [Int(Crlf.Contonie)]
        public int multiple = 300;
        [Int]
        public int timeOut = 60;


        [TabPage]
        public string tab = "";



        [CheckBox]
        public bool useRequestLog = false;
        [TextBox]
        public string anonymousAddress = "BlackJumboDog@";
        [TextBox]
        public string serverHeader = "BlackJumboDog .NET Core Version $v";
        [Group]
        public string anonymousFtp = "";

        [CheckBox]
        public bool useBrowserHedaer = false;
        [CheckBox(Crlf.Contonie)]
        public bool addHeaderRemoteHost = false;
        [CheckBox(Crlf.Contonie)]
        public bool addHeaderXForwardedFor = false;
        [CheckBox]
        public bool addHeaderForwarded = false;
        [Group]
        public string groupAddHeader = "";


        [CheckBox]
        public bool useUpperProxy = false;
        [TextBox(Crlf.Contonie)]
        public string upperProxyServer = "";
        [Int]
        public int upperProxyPort = 8080;
        [CheckBox]
        public bool upperProxyUseAuth = false;
        [TextBox(Crlf.Contonie)]
        public string upperProxyAuthName = "";
        [Hidden]
        public string upperProxyAuthPass = "";

        [Dat]
        public List<disableAddressClass> disableAddress = new List<disableAddressClass>() { new disableAddressClass() };

        public class disableAddressClass
        {
            [TextBox]
            public string address = "";
        }


        [CheckBox]
        public bool useCache = false;
        [Folder]
        public string cacheDir = "";
        [Int]
        public int testTime = 3;
        [Int]
        public int memorySize = 1000;
        [Int]
        public int diskSize = 5000;
        [Int]
        public int expires = 24;
        [Int]
        public int maxSize = 1200;



        [Radio]
        public int enableHost = 1;

        [Dat]
        public List<hostClass> cacheHost = new List<hostClass>() { new hostClass() };

        public class hostClass
        {
            [TextBox]
            public string host = "";
        }

        [Radio]
        public int enableExt = 1;

        [Dat]
        public List<cacheExtClass> cacheExt = new List<cacheExtClass>() { new cacheExtClass() };

        public class cacheExtClass
        {
            [TextBox]
            public string ext = "";
        }


        [Dat]
        public List<limitUrlAllowClass> limitUrlAllow = new List<limitUrlAllowClass>() { new limitUrlAllowClass() };
        public class limitUrlAllowClass
        {
            [TextBox(Crlf.Contonie)]
            public string allowUrl = "";
            [ComboBox]
            public Matching allowMatching = Matching.beginsWithMatch;
        }
        [Dat]
        public List<limitUrlDenyClass> limitUrlDeny = new List<limitUrlDenyClass>() { new limitUrlDenyClass() };
        public class limitUrlDenyClass
        {
            [TextBox(Crlf.Contonie)]
            public string denyUrl = "";
            [ComboBox]
            public Matching denyMatching = Matching.beginsWithMatch;
        }

        [Dat]
        public List<limitStringClass> limitString = new List<limitStringClass>() { new limitStringClass() };
        public class limitStringClass
        {
            [TextBox]
            public string @string = "";
        }


    }
}
