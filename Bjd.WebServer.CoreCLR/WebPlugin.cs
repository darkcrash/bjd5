using Bjd.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;

namespace Bjd.WebServer
{
    public class WebPlugin : IPlugin
    {
        public WebPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.WebServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Web";
            }
        }

        OneOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            switch (path)
            {
                case "OptionVirtualHost":
                    return new Bjd.WebServer.Configurations.VirtualHostOption(kernel, path, nameTag);
            }
            return new Bjd.WebServer.Configurations.WebServerOption(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new Bjd.WebServer.WebServer(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
