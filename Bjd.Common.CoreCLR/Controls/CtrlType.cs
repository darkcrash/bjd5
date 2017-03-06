namespace Bjd.Controls
{
    public enum CtrlType
    {
        CheckBox = 0,
        TextBox = 1,
        AddressV4 = 2,
        AddressV6 = 3,
        BindAddr = 4,
        Folder = 5,
        File = 6,
        ComboBox = 7,
        Dat = 8,
        Group = 9,
        Int = 10,
        Label = 11,
        Memo = 12,
        TabPage = 13,
        Font = 14,
        Radio = 15,
        Hidden = 16
    }

    public static class ControlExtension
    {
        public static string GetControlTypeString(this CtrlType t)
        {
            switch (t)
            {
                case CtrlType.CheckBox:
                    return "BOOL";
                case CtrlType.TextBox:
                    return "STRING";
                case CtrlType.Hidden:
                    return "HIDE_STRING";
                case CtrlType.ComboBox:
                    return "LIST";
                case CtrlType.Folder:
                    return "FOLDER";
                case CtrlType.File:
                    return "FILE";
                case CtrlType.Dat:
                    return "DAT";
                case CtrlType.Int:
                    return "INT";
                case CtrlType.AddressV4:
                    return "ADDRESS_V4";
                case CtrlType.AddressV6:
                    return "ADDRESS_V6";
                case CtrlType.BindAddr:
                    return "BINDADDR";
                case CtrlType.Font:
                    return "FONT";
                case CtrlType.Group:
                    return "GROUP";
                case CtrlType.Label:
                    return "LABEL";
                case CtrlType.Memo:
                    return "MEMO";
                case CtrlType.Radio:
                    return "RADIO";
                case CtrlType.TabPage:
                    return "TAB_PAGE";
            }
            return string.Empty;
        }

    }

}
