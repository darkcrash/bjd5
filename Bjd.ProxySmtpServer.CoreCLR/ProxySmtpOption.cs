using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Configurations.Attributes;

namespace Bjd.ProxySmtpServer
{
    class ProxySmtpOption : SmartOption
    {

        public override char Mnemonic { get { return 'S'; } }

        public ProxySmtpOption(Kernel kernel, string path, string nameTag)
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
        public int port = 8025;
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
        public int targetPort = 25;
        [TextBox]
        public string targetServer = "";
        [Int]
        public int idleTime = 1;


        [Dat]
        public List<specialUserClass> specialUser = new List<specialUserClass>() { new specialUserClass() };

        public class specialUserClass
        {
            [TextBox]
            public string mail = "";
            [TextBox(Crlf.Contonie)]
            public string server = "";
            [Int]
            public int dstPort = 25;
            [TextBox]
            public string address = "";
        }

    }
}



