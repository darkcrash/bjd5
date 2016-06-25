using Bjd.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;

namespace Bjd.SmtpServer
{
    public class SmtpPlugin : IPlugin
    {
        public SmtpPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.SmtpServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Smtp";
            }
        }

        OneOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            switch(nameTag)
            {
                case "Smtp":
                    return new SmtpServer.Option(kernel, path, nameTag);
                case "Ml":
                    return new SmtpServer.OptionMl(kernel, path, nameTag);
            }
            return new SmtpServer.OptionOneMl(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new SmtpServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
