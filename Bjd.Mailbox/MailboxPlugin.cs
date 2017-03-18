using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using System.Collections.Generic;
using System.IO;
using Bjd.Components;

namespace Bjd.Mailbox
{
    public class MailboxPlugin : IPlugin
    {

        public MailboxPlugin() { }

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
                return "Bjd.Mailbox";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "MailBox";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new Mailbox.Configurations.ConfigurationMailBox(kernel, path);
        }

        public ComponentBase CreateComponent(Kernel kernel, Conf conf)
        {
            //return new MailBox(logger, datUser, dirFullPath);
            return new MailBox(kernel, conf);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return null;
        }

        void IDisposable.Dispose()
        {
        }

    }
}
