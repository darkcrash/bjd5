using System.Collections.Generic;
using Bjd;
using Bjd.ctrl;
using Bjd.net;
using Bjd.option;

namespace Bjd.RemoteServer
{
    class Option : OneOption
    {
        public override char Mnemonic { get { return 'R'; } }

        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel.IsJp(), path, nameTag)
        {
            //var key = "useServer";
            //Add(new OneVal(key, false, Crlf.Nextline, new CtrlCheckBox(Lang.Value(key))));

            //var pageList = new List<OnePage>();
            //key = "Basic";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //pageList.Add(PageAcl());
            //Add(new OneVal("tab", null, Crlf.Nextline, new CtrlTabPage("tabPage", pageList)));

            Add(new OneVal("useServer", false, Crlf.Nextline));

            var pageList = new List<OnePage>();
            pageList.Add(Page1("Basic", Lang.Value("Basic"), kernel));
            pageList.Add(PageAcl());
            Add(new OneVal("tab", null, Crlf.Nextline));


            Read(kernel.IniDb); //�@���W�X�g������̓ǂݍ���
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);

            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 10001, 60, 1)); //�T�[�o��{�ݒ�
            //var key = "password";
            //onePage.Add(new OneVal(key, "", Crlf.Nextline, new CtrlHidden(Lang.Value(key), 20)));

            CreateServerOption(ProtocolKind.Tcp, 10001, 60, 1);
            Add(new OneVal("password", "", Crlf.Nextline, true));

            return onePage;
        }

    }
}
