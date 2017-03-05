using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;

namespace Bjd.RemoteServer
{
    public class RemotePlugin : IPlugin
    {
        public RemotePlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.RemoteServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Remote";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new RemoteServer.Configurations.RemoteOption(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new RemoteServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
