using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Configurations.Attributes;

namespace Bjd.ProxyFtpServer
{
    class ProxyFtpOption : SmartOption
    {

        public override char Mnemonic { get { return 'F'; } }


        public ProxyFtpOption(Kernel kernel, string path, string nameTag)
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
        public int port = 8021;
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
