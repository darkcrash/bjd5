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


        public OptionSample(Kernel kernel, String path)
            : base(kernel, path, "Sample")
        {

            var pageList = new List<OnePage>();
            var key = "Basic";
            pageList.Add(Page1(key, Lang.Value(key)));
            pageList.Add(PageAcl());
            Add(new OneVal("tab", null, Crlf.Nextline));

            Read(kernel.Configuration); //　レジストリからの読み込み

        }

        private OnePage Page1(String name, String title)
        {
            var onePage = new OnePage(name, title);
            //onePage.Add(CreateServerOption(ProtocolKind.Tcp, 999, 30, 50)); //サーバ基本設定
            CreateServerOption(ProtocolKind.Tcp, 999, 30, 50); //サーバ基本設定
            return onePage;
        }

        protected void AbstractOnChange(OneCtrl oneCtrl)
        {
        }
    }
}