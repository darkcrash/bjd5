﻿using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;

namespace Bjd.Pop3Server
{
    public class Pop3Plugin : IPlugin
    {
        public Pop3Plugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.Pop3Server.CoreCLR";
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

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new Pop3Server.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
