using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.WebServer.Handlers
{
    public interface IHandler
    {
        string HandlerName { get; }

        bool Request(HttpRequestContext context);

    }
}
