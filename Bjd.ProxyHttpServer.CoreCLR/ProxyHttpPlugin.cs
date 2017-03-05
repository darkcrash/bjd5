using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;

namespace Bjd.ProxyHttpServer
{
    public class ProxyHttpPlugin : IPlugin
    {
        public ProxyHttpPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.ProxyHttpServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "ProxyHttp";
            }
        }

        SmartOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new ProxyHttpServer.ProxyHttpOption(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new ProxyHttpServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
