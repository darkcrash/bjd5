using System.Collections.Generic;
using Bjd.Controls;
using Bjd.Utils;
using Bjd.Options.Attributes;
using Bjd.Logs;

namespace Bjd.Options
{
    public class SmartOptionLog : SmartOption
    {
        [TabPage]
        public string tab = null;

        [ComboBox]
        public LogKind normalLogKind =  LogKind.Error;

        [ComboBox]
        public LogKind secureLogKind = LogKind.Error;

        [Folder]
        public string saveDirectory = "";

        [CheckBox]
        public bool useLogFile = true;

        [CheckBox]
        public bool useLogClear = false;

        [Int]
        public int saveDays = 31;

        [Int]
        public int linesMax = 3000;

        [Int]
        public int linesDelete = 2000;

        [Radio]
        public int isDisplay = 1;

        [Dat]
        public List<limitStringClass> limitString = new List<limitStringClass>() { new limitStringClass() };

        public class limitStringClass
        {
            [TextBox]
            public string Character = "";
        }

        [CheckBox]
        public bool useLimitString = false;

        public override char Mnemonic { get { return 'L'; } }

        public SmartOptionLog(Kernel kernel, string path) : base(kernel, path, "Log")
        {
            Read(kernel.Configuration); //　レジストリからの読み込み
        }

    }
}


