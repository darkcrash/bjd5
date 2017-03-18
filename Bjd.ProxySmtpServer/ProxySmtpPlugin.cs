using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using System.Collections.Generic;
using Bjd.Components;

namespace Bjd.ProxySmtpServer
{
    public class ProxySmtpPlugin : IPlugin
    {
        public ProxySmtpPlugin() { }

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
                return "Bjd.ProxySmtpServer";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "ProxySmtp";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new ProxySmtpServer.Configurations.ProxySmtpOption(kernel, path, nameTag);
        }

        public ComponentBase CreateComponent(Kernel kernel, Conf conf)
        {
            return null;
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new ProxySmtpServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
