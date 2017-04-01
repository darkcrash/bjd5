using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Memory
{
    public interface IPoolBuffer : IDisposable
    {
        void Initialize();
        void DisposeInternal();
    }
}
