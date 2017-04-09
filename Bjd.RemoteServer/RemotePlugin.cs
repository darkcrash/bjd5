using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using System.Collections.Generic;
using Bjd.Components;

namespace Bjd.RemoteServer
{
    public class RemotePlugin : IPlugin
    {
        public RemotePlugin() { }


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
                return "Bjd.RemoteServer";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Remote";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new Configurations.RemoteOption(kernel, path, nameTag);
        }

        private Logs.RemoteLogService logService;


        public ComponentBase CreateComponent(Kernel kernel, Conf conf)
        {
            return null;
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            if (logService != null) kernel.LogServices.Remove(logService);
            logService = new Logs.RemoteLogService();
            kernel.LogServices.Add(logService);

            var sv = new RemoteServer.Server(kernel, conf, oneBind);
            logService.remoteServer = sv;
            return sv;
        }

        void IDisposable.Dispose()
        {
        }
    }
}
