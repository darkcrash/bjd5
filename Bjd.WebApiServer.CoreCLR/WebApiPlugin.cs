using Bjd.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;

namespace Bjd.WebApiServer
{
    public class WebApiPlugin : IPlugin
    {
        public WebApiPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.WebApiServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "WebApi";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new WebApiServer.Configurations.WebApiOption(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new WebApiServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
