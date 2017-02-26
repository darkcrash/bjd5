using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Options;
using Bjd.Net;
using Bjd.Options.Attributes;

namespace Bjd.SmtpServer
{
    class MailingListOption : SmartOption
    {

        public override char Mnemonic { get { return 'A'; } }

        public MailingListOption(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {
            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        [Dat]
        public List<mlListClass> mlList = new List<mlListClass>() { new mlListClass() };
        public class mlListClass
        {
            [TextBox]
            public string user = "";
        }

        [TabPage]
        public string tab = "";

    }
}
