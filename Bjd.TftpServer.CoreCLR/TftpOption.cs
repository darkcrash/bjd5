using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;
using Bjd.Options.Attributes;

namespace Bjd.TftpServer
{
    class TftpOption : SmartOption
    {

        //public override string JpMenu { get { return "TFTPサーバ"; } }
        //public override string EnMenu { get { return "TFTP Server"; } }
        public override char Mnemonic { get { return 'T'; } }

        public TftpOption(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {
            PageAcl();
            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        [CheckBox]
        public bool useServer = false;

        [ComboBox(Crlf.Contonie)]
        public ProtocolKind protocolKind = ProtocolKind.Udp;
        [Int]
        public int port = 69;
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

        [Folder]
        public string workDir = "Tftp";
        [CheckBox]
        public bool read = false;
        [CheckBox]
        public bool write = false;
        [CheckBox]
        public bool @override = false;

    }
}

