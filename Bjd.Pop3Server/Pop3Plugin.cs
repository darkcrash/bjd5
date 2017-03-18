using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using System.Collections.Generic;
using Bjd.Components;

namespace Bjd.Pop3Server
{
    public class Pop3Plugin : IPlugin
    {
        public Pop3Plugin() { }

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
                return "Bjd.Pop3Server";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Pop3";
            }
        }

        ConfigurationSmart IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new Pop3Server.Configurations.Pop3Option(kernel, path, nameTag);
        }

        public ComponentBase CreateComponent(Kernel kernel, Conf conf)
        {
            return null;
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new Pop3Server.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
