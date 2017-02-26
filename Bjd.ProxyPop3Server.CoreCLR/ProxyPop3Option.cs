using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;
using Bjd.Options.Attributes;

namespace Bjd.ProxyPop3Server
{
    internal class ProxyPop3Option : SmartOption
    {
        public override char Mnemonic
        {
            get { return 'P'; }
        }

        public ProxyPop3Option(Kernel kernel, string path, string nameTag)
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
        public int port = 8110;
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
        public int targetPort = 110;
        [TextBox]
        public string targetServer = "";
        [Int]
        public int idleTime = 1;



        [Dat]
        public List<specialUserListClass> specialUserList = new List<specialUserListClass>() { new specialUserListClass() };

        public class specialUserListClass
        {
            [TextBox]
            public string specialUser = "";
            [TextBox(Crlf.Contonie)]
            public string specialServer = "";
            [Int]
            public int specialPort = 110;
            [TextBox]
            public string specialName = "";
        }

    }
}


