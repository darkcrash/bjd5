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
        static HttpContextPool Pool;
        Kernel kernel;
        Logger logger;
        HttpContentType contentType;
        Conf conf;

        public static HttpConnectionContext GetContext()
        {
            return Pool.Get();
        }

        private HttpContextPool()
        {
        }

        public static void InitializePool(Kernel kernel, Logger logger, Conf conf, HttpContentType contentType)
        {
            if (Pool != null) Pool.Dispose();
            Pool = new HttpContextPool();
            Pool.kernel = kernel;
            Pool.logger = logger;
            Pool.conf = conf;
            Pool.contentType = contentType;
            Pool.InitializePool(300, 1024);

        }

        protected override int BufferSize => 1;

        protected override HttpConnectionContext CreateBuffer()
        {
            return new HttpConnectionContext(this, kernel, logger, conf, contentType);
        }
    }
}
