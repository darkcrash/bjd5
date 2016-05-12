using System.Collections.Generic;
using Bjd.Controls;
using Bjd.Utils;

namespace Bjd.Options
{
    public class OptionBasic : OneOption
    {
        public override char Mnemonic { get { return 'O'; } }


        public OptionBasic(Kernel kernel, string path)
            : base(kernel.IsJp, path, "Basic")
        {
            //var pageList = new List<OnePage>();


            //var key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));

            Add(new OneVal("useExitDlg", false, Crlf.Nextline));
            Add(new OneVal("useLastSize", true, Crlf.Nextline));
            Add(new OneVal("isWindowOpen", true, Crlf.Nextline));
            Add(new OneVal("useAdminPassword", false, Crlf.Nextline));
            Add(new OneVal("password", "", Crlf.Nextline));
            Add(new OneVal("serverName", "", Crlf.Nextline));
            Add(new OneVal("editBrowse", false, Crlf.Nextline));
            Add(new OneVal("lang", 2, Crlf.Nextline));


            Add(new OneVal("tab", null, Crlf.Nextline));

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


