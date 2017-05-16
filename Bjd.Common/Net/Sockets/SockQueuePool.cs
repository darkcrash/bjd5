using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Bjd.Memory;

namespace Bjd.Net.Sockets
{
    public class SockQueuePool : PoolBase<SockQueue>
    {
        public readonly static SockQueuePool Instance = new SockQueuePool();
        const int poolSize = 16384;

        private SockQueuePool()
        {
            InitializePool(2000, poolSize);
        }

        ~SockQueuePool()
        {
            Dispose();
        }


        protected override SockQueue CreateBuffer()
        {
            return new SockQueue(this);
        }

        protected override int BufferSize => 1;

    }
}

