using System;
using Bjd.Plugins;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;

namespace Bjd.FtpServer
{
    public class FtpPlugin : IPlugin
    {
        public FtpPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.FtpServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Ftp";
            }
        }

        OneOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new FtpServer.Option(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new FtpServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
