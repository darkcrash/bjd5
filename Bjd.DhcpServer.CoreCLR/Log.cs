﻿
using Bjd;
using Bjd.option;
using Bjd.util;

namespace Bjd.DhcpServer
{
    partial class Server
    {
        private readonly Dat _macAcl;

        //BJD.Lang.txtに必要な定義が揃っているかどうかの確認
        protected override void CheckLang()
        {
            Lang.Value(1);
            for (var n = 3; n <= 6; n++)
            {
                Lang.Value(n);
            }
        }

        public override string GetMsg(int messageNo)
        {
            switch (messageNo)
            {
                case 1:
                case 3:
                case 4:
                case 5:
                case 6:
                    return Lang.Value(messageNo);
            }
            return "unknown";
        }

    }
}
