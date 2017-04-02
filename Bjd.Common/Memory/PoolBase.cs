using Bjd.Logs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Memory
{
    public abstract class PoolBase<T> : IDisposable where T : class, IPoolBuffer
    {
        internal static Logger _log;
        internal static List<PoolBase<T>> PoolList = new List<PoolBase<T>>();

        private T[] _buffers;
        private int _poolInitialSize = 0;
        private int _poolMaxSize = 0;
        private int _Count = 0;
        private int _leaseCount = 0;
        private int _poolCount = 0;
        private int _cursorEnqueue = -1;
        private int _cursorDequeue = -1;

        protected PoolBase()
        {
            PoolList.Add(this);
        }

        protected void InitializePool(int pSize, int pMaxSize)
        {
            _poolMaxSize = pMaxSize;
            _poolInitialSize = pSize;
            _buffers = new T[_poolMaxSize];
            for (int i = 0; i < _poolInitialSize; i++)
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

        protected abstract int BufferSize { get; }

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
                if (b == null) {
                    b = CreateBuffer();
                }
                b.Initialize();
                _buffers[idx] = null;
                return b;
            }
            Interlocked.Increment(ref _poolCount);

            if (_log != null) _log.DebugInformation("CreateBuffer");
            Interlocked.Increment(ref _Count);
            var newB = CreateBuffer();
            newB.Initialize();
            return newB;
        }

        public void PoolInternal(T buf)
        {
            Interlocked.Decrement(ref _leaseCount);
            if (_poolMaxSize <= _poolCount)
            {
                if (_log != null) _log.DebugInformation("DisposeBuffer");
                Interlocked.Decrement(ref _Count);
                buf.DisposeInternal();
                return;
            }
            var idx = Increment(ref _cursorEnqueue);
            _buffers[idx] = buf;
            Interlocked.Increment(ref _poolCount);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Increment(ref int _cursor)
        {
            var idx = Interlocked.Increment(ref _cursor);
            if (idx >= _poolMaxSize)
            {
                idx = idx % _poolMaxSize;
                Interlocked.CompareExchange(ref _cursor, idx, _poolMaxSize + idx);
            }
            return idx;
        }

        public string ToConsoleString()
        {
            return $"Size {BufferSize.ToString().PadLeft(10)} Pool(Now/Max) {_poolCount.ToString().PadLeft(5)} / {_poolMaxSize.ToString().PadLeft(5)} Use:{_leaseCount.ToString().PadLeft(5)} Total:{_Count.ToString().PadLeft(5)}";
        }

    }
}
