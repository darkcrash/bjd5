using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;

namespace Bjd.DhcpServer
{
    public class DhcpPlugin : IPlugin
    {
        public DhcpPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.DhcpServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Dhcp";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new DhcpServer.Configurations.Dhcp(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new DhcpServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
