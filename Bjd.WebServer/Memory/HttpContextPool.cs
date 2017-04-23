using Bjd.Configurations;
using Bjd.Logs;
using Bjd.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bjd.WebServer.Memory
{
    public class HttpContextPool : PoolBase<HttpConnectionContext>
    {
        Kernel kernel;
        Logger logger;
        HttpContentType contentType;
        Conf conf;

        private HttpContextPool()
        {
        }

        public static HttpContextPool InitializePool(Kernel kernel, Logger logger, Conf conf, HttpContentType contentType)
        {
            var Pool = new HttpContextPool();
            Pool.kernel = kernel;
            Pool.logger = logger;
            Pool.conf = conf;
            Pool.contentType = contentType;
            Pool.InitializePool(300, 1024);
            return Pool;
        }

        protected override int BufferSize => 1;

        protected override HttpConnectionContext CreateBuffer()
        {
            return new HttpConnectionContext(this, kernel, logger, conf, contentType);
        }
    }
}
