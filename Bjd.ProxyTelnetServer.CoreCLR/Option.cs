using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Configurations;
using System.Collections.Generic;

namespace Bjd.ProxyTelnetServer
{
    class Option : ConfigurationBase
    {

        public override char Mnemonic { get { return 'T'; } }


        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {
            //var key = "useServer";
            //Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));
            //var pageList = new List<OnePage>();
            //key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //pageList.Add(PageAcl());
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            Add(new OneVal(kernel, CtrlType.CheckBox, "useServer", false, Crlf.Nextline));
            var pageList = new List<OnePage>();
            pageList.Add(Page1(kernel, "Basic", Lang.Value("Basic")));
            pageList.Add(PageAcl());
            Add(new OneVal(kernel, CtrlType.TabPage, "tab", null, Crlf.Nextline));


            Read(kernel.Configuration); //�@���W�X�g������̓ǂݍ���
        }

        private OnePage Page1(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 8023, 60, 10)); //�T�[�o��{�ݒ�
            CreateServerOption(ProtocolKind.Tcp, 8023, 60, 10); //�T�[�o��{�ݒ�

            //var key = "idleTime";
            //onePage.Add(new OneVal(key, 1, Crlf.Contonie, new CtrlInt(Lang.Value(key), 5)));

            onePage.Add(new OneVal(kernel, CtrlType.Int, "idleTime", 1, Crlf.Contonie));

            return onePage;
        }

    }
}
