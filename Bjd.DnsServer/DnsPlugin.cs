using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using System.Collections.Generic;
using Bjd.Components;

namespace Bjd.DnsServer
{
    public class DnsPlugin : IPlugin
    {
        public DnsPlugin() { }

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
                return "Bjd.DnsServer";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Dns";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            switch(path)
            {
                case "OptionDnsDomain":
                    return new DnsServer.Configurations.DnsDomainOption(kernel, path, nameTag);
                case "OptionDnsResource":
                    return new DnsServer.Configurations.DnsResourceOption(kernel, path, nameTag);
            }
            return new DnsServer.Configurations.DnsOption(kernel, path, nameTag);
        }

        public ComponentBase CreateComponent(Kernel kernel, Conf conf)
        {
            return null;
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new DnsServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
