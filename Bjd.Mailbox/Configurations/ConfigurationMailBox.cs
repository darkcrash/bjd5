using System.Collections.Generic;
using Bjd.Controls;
using Bjd.Configurations;
using Bjd.Configurations.Attributes;

namespace Bjd.Mailbox.Configurations
{
    public class ConfigurationMailBox : ConfigurationSmart
    {
        public override char Mnemonic { get { return 'B'; } }

        public ConfigurationMailBox(Kernel kernel, string path)
            : base(kernel, path, "MailBox")
        {
            Read(kernel.Configuration); //　レジストリからの読み込み
        }


        [Folder]
        public string dir = "";

        [CheckBox]
        public bool useDetailsLog = false;

        [Dat]
        public List<userClass> user = new List<userClass>() { new userClass() };

        public class userClass
        {
            [TextBox]
            public string userName = "";
            [TextBox]
            public string password = "";
        }

    }
}
