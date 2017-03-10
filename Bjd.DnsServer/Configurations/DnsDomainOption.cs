using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Configurations;
using Bjd.Configurations.Attributes;

namespace Bjd.DnsServer.Configurations
{
    public class DnsDomainOption : ConfigurationSmart
    {

        public override char Mnemonic { get { return 'A'; } }

        [TabPage]
        public string tab = "";


        public DnsDomainOption(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            Read(kernel.Configuration); //�@���W�X�g������̓ǂݍ���
        }

        [Dat]
        public List<domainListClass> domainList = new List<domainListClass>() { new domainListClass() };

        public class domainListClass
        {
            [TextBox]
            public string name = "";
            [CheckBox]
            public bool authority = true;
        }

    }
}
