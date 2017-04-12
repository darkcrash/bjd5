using Bjd.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bjd.Threading
{
    public class SimpleResetPool : PoolBase<SimpleResetEvent>
    {
        private readonly static SimpleResetPool Pool;

        static SimpleResetPool()
        {
            Pool = new SimpleResetPool();
        }

        public static SimpleResetEvent GetResetEvent()
        {
            return Pool.Get();
        }
        public static SimpleResetEvent GetResetEvent(bool initialState)
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

        private SimpleResetPool()
        {
            InitializePool(1024, 4096);
        }

        ~SimpleResetPool()
        {
            Dispose();
        }

        protected override int BufferSize => 1;

        protected override SimpleResetEvent CreateBuffer()
        {
            return new SimpleResetEvent(this);
        }
    }
}
