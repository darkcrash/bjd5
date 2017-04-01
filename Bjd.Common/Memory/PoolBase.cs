using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Memory
{
    public abstract class PoolBase<T> : IDisposable where T : class, IPoolBuffer
    {

        private T[] _buffers;
        private int _poolSize = 0;
        private int _Count = 0;
        private int _leaseCount = 0;
        private int _poolCount = 0;
        private int _cursorEnqueue = -1;
        private int _cursorDequeue = -1;

        protected PoolBase()
        {
        }

        protected void InitializePool(int pSize, int pMaxSize)
        {
            _poolSize = pMaxSize;
            _buffers = new T[_poolSize];
            for (int i = 0; i < pSize; i++)
            {
                _Count++;
                _poolCount++;
                _cursorEnqueue++;
                _buffers[i] = CreateBuffer();
            }
        }

        ~PoolBase()
        {
            Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract T CreateBuffer();

        public void Dispose()
        {
            for (var i = 0; i < _buffers.Length; i++)
            {
                var b = _buffers[i];
                _buffers[i] = null;
                if (b != null) b.DisposeInternal();
            }
        }

        public T Get()
        {
            Interlocked.Increment(ref _leaseCount);
            var p = Interlocked.Decrement(ref _poolCount);
            if (p >= 0)
            {
                var idx = Increment(ref _cursorDequeue);
                var b = _buffers[idx];
                b.Initialize();
                _buffers[idx] = null;
                return b;
            }
            Interlocked.Increment(ref _poolCount);
            
            Interlocked.Increment(ref _Count);
            var newB = CreateBuffer();
            newB.Initialize();
            return newB;
        }

        public void PoolInternal(T buf)
        {
            Interlocked.Decrement(ref _leaseCount);
            if (_poolSize <= _poolCount)
            {
                Interlocked.Decrement(ref _Count);
                buf.DisposeInternal();
                return;
            }
            var idx = Increment(ref _cursorEnqueue);
            _buffers[idx] = buf;
            Interlocked.Increment(ref _poolCount);
        }

        private int Increment(ref int _cursor)
        {
            var idx = Interlocked.Increment(ref _cursor);
            if (idx >= _buffers.Length)
            {
                //idx = idx | ~_buffers.Length-1;
                idx = idx % _buffers.Length;
                Interlocked.CompareExchange(ref _cursor, idx, _buffers.Length + idx);
            }
            return idx;
        }


    }
}
