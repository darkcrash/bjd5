using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
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

        SmartOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new ProxyFtpServer.ProxyFtpOption(kernel, path, nameTag);
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
