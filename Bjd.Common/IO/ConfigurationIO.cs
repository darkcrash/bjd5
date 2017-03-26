using System.Collections.Generic;
using Bjd.Controls;
using Bjd.Configurations;
using Bjd.Configurations.Attributes;

namespace Bjd.Common.IO
{
    public class ConfigurationIO : ConfigurationSmart
    {
        public override char Mnemonic { get { return 'I'; } }

        public ConfigurationIO(Kernel kernel, string path)
            : base(kernel, path, "IO")
        {
            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        [Int]
        public int cacheInterval = 3000;

        [CheckBox]
        public bool useDetailsLog = false;

    }
}
