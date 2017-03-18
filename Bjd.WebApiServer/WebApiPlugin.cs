using Bjd.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using Bjd.Components;

namespace Bjd.WebApiServer
{
    public class WebApiPlugin : IPlugin
    {
        public WebApiPlugin() { }

        public IEnumerator<Type> Dependencies
        {
            get
            {
                yield return typeof(Mailbox.MailboxPlugin);
                yield break;
            }
        }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.WebApiServer";
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

        public ComponentBase CreateComponent(Kernel kernel, Conf conf)
        {
            return null;
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
