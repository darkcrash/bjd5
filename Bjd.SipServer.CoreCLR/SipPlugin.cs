using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;

namespace Bjd.SipServer
{
    public class SipPlugin : IPlugin
    {
        public SipPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.SipServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Sip";
            }
        }

        SmartOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new SipServer.SipOption(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new SipServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
