
using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Configurations.Attributes;

namespace Bjd.DnsServer
{
    class DnsOption : SmartOption
    {
        public override char Mnemonic { get { return 'D'; } }

        [CheckBox]
        public bool useServer = false;

        public DnsOption(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            PageAcl();

            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        [ComboBox(Crlf.Contonie)]
        public ProtocolKind protocolKind = ProtocolKind.Udp;
        [Int]
        public int port = 53;
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
        public string rootCache = "named.ca";
        [CheckBox]
        public bool useRD = true;
        [TextBox]
        public string soaMail = "postmaster";
        [Int]
        public int soaSerial = 1;
        [Int(Crlf.Contonie)]
        public int soaRefresh = 3600;
        [Int]
        public int soaRetry = 300;
        [Int(Crlf.Contonie)]
        public int soaExpire = 360000;
        [Int]
        public int soaMinimum = 3600;
        [Group]
        public string GroupSoa = "";



    }
}
