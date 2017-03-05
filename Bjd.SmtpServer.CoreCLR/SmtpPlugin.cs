using Bjd.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bjd.Net;
using Bjd.Configurations;
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

        SmartOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            switch(path)
            {
                case "OptionMl":
                    return new SmtpServer.MailingListOption(kernel, path, nameTag);
                case "OptionOneMl":
                    return new SmtpServer.OneMlOption(kernel, path, nameTag);
            }
            return new SmtpServer.SmtpOption(kernel, path, nameTag);
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
