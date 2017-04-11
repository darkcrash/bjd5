using Bjd.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bjd.WebServer.Memory
{
    public class HttpContextPool : PoolBase<HttpConnectionContext>
    {
        static HttpContextPool Pool = new HttpContextPool();

        public static HttpConnectionContext GetContext()
        {
            return Pool.Get();
        }

        private HttpContextPool()
        {
            InitializePool(300, 1024);
        }

        protected override int BufferSize => 1;

        protected override HttpConnectionContext CreateBuffer()
        {
            return new HttpConnectionContext(this);
        }
    }
}
