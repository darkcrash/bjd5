using Bjd.Logs;
using Bjd.Memory;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.Threading;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bjd.Net.Sockets
{
    public interface ISocketBase
    {
        string RemoteHostname { get; }

    }
}
