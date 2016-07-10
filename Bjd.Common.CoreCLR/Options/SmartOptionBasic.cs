﻿using System.Collections.Generic;
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
        public LangKind lang =  LangKind.Jp;

        [TabPage]
        public object tab = null;

        public SmartOptionBasic(Kernel kernel, string path)
            : base(kernel, path, "Basic")
        {

            Read(kernel.Configuration); //　レジストリからの読み込み
        }

    }
}


