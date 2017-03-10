
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Configurations.Attributes;
using System.Collections.Generic;

namespace Bjd.DhcpServer.Configurations
{
    class Dhcp : ConfigurationSmart
    {
        public override char Mnemonic { get { return 'H'; } }


        [TabPage]
        public string tab = null;

        [CheckBox]
        public bool useServer = false;

        public Dhcp(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {


            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        [ComboBox(Crlf.Contonie)]
        public ProtocolKind protocolKind = ProtocolKind.Udp;
        [Int]
        public int port = 67;
        [BindAddr]
        public BindAddr bindAddress2 = new BindAddr();
        [CheckBox]
        public bool useResolve = false;
        [CheckBox(Crlf.Contonie)]
        public bool useDetailsLog = true;
        [Int(Crlf.Contonie)]
        public int multiple = 10;
        [Int]
        public int timeOut = 10;
        [Int]
        public int leaseTime = 18000;
        [AddressV4]
        public Ip startIp = new Ip(IpKind.V4_0);
        [AddressV4]
        public Ip endIp = new Ip(IpKind.V4_0);
        [AddressV4]
        public Ip maskIp = new Ip("255.255.255.0");
        [AddressV4]
        public Ip gwIp = new Ip(IpKind.V4_0);
        [AddressV4]
        public Ip dnsIp0 = new Ip(IpKind.V4_0);
        [AddressV4]
        public Ip dnsIp1 = new Ip(IpKind.V4_0);
        [CheckBox(Crlf.Contonie)]
        public bool useWpad = false;
        [TextBox]
        public string wpadUrl = "http://";
        [CheckBox]
        public bool useMacAcl = false;
        [Dat]
        public List<macAclClass> macAcl = new List<macAclClass>() { new macAclClass() };
        public class macAclClass
        {
            [TextBox]
            public string macAddress = "";
            [AddressV4]
            public Ip v4Address = new Ip(IpKind.V4_0);
            [TextBox]
            public string macName = "";
        }



    }

}
