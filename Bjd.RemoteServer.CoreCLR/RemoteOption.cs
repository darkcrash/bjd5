using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Configurations.Attributes;

namespace Bjd.RemoteServer
{
    class RemoteOption : SmartOption
    {
        public override char Mnemonic { get { return 'R'; } }

        public RemoteOption(Kernel kernel, string path, string nameTag)
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
        public int port = 10001;
        [BindAddr]
        public BindAddr bindAddress2 = new BindAddr();
        [CheckBox]
        public bool useResolve = false;
        [CheckBox(Crlf.Contonie)]
        public bool useDetailsLog = true;
        [Int(Crlf.Contonie)]
        public int multiple = 1;
        [Int]
        public int timeOut = 60;

        [TabPage]
        public string tab = "";

        [Hidden]
        public string password = "";

    }
}
