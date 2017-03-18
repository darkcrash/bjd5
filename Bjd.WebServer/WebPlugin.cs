using Bjd.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using Bjd.Components;

namespace Bjd.WebServer
{
    public class WebPlugin : IPlugin
    {
        public WebPlugin() { }

        public IEnumerator<Type> Dependencies
        {
            get
            {
                yield break;
            }
        }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.WebServer";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Web";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            switch (path)
            {
                case "OptionVirtualHost":
                    return new Bjd.WebServer.Configurations.VirtualHostOption(kernel, path, nameTag);
            }
            return new Bjd.WebServer.Configurations.WebServerOption(kernel, path, nameTag);
        }

        public ComponentBase CreateComponent(Kernel kernel, Conf conf)
        {
            return null;
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
