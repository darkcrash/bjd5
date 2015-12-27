using System.Collections.Generic;
using Bjd.ctrl;
using Bjd.util;

namespace Bjd.option
{
    public class OptionLog : OneOption
    {

        public override char Mnemonic { get { return 'L'; } }

        public OptionLog(Kernel kernel, string path) : base(kernel.IsJp(), path, "Log")
        {
            var pageList = new List<OnePage>();

            var key = "Basic";
            pageList.Add(Page1(key, Lang.Value(key), kernel));
            key = "Limit";
            pageList.Add(Page2(key, Lang.Value(key)));
            Add(new OneVal("tab", null, Crlf.Nextline));
            Read(kernel.IniDb); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);
            var key = "normalLogKind";
            onePage.Add(new OneVal(key, 2, Crlf.Nextline));
            key = "secureLogKind";
            onePage.Add(new OneVal(key, 2, Crlf.Nextline));
            key = "saveDirectory";
            onePage.Add(new OneVal(key, "", Crlf.Nextline));
            key = "useLogFile";
            onePage.Add(new OneVal(key, true, Crlf.Nextline));
            key = "useLogClear";
            onePage.Add(new OneVal(key, false, Crlf.Nextline));
            key = "saveDays";
            onePage.Add(new OneVal(key, 31, Crlf.Nextline));
            key = "linesMax";
            onePage.Add(new OneVal(key, 3000, Crlf.Nextline));
            key = "linesDelete";
            onePage.Add(new OneVal(key, 2000, Crlf.Nextline));
            onePage.Add(new OneVal("font", null, Crlf.Nextline));
            return onePage;
        }

        private OnePage Page2(string name, string title)
        {
            var onePage = new OnePage(name, title);
            var key = "isDisplay";
            onePage.Add(new OneVal(key, 1, Crlf.Nextline));
            var list = new ListVal();

            key = "Character";
            list.Add(new OneVal(key, "", Crlf.Nextline));
            key = "limitString";
            onePage.Add(new OneVal(key, null, Crlf.Nextline));
            key = "useLimitString";
            onePage.Add(new OneVal(key, false, Crlf.Nextline));
            return onePage;
        }

    }
}


