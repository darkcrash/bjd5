using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Configurations;
using Bjd.Configurations.Attributes;

namespace Bjd.DnsServer.Configurations
{
    public class DnsResourceOption : ConfigurationSmart
    {

        public override char Mnemonic { get { return '0'; } }

        [TabPage]
        public string tab = "";

        public DnsResourceOption(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {


            Read(kernel.Configuration); 
        }

        [Dat]
        public List<resourceListClass> resourceList = new List<resourceListClass>() { new resourceListClass() };


        public class resourceListClass
        {
            [ComboBox]
            public DnsType type = DnsType.A;
            [TextBox(Crlf.Contonie)]
            public string name = "";
            [TextBox]
            public string alias = "";
            [TextBox(Crlf.Contonie)]
            public string address = "";
            [Int]
            public int priority = 10;
        }


    }
}