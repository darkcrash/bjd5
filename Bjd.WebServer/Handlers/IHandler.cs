﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.WebServer.Handlers
{
    internal interface IHandler
    {

        bool Request(HttpContext context, HandlerSelectorResult result);

    }
}
