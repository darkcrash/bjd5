using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;

namespace Bjd.ProxySmtpServer
{
    public class ProxySmtpPlugin : IPlugin
    {
        public ProxySmtpPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.ProxySmtpServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "ProxySmtp";
            }
        }

        OneOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new ProxySmtpServer.Option(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new ProxySmtpServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
