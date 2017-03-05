using System;

using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Configurations;
using System.Collections.Generic;
using Bjd.Configurations.Attributes;

namespace Bjd.TunnelServer
{
    class TunnelOption : SmartOption
    {

        public override string MenuStr
        {
            get { return NameTag; }
        }
        public override char Mnemonic { get { return '0'; } }

        public TunnelOption(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {

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


            PageAcl();
            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        [CheckBox]
        public bool useServer = false;

        [ComboBox(Crlf.Contonie)]
        public ProtocolKind protocolKind = ProtocolKind.Tcp;
        [Int]
        public int port = 0;
        [BindAddr]
        public BindAddr bindAddress2 = new BindAddr();
        [CheckBox]
        public bool useResolve = false;
        [CheckBox(Crlf.Contonie)]
        public bool useDetailsLog = true;
        [Int(Crlf.Contonie)]
        public int multiple = 10;
        [Int]
        public int timeOut = 60;

        [TabPage]
        public string tab = "";

        [Int]
        public int targetPort = 0;
        [TextBox]
        public string targetServer = "";
        [Int]
        public int idleTime = 1;

    }
}
