using System.Collections.Generic;

using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Configurations;

namespace Bjd.Pop3Server
{
    public class Option : OneOption
    {

        public override char Mnemonic { get { return 'P'; } }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

            //var key = "useServer";
            //Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            //var pageList = new List<OnePage>();
            //key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key)));
            //key = "Cange Password";
            //pageList.Add(Page2(key, Lang.Value(key)));
            //key = "AutoDeny";
            //pageList.Add(Page3(key, Lang.Value(key)));
            //pageList.Add(PageAcl());
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            Add(new OneVal(kernel, CtrlType.CheckBox, "useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1(kernel, "Basic", Lang.Value("Basic")));
            pageList.Add(Page2(kernel, "Cange Password", Lang.Value("Cange Password")));
            pageList.Add(Page3(kernel, "AutoDeny", Lang.Value("AutoDeny")));
            pageList.Add(PageAcl());
            Add(new OneVal(kernel, CtrlType.TabPage, "tab", null, Crlf.Nextline));


            Read(kernel.Configuration); //�@���W�X�g������̓ǂݍ���
        }

        private OnePage Page1(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 110, 30, 10)); //�T�[�o��{�ݒ�
            //var key = "bannerMessage";
            //onePage.Add(new OneVal(key, "$p (Version $v) ready", Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 80)));
            //key = "authType";
            //onePage.Add(new OneVal(key, 0, Crlf.Nextline, new CtrlRadio(Lang.Value(key), new[] { Lang.Value(key + "1"), Lang.Value(key + "2"), Lang.Value(key + "3") }, 600, 2)));
            //key = "authTimeout";
            //onePage.Add(new OneVal(key, 30, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));

            CreateServerOption(ProtocolKind.Tcp, 110, 30, 10);

            Add(new OneVal(kernel, CtrlType.TextBox, "bannerMessage", "$p (Version $v) ready", Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Radio, "authType", 0, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Int, "authTimeout", 30, Crlf.Nextline));

            return onePage;
        }

        private OnePage Page2(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //var key = "useChps";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            //key = "minimumLength";
            //onePage.Add(new OneVal(key, 8, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "disableJoe";
            //onePage.Add(new OneVal(key, true, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            //var list = new ListVal();
            //key = "useNum";
            //list.Add(new OneVal(key, true, Crlf.Contonie, new CtrlCheckBox(Lang.Value(key))));
            //key = "useSmall";
            //list.Add(new OneVal(key, true, Crlf.Contonie, new CtrlCheckBox(Lang.Value(key))));
            //key = "useLarge";
            //list.Add(new OneVal(key, true, Crlf.Contonie, new CtrlCheckBox(Lang.Value(key))));
            //key = "useSign";
            //list.Add(new OneVal(key, true, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            //key = "groupNeed";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline, new CtrlGroup(Lang.Value(key), list)));

            Add(new OneVal(kernel, CtrlType.CheckBox, "useChps", false, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Int, "minimumLength", 8, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.CheckBox, "disableJoe", true, Crlf.Nextline));

            var list = new ListVal();
            Add(new OneVal(kernel, CtrlType.CheckBox, "useNum", true, Crlf.Contonie));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useSmall", true, Crlf.Contonie));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useLarge", true, Crlf.Contonie));
            Add(new OneVal(kernel, CtrlType.CheckBox, "useSign", true, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Group, "groupNeed", new Dat(list), Crlf.Nextline));

            return onePage;
        }

        private OnePage Page3(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //var key = "useAutoAcl";
            //onePage.Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            //key = "autoAclLabel";
            //onePage.Add(new OneVal(key, Lang.Value(key), Crlf.Nextline, new CtrlLabel(Lang.Value(key))));
            //key = "autoAclMax";
            //onePage.Add(new OneVal(key, 5, Crlf.Contonie, new CtrlInt(Lang.Value(key), 5)));
            //key = "autoAclSec";
            //onePage.Add(new OneVal(key, 60, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));

            Add(new OneVal(kernel, CtrlType.CheckBox, "useAutoAcl", false, Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Label, "autoAclLabel", Lang.Value("autoAclLabel"), Crlf.Nextline));
            Add(new OneVal(kernel, CtrlType.Int, "autoAclMax", 5, Crlf.Contonie));
            Add(new OneVal(kernel, CtrlType.Int, "autoAclSec", 60, Crlf.Nextline));

            return onePage;
        }


    }
}
