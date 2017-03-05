using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;

namespace Bjd.SipServer
{
    public class SipPlugin : IPlugin
    {
        public SipPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.SipServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Sip";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new SipServer.Configurations.SipOption(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new SipServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
