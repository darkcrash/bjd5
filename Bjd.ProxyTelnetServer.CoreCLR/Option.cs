using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;
using System.Collections.Generic;

namespace Bjd.ProxyTelnetServer
{
    class Option : OneOption
    {

        public override char Mnemonic { get { return 'T'; } }


        public Option(Kernel kernel, string path, string nameTag)
            : base(kernel.IsJp, path, nameTag)
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

            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 8023, 60, 10)); //�T�[�o��{�ݒ�
            CreateServerOption(ProtocolKind.Tcp, 8023, 60, 10); //�T�[�o��{�ݒ�

            //var key = "idleTime";
            //onePage.Add(new OneVal(key, 1, Crlf.Contonie, new CtrlInt(Lang.Value(key), 5)));

            onePage.Add(new OneVal("idleTime", 1, Crlf.Contonie));

            return onePage;
        }

    }
}
