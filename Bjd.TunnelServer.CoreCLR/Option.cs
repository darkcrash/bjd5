using System;

using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;
using System.Collections.Generic;

namespace Bjd.TunnelServer
{
    class Option : OneOption
    {

        public override string MenuStr
        {
            get { return NameTag; }
        }
        public override char Mnemonic { get { return '0'; } }

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

            Read(kernel.IniDb); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            var onePage = new OnePage(name, title);

            //nameTagからポート番号を取得しセットする（変更不可）
            var tmp = NameTag.Split(':');
            var protocolKind = ProtocolKind.Tcp;
            var port = 0;
            var targetServer = "";
            var targetPort = 0;
            if (tmp.Length == 4)
            {
                //値を強制的に設定
                protocolKind = (tmp[0] == "Tunnel-TCP") ? ProtocolKind.Tcp : ProtocolKind.Udp;
                port = Convert.ToInt32(tmp[1]);
                targetServer = tmp[2];
                targetPort = Convert.ToInt32(tmp[3]);
            }
            //onePage.Add(CreateServerOption(protocolKind, port, 60, 10)); //サーバ基本設定
            CreateServerOption(protocolKind, port, 60, 10);

            //var key = "targetPort";
            //onePage.Add(new OneVal(key, targetPort, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            //key = "targetServer";
            //onePage.Add(new OneVal(key, targetServer, Crlf.Nextline, new CtrlTextBox(Lang.Value(key), 50)));
            //key = "idleTime";
            //onePage.Add(new OneVal(key, 1, Crlf.Nextline, new CtrlInt(Lang.Value(key), 5)));
            Add(new OneVal("targetPort", targetPort, Crlf.Nextline));
            Add(new OneVal("targetServer", targetServer, Crlf.Nextline));
            Add(new OneVal("idleTime", 1, Crlf.Nextline));

            return onePage;
        }

    }
}
