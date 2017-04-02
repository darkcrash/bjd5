
using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using System.Collections.Generic;
using Bjd.Components;

namespace Bjd.Memory
{
    public class MemoryPlugin : IPlugin
    {
        public MemoryPlugin() { }

        public IEnumerator<Type> Dependencies
        {
            get
            {
                yield break;
            }
        }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.Memory";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Memory";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new ConfigurationMemory(kernel, path);
        }

        public ComponentBase CreateComponent(Kernel kernel, Conf conf)
        {
            return new MemoryComponent(kernel, conf);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return null;
        }

        void IDisposable.Dispose()
        {
        }
    }
}
