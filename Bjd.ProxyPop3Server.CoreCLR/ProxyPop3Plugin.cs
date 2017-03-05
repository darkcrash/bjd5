using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;

namespace Bjd.ProxyPop3Server
{
    public class ProxyPop3Plugin : IPlugin
    {
        public ProxyPop3Plugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.ProxyPop3Server.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "ProxyPop3";
            }
        }

        SmartOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new ProxyPop3Server.ProxyPop3Option(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new ProxyPop3Server.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
