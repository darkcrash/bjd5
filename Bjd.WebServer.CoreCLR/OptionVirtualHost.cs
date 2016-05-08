using System.Collections.Generic;
using Bjd;
using Bjd.Controls;
using Bjd.Options;

namespace Bjd.WebServer
{
    public class OptionVirtualHost : OneOption
    {
        //public override string JpMenu { get { return "Webの追加と削除"; } }
        //public override string EnMenu { get { return "Add or Remove VirtualHost"; } }
        public override char Mnemonic { get { return 'A'; } }

        public OptionVirtualHost(Kernel kernel, string path, string nameTag)
            : base(kernel.IsJp, path, nameTag)
        {

            //var pageList = new List<OnePage>();

            //var key = "VirtualHost";
            //pageList.Add(Page1(key, Lang.Value(key), kernel));
            //Add(new OneVal("tab", null, Crlf.Nextline));

            var list1 = new ListVal();
            list1.Add(new OneVal("protocol", 0, Crlf.Nextline));
            list1.Add(new OneVal("host", "", Crlf.Contonie));
            list1.Add(new OneVal("port", 80, Crlf.Nextline));
            Add(new OneVal("hostList", new Dat(list1), Crlf.Nextline));

            var list2 = new ListVal();
            list2.Add(new OneVal("certificate", "", Crlf.Nextline));
            list2.Add(new OneVal("privateKeyPassword", "", Crlf.Nextline, true));
            Add(new OneVal("groupHttps", new Dat(list2), Crlf.Nextline));


            Read(kernel.IniDb); //　レジストリからの読み込み
        }

        private OnePage Page1(string name, string title, Kernel kernel)
        {
            //var onePage = new OnePage(name, title);
            //var list1 = new ListVal();
            //var key = "protocol";
            //list1.Add(new OneVal(key, 0, Crlf.Nextline));
            //key = "host";
            //list1.Add(new OneVal(key, "", Crlf.Contonie));
            //key = "port";
            //list1.Add(new OneVal(key, 80, Crlf.Nextline));
            //onePage.Add(new OneVal("hostList", null, Crlf.Nextline));
            //var list2 = new ListVal();
            //key = "certificate";
            //list2.Add(new OneVal(key, "", Crlf.Nextline));
            //key = "privateKeyPassword";
            //list2.Add(new OneVal(key, "", Crlf.Nextline));
            //key = "groupHttps";
            //onePage.Add(new OneVal(key, null, Crlf.Nextline));

            //return onePage;
            return null;
        }

    }
}
