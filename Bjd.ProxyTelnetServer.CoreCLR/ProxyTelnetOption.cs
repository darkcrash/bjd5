using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;
using Bjd.Options.Attributes;
using System.Collections.Generic;

namespace Bjd.ProxyTelnetServer
{
    class ProxyTelnetOption : SmartOption
    {

        public override char Mnemonic { get { return 'T'; } }

        public ProxyTelnetOption(Kernel kernel, string path, string nameTag)
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
        public int port = 8023;
        [BindAddr]
        public BindAddr bindAddress2 = new BindAddr();
        [CheckBox]
        public bool useResolve = false;
        [CheckBox(Crlf.Contonie)]
        public bool useDetailsLog = true;
        [Int(Crlf.Contonie)]
        public int multiple = 10;
        [Int]
        public int timeOut = 60;

        [TabPage]
        public string tab = "";

        [Int]
        public int idleTime = 1;

    }
}
