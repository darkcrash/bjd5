using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;

namespace Bjd.DnsServer
{
    public class DnsPlugin : IPlugin
    {
        public DnsPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.DnsServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Dns";
            }
        }

        OneOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            switch(nameTag)
            {
                case "Dns":
                    return new DnsServer.Option(kernel, path, nameTag);
                case "DnsDomain":
                    return new DnsServer.OptionDnsDomain(kernel, path, nameTag);
                case "Resource-example":
                    return new DnsServer.OptionDnsResource(kernel, path, nameTag);
            }
            return new DnsServer.Option(kernel, path, nameTag);
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
