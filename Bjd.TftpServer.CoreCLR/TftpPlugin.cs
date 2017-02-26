using Bjd.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;

namespace Bjd.TftpServer
{
    public class TftpPlugin : IPlugin
    {
        public TftpPlugin() { }

        string IPlugin.PluginName
        {
            get
            {
                return "Bjd.TftpServer.CoreCLR";
            }
        }

        string IPlugin.Name
        {
            get
            {
                return "Tftp";
            }
        }

        SmartOption IPlugin.CreateOption(Kernel kernel, string path, string nameTag)
        {
            return new TftpServer.TftpOption(kernel, path, nameTag);
        }

        OneServer IPlugin.CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            return new TftpServer.Server(kernel, conf, oneBind);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
