using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.WebServer.Handlers
{
    internal class NotFoundHandler : IHandler
    {
        public bool Request(HttpContext context, HandlerSelectorResult selector)
        {
            context.ResponseCode = 404;
            return true;
        }
    }
}
