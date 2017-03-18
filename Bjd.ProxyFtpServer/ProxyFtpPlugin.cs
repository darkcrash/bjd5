using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using System.Collections.Generic;
using Bjd.Components;

namespace Bjd.ProxyFtpServer
{
    public class ProxyFtpPlugin : IPlugin
    {
        public ProxyFtpPlugin() { }

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
                return "Bjd.ProxyFtpServer";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "ProxyFtp";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new ProxyFtpServer.Configurations.ProxyFtpOption(kernel, path, nameTag);
        }

        public ComponentBase CreateComponent(Kernel kernel, Conf conf)
        {
            return null;
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new ProxyFtpServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
