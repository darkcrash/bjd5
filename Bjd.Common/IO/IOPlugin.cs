using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using System.Collections.Generic;
using Bjd.Components;

namespace Bjd.Common.IO
{
    public class IOPlugin : IPlugin
    {
        public IOPlugin() { }

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
                return "Bjd.Common.IO";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "IO";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new ConfigurationIO(kernel, path);
        }

        public ComponentBase CreateComponent(Kernel kernel, Conf conf)
        {
            return new IOComponent(kernel, conf);
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
