using System.Collections.Generic;
using Bjd.Controls;
using Bjd.Utils;
using System;

namespace Bjd.Options
{
    [Obsolete]
    public class OptionLog : OneOption
    {

        public override char Mnemonic { get { return 'L'; } }

        public OptionLog(Kernel kernel, string path) : base(kernel, path, "Log")
        {
            //var pageList = new List<OnePage>();

            Add(new OneVal(kernel, CtrlType.TabPage, "tab", null, Crlf.Nextline));

            // Page1
            //pageList.Add(Page1("Basic", Lang.Value(key), kernel));
            Add(new OneVal(kernel, CtrlType.ComboBox, "normalLogKind", 2, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.ComboBox, "secureLogKind", 2, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Folder, "saveDirectory", "", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useLogFile", true, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useLogClear", false, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Int, "saveDays", 31, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Int, "linesMax", 3000, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Int, "linesDelete", 2000, Crlf.Nextline));
            //Add(new OneVal("font", null, Crlf.Nextline));

            // Page2
            //pageList.Add(Page2("Limit", Lang.Value(key)));
            Add(new OneVal(kernel, CtrlType.Radio, "isDisplay", 1, Crlf.Nextline));
            var list = new ListVal();
            list.Add(new OneVal(kernel, CtrlType.TextBox, "Character", "", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Dat, "limitString", new Dat(list), Crlf.Nextline));
            //Add(new OneVal("limitString", new OneDat(true, new string[] { }, new bool[] { }), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useLimitString", false, Crlf.Nextline));



            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            //var onePage = new OnePage(name, title);
            //var key = "normalLogKind";
            //onePage.Add(new OneVal(key, 2, Crlf.Nextline));
            //key = "secureLogKind";
            //onePage.Add(new OneVal(key, 2, Crlf.Nextline));
            //key = "saveDirectory";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline));
            //key = "useLogFile";
            //onePage.Add(new OneVal(key, true, Crlf.Nextline));
            //key = "useLogClear";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline));
            //key = "saveDays";
            //onePage.Add(new OneVal(key, 31, Crlf.Nextline));
            //key = "linesMax";
            //onePage.Add(new OneVal(key, 3000, Crlf.Nextline));
            //key = "linesDelete";
            //onePage.Add(new OneVal(key, 2000, Crlf.Nextline));
            //onePage.Add(new OneVal("font", null, Crlf.Nextline));
            //return onePage;
            return null;
        }

        private OnePage Page2(string name, string title)
        {
            //var onePage = new OnePage(name, title);
            //var key = "isDisplay";
            //onePage.Add(new OneVal(key, 1, Crlf.Nextline));
            //var list = new ListVal();

            //key = "Character";
            //list.Add(new OneVal(key, "", Crlf.Nextline));
            //key = "limitString";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline));
            //key = "useLimitString";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline));
            //return onePage;
            return null;
        }

    }
}


