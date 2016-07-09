using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.WebServer.Handlers
{
    internal class DirectoryListHandler : IHandler
    {
        public bool Request(HttpRequestContext context, HandlerSelectorResult selector)
        {
            //インデックスドキュメントを生成する
            if (!context.Response.CreateFromIndex(context.Request, selector.FullPath))
                return false;
            return true;
        }
    }
}
