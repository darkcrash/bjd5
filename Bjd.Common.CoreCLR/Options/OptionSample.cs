using System;
using System.Collections.Generic;
using Bjd.Controls;
using Bjd.Net;

namespace Bjd.Options
{
    public class OptionSample : OneOption
    {


        //public override string JpMenu { get { return "基本オプション"; } }
        //public override string EnMenu { get { return "Basic Option"; } }
        public override char Mnemonic { get { return 'O'; } }


        public OptionSample(Kernel kernel, string path)
            : base(kernel, path, "Sample")
        {

            var pageList = new List<OnePage>();
            var key = "Basic";
            pageList.Add(Page1(kernel, key, Lang.Value(key)));
            pageList.Add(PageAcl());
            Add(new OneVal(kernel, CtrlType.TabPage, "tab", null, Crlf.Nextline));

            Read(kernel.Configuration); //　レジストリからの読み込み

        }

        private OnePage Page1(Kernel kernel, string name, string title)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 999, 30, 50)); //サーバ基本設定
            CreateServerOption(ProtocolKind.Tcp, 999, 30, 50); //サーバ基本設定
            return onePage;
        }


    }
}