using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Options;
using Bjd.Options.Attributes;

namespace Bjd.SmtpServer
{
    class OneMlOption : SmartOption
    {
        public override string MenuStr
        {
            get { return NameTag; }
        }
        public override char Mnemonic { get { return '0'; } }

        public OneMlOption(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        [TabPage]
        public string tab = "";

        [Folder]
        public string manageDir = "";
        [CheckBox]
        public bool useDetailsLog = false;
        [ComboBox]
        public int title = 5;
        [Int]
        public int maxGet = 10;
        [Int]
        public int maxSummary = 100;
        [CheckBox]
        public bool autoRegistration = true;

        [Dat]
        public List<memberListClass> memberList = new List<memberListClass>() { new memberListClass() };
        public class memberListClass
        {
            [TextBox(Crlf.Contonie)]
            public string name = "";
            [TextBox]
            public string address = "";
            [CheckBox(Crlf.Contonie)]
            public bool manager = false;
            [CheckBox(Crlf.Contonie)]
            public bool reacer = true;
            [CheckBox]
            public bool contributor = true;
            [Hidden]
            public string pass = "";
        }

        [Memo]
        public string guideDocument = "";

        [Memo]
        public string denyDocument = "";

        [Memo]
        public string confirmDocument = "";

        [Memo]
        public string welcomeDocument = "";

        [Memo]
        public string appendDocument = "";

        [Memo]
        public string helpDocument = "";

        [Memo]
        public string adminDocument = "";
    }
}
