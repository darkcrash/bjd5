using System.Collections.Generic;
using Bjd.Controls;
using Bjd.Utils;
using Bjd.Options.Attributes;

namespace Bjd.Options
{
    public class SmartOptionBasic : SmartOption
    {
        public override char Mnemonic { get { return 'O'; } }

        [CheckBox]
        public bool useExitDlg = false;

        [CheckBox]
        public bool useLastSize = true;

        [CheckBox]
        public bool isWindowOpen = true;

        [CheckBox]
        public bool useAdminPassword = false;

        [Hidden]
        public string password = "";

        [TextBox]
        public string serverName = "";

        [CheckBox]
        public bool editBrowse = false;

        [ComboBox]
        public int lang = 2;

        [TabPage]
        public object tab = null;

        public SmartOptionBasic(Kernel kernel, string path)
            : base(kernel, path, "Basic")
        {
            //var pageList = new List<OnePage>();


            //var key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));

            //Add(new OneVal(CtrlType.CheckBox, "useExitDlg", false, Crlf.Nextline));
            //Add(new OneVal(CtrlType.CheckBox, "useLastSize", true, Crlf.Nextline));
            //Add(new OneVal(CtrlType.CheckBox, "isWindowOpen", true, Crlf.Nextline));
            //Add(new OneVal(CtrlType.CheckBox, "useAdminPassword", false, Crlf.Nextline));
            //Add(new OneVal(CtrlType.Hidden, "password", "", Crlf.Nextline));
            //Add(new OneVal(CtrlType.TextBox, "serverName", "", Crlf.Nextline));
            //Add(new OneVal(CtrlType.CheckBox, "editBrowse", false, Crlf.Nextline));
            //Add(new OneVal(CtrlType.ComboBox, "lang", 2, Crlf.Nextline));

            //Add(new OneVal(CtrlType.TabPage, "tab", null, Crlf.Nextline));

            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {

            //var onePage = new OnePage(name, title);

            //var key = "useExitDlg";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline));
            //key = "useLastSize";
            //onePage.Add(new OneVal(key, true, Crlf.Nextline));
            //key = "isWindowOpen";
            //onePage.Add(new OneVal(key, true, Crlf.Nextline));
            //key = "useAdminPassword";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline));
            //key = "password";
            //onePage.Add(new OneVal("password", "", Crlf.Nextline));
            //key = "serverName";
            //onePage.Add(new OneVal("serverName", "", Crlf.Nextline));
            //key = "editBrowse";
            //onePage.Add(new OneVal("editBrowse", false, Crlf.Nextline));
            //key = "lang";
            //onePage.Add(new OneVal("lang", 2, Crlf.Nextline));
            //return onePage;
            return null;
        }
    }
}


