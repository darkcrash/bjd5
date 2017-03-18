using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using System.Collections.Generic;
using Bjd.Components;

namespace Bjd.ProxyTelnetServer
{
    public class ProxyTelnetPlugin : IPlugin
    {
        public ProxyTelnetPlugin() { }

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
                return "Bjd.ProxyTelnetServer";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "ProxyTelnet";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new ProxyTelnetServer.ProxyTelnetOption(kernel, path, nameTag);
        }

        public ComponentBase CreateComponent(Kernel kernel, Conf conf)
        {
            return null;
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new ProxyTelnetServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
