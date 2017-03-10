using System.Collections.Generic;

using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Configurations.Attributes;

namespace Bjd.Pop3Server.Configurations
{
    public class Pop3Option : ConfigurationSmart
    {

        public override char Mnemonic { get { return 'P'; } }

        public Pop3Option(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {
            PageAcl();
            Read(kernel.Configuration);
        }

        [CheckBox]
        public bool useServer = false;

        [ComboBox(Crlf.Contonie)]
        public ProtocolKind protocolKind = ProtocolKind.Tcp;
        [Int]
        public int port = 110;
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



        [TextBox]
        public string bannerMessage = "$p (Version $v) ready";
        [Radio]
        public int authType = 0;
        [Int]
        public int authTimeout = 30;


        [CheckBox]
        public bool useChps = false;
        [Int]
        public int minimumLength = 8;
        [CheckBox]
        public bool disableJoe = true;
        [CheckBox(Crlf.Contonie)]
        public bool useNum = true;
        [CheckBox(Crlf.Contonie)]
        public bool useSmall = true;
        [CheckBox(Crlf.Contonie)]
        public bool useLarge = true;

        [CheckBox]
        public bool useSign = true;
        [Group]
        public string groupNeed = "";

        [CheckBox]
        public bool useAutoAcl = false;
        [Label]
        public string autoAclLabel;
        [Int(Crlf.Contonie)]
        public int autoAclMax = 5;
        [Int]
        public int autoAclSec = 60;

    }
}
