using System;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;

namespace Bjd.Plugins
{
    public interface IPlugin : IDisposable
    {

        string PluginName { get; }

        string Name { get; }

        ConfigurationSmart CreateOption(Kernel kernel, string path, string nameTag);

        OneServer CreateServer(Kernel kernel, Conf conf, OneBind oneBind);

    }
}
