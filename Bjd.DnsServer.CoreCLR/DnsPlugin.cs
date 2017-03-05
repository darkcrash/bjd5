using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
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

        SmartOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            switch(path)
            {
                case "OptionDnsDomain":
                    return new DnsServer.DnsDomainOption(kernel, path, nameTag);
                case "OptionDnsResource":
                    return new DnsServer.DnsResourceOption(kernel, path, nameTag);
            }
            return new DnsServer.DnsOption(kernel, path, nameTag);
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
