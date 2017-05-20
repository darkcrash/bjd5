using Bjd.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bjd.Threading
{
    public class SimpleAsyncAwaiterPool : PoolBase<SimpleAsyncAwaiter>
    {
        private readonly static SimpleAsyncAwaiterPool Pool;

        static SimpleAsyncAwaiterPool()
        {
            Pool = new SimpleAsyncAwaiterPool();
        }

        public static SimpleAsyncAwaiter GetResetEvent()
        {
            return Pool.Get();
        }
        public static SimpleAsyncAwaiter GetResetEvent(bool initialState)
        {
            var b = Pool.Get();
            if (initialState)
            {
                b.Set();
            }
            else
            {
                b.Reset();
            }
            return b;

        }

        private SimpleAsyncAwaiterPool()
        {
            InitializePool(2048, 32768);
        }

        ~SimpleAsyncAwaiterPool()
        {
            Dispose();
        }

        protected override int BufferSize => 1;

        protected override SimpleAsyncAwaiter CreateBuffer()
        {
            return new SimpleAsyncAwaiter(this);
        }
    }
}
