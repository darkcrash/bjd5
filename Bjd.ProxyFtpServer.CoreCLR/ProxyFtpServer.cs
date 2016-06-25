using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;

namespace Bjd.ProxyFtpServer
{
    public class ProxyFtpPlugin : IPlugin
    {
        public ProxyFtpPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.ProxyFtpServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "ProxyFtp";
            }
        }

        OneOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new ProxyFtpServer.Option(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new ProxyFtpServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
