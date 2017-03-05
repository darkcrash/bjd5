using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Configurations.Attributes;

namespace Bjd.FtpServer.Configurations
{
    public class FtpOption : ConfigurationSmart
    {

        //public override string JpMenu { get { return "FTPサーバ"; } }
        //public override string EnMenu { get { return "FTP Server"; } }
        public override char Mnemonic { get { return 'F'; } }


        public FtpOption(Kernel kernel, string path, string nameTag)
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
        public int port = 21;
        [BindAddr]
        public BindAddr bindAddress2 = new BindAddr();
        [CheckBox]
        public bool useResolve = false;
        [CheckBox(Crlf.Contonie)]
        public bool useDetailsLog = true;
        [Int(Crlf.Contonie)]
        public int multiple = 50;
        [Int]
        public int timeOut = 30;


        [TextBox]
        public string bannerMessage = "FTP ( $p Version $v ) ready";

        [CheckBox]
        public bool useSyst = false;

        [Int]
        public int reservationTime = 5000;

        [Dat]
        public List<mountListClass> mountList = new List<mountListClass>() { new mountListClass() };

        public class mountListClass
        {
            [Folder]
            public string fromFolder = "";

            [Folder]
            public string toFolder = "";
        }


        [Dat]
        public List<userClass> user = new List<userClass>() { new userClass() };

        public class userClass
        {
            [ComboBox]
            public FtpAcl accessControl = FtpAcl.Full;
            [Folder]
            public string homeDirectory = "";
            [TextBox]
            public string userName = "";
            [Hidden]
            public string password = "";
        }


    }
}
