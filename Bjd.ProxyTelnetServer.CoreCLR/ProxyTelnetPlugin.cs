using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;

namespace Bjd.ProxyTelnetServer
{
    public class ProxyTelnetPlugin : IPlugin
    {
        public ProxyTelnetPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.ProxyTelnetServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "ProxyTelnet";
            }
        }

        SmartOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new ProxyTelnetServer.ProxyTelnetOption(kernel, path, nameTag);
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
