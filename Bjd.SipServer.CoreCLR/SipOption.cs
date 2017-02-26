using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;
using Bjd.Options.Attributes;

namespace Bjd.SipServer
{
    public class SipOption : SmartOption
    {

        //メニューに表示される文字列
        //public override string JpMenu { get { return "SIPサーバ"; } }
        //public override string EnMenu { get { return "Sip Server"; } }
        public override char Mnemonic { get { return 'Z'; } }

        public SipOption(Kernel kernel, string path, string nameTag)
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
        public int port = 5060;
        [BindAddr]
        public BindAddr bindAddress2 = new BindAddr();
        [CheckBox]
        public bool useResolve = false;
        [CheckBox(Crlf.Contonie)]
        public bool useDetailsLog = true;
        [Int(Crlf.Contonie)]
        public int multiple = 30;
        [Int]
        public int timeOut = 30;

        [TabPage]
        public string tab = "";

        [TextBox]
        public string sampleText = "Sample Server : ";

    }
}
