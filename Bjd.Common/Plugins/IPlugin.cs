using System;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Servers;
using System.Collections.Generic;
using Bjd.Components;

namespace Bjd.Plugins
{
    public interface IPlugin : IDisposable
    {
        IEnumerator<Type> Dependencies { get; }

        string PluginName { get; }

        string Name { get; }

        ConfigurationSmart CreateOption(Kernel kernel, string path, string nameTag);

        ComponentBase CreateComponent(Kernel kernel, Conf conf);

        OneServer CreateServer(Kernel kernel, Conf conf, OneBind oneBind);


    }
}
