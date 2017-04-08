using Bjd.Logs;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private object ChangeZeroLock = new object();
        private GCHandle bufferHandle;

        internal bool debugwrite = false;

        protected PoolBase()
        {
            PoolList.Add(this);
            bufferHandle = GCHandle.Alloc(_buffers, GCHandleType.Pinned);
        }

        protected void InitializePool(int pSize, int pMaxSize)
        {
            _poolMaxSize = pMaxSize;
            _poolInitialSize = pSize;
            _buffers = new T[_poolMaxSize];
            for (int i = 0; i < _poolInitialSize; i++)
            {
                _Count++;
                _leaseCount++;
                PoolInternal(CreateBuffer());
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
            bufferHandle.Free();
        }

        public T Get()
        {
            Interlocked.Increment(ref _leaseCount);
            var p = Interlocked.Decrement(ref _poolCount);
            if (p >= 0)
            {
                var b = GetInternal();
                b.Initialize();
                return b;
            }
            Interlocked.Increment(ref _poolCount);
            Interlocked.Increment(ref _Count);
            var newB = CreateBuffer();
            newB.Initialize();
            return newB;
        }
        private T GetInternal()
        {
            var idx = Interlocked.Increment(ref _cursorDequeue);
            var idxMod = idx % _poolMaxSize;
            var b = Interlocked.Exchange(ref _buffers[idxMod], null);
            if (b == null)
            {
                Debug.WriteLine($"NotFound: {idxMod}, {this.ToConsoleString()}");
                b = GetInternal();
            }
            ExchangeZero(ref _cursorDequeue, idx, idxMod);
            return b;
        }


        public void PoolInternal(T buf)
        {
            if (buf == null) return;
            if (_poolMaxSize <= _poolCount)
            {
                Interlocked.Decrement(ref _Count);
                buf.DisposeInternal();
                Interlocked.Decrement(ref _leaseCount);
                return;
            }
            AddBuffer(buf);
            Interlocked.Decrement(ref _leaseCount);
            Interlocked.Increment(ref _poolCount);
        }

        private void AddBuffer(T buf)
        {
            var idx = Interlocked.Increment(ref _cursorEnqueue);
            var idxMod = idx % _poolMaxSize;
            var result = Interlocked.Exchange(ref _buffers[idxMod], buf);
            if (result != null)
            {
                Debug.WriteLine($"Duplication: {idxMod}, {this.ToConsoleString()}");
                AddBuffer(result);
            }
            ExchangeZero(ref _cursorEnqueue, idx, idxMod);
        }

        internal void Finalized(T buf)
        {
            Interlocked.Decrement(ref _leaseCount);
            Interlocked.Decrement(ref _Count);
        }

        private void ExchangeZero(ref int _cursor, int idx, int idxMod)
        {
            if (idx == idxMod) return;
            if (_poolMaxSize < _cursor)
            {
                if (System.Threading.Monitor.TryEnter(ChangeZeroLock))
                {
                    if (_poolMaxSize < _cursor)
                    {
                        Interlocked.Add(ref _cursor, -_poolMaxSize);
                    }
                    System.Threading.Monitor.Exit(ChangeZeroLock);
                }
            }
        }

        public string ToConsoleString()
        {
            return $"Size {BufferSize.ToString().PadLeft(10)} Pool(Now/Max) {_poolCount.ToString().PadLeft(5)} / {_poolMaxSize.ToString().PadLeft(5)} Use:{_leaseCount.ToString().PadLeft(5)} Total:{_Count.ToString().PadLeft(5)}";
        }

    }
}
