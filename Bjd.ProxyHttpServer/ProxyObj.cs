﻿using System;
using Bjd;
using Bjd.Threading;
using System.Threading.Tasks;

namespace Bjd.ProxyHttpServer
{
    abstract class ProxyObj : IDisposable
    {
        protected Proxy Proxy;

        protected ProxyObj(Proxy proxy)
        {
            Proxy = proxy;
        }
        public abstract void Dispose();
        public abstract bool IsFinish();
        public abstract bool IsTimeout();
        public abstract void Add(OneObj oneObj);
        public abstract void DebugLog();
        public abstract bool Pipe(ILife iLife);
        public abstract ValueTask<bool> PipeAsync(ILife iLife);

        public virtual bool WaitProcessing()
        {
            if (Proxy.Sock(CS.Client).Length() != 0)
                return true;
            if (Proxy.Sock(CS.Server).Length() != 0)
                return true;
            return false;
        }

    }
}
