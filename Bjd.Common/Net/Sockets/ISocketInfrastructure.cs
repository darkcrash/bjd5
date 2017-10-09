using Bjd.Logs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bjd.Net.Sockets
{
    internal interface ISocketInfrastructure
    {
        void Resolve(bool useResolve, Logger logger);
        void Cancel();
    }
}
