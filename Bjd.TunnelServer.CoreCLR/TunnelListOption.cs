using System.Collections.Generic;

using Bjd;
using Bjd.Controls;
using Bjd.Options;
using Bjd.Net;
using Bjd.Options.Attributes;

namespace Bjd.TunnelServer
{
    internal class TunnelListOption : SmartOption
    {
        public override char Mnemonic { get { return 'A'; } }

        public TunnelListOption(Kernel kernel, string path, string nameTag)
            : base(kernel, path, nameTag)
        {
            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        [Dat]
        public List<tunnelListClass> tunnelList = new List<tunnelListClass>() { new tunnelListClass() };
        public class tunnelListClass
        {
            [ComboBox]
            public ProtocolKind protocol = ProtocolKind.Tcp;
            [Int]
            public int srcPort = 0;
            [TextBox]
            public string server = "";
            [Int]
            public int dstPort = 0;
        }

        [TabPage]
        public string tab = "";


    }
}
