using Bjd.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;

namespace Bjd.TunnelServer
{
    public class TunnelPlugin : IPlugin
    {
        public TunnelPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.TunnelServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Tunnel";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            switch(path)
            {
                case "OptionTunnel":
                    return new TunnelServer.Configurations.TunnelListOption(kernel, path, nameTag);
            }
            return new TunnelServer.Configurations.TunnelOption(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new TunnelServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
