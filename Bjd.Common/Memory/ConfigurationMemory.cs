using System.Collections.Generic;
using Bjd.Controls;
using Bjd.Configurations;
using Bjd.Configurations.Attributes;

namespace Bjd.Memory
{
    public class ConfigurationMemory : ConfigurationSmart
    {
        public override char Mnemonic { get { return 'M'; } }

        public ConfigurationMemory(Kernel kernel, string path)
            : base(kernel, path, "Memory")
        {
            Read(kernel.Configuration); //　レジストリからの読み込み
        }

        [CheckBox]
        public bool useCleanup = false;

        [CheckBox]
        public bool useDetailsLog = false;

    }
}
