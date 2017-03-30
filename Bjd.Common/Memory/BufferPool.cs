using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Common.Memory
{
    public class BufferPool : IDisposable
    {
        const int bufferSizeXXXXL = 67108864;
        const int bufferSizeXXXL = 16777216;
        const int bufferSizeXXL = 4194304;
        const int bufferSizeXL = 1048576;
        const int bufferSizeL = 262144;
        const int bufferSizeM = 65536;
        const int bufferSizeS = 4096;
        const int bufferSizeXS = 1024;

        private readonly static BufferPool ExtraExtraExtraExtraLarge = new BufferPool(0, bufferSizeXXXXL);
        private readonly static BufferPool ExtraExtraExtraLarge = new BufferPool(0, bufferSizeXXXL);
        private readonly static BufferPool ExtraExtraLarge = new BufferPool(5, bufferSizeXXL);
        private readonly static BufferPool ExtraLarge = new BufferPool(20, bufferSizeXL);
        private readonly static BufferPool Large = new BufferPool(80, bufferSizeL);
        private readonly static BufferPool Medium = new BufferPool(320, bufferSizeM);
        private readonly static BufferPool Small = new BufferPool(1280, bufferSizeS);
        private readonly static BufferPool ExtraSmall = new BufferPool(5120, bufferSizeXS);

        public static BufferData GetExtraLarge()
        {
            return ExtraLarge.Get();
        }

        public static BufferData GetLarge()
        {
            return Large.Get();
        }
        public static BufferData GetMedium()
        {
            return Medium.Get();
        }
        public static BufferData GetSmall()
        {
            return Small.Get();
        }
        public static BufferData GetExtraSmall()
        {
            return ExtraSmall.Get();
        }
        public static BufferData Get(long length)
        {
            if (length <= bufferSizeXS) return ExtraSmall.Get();
            if (length <= bufferSizeS) return Small.Get();
            if (length <= bufferSizeM) return Medium.Get();
            if (length <= bufferSizeL) return Large.Get();
            return ExtraLarge.Get();
        }
        public static BufferData GetMaximum(long length)
        {
            if (length <= bufferSizeXS) return ExtraSmall.Get();
            if (length <= bufferSizeS) return Small.Get();
            if (length <= bufferSizeM) return Medium.Get();
            if (length <= bufferSizeL) return Large.Get();
            if (length <= bufferSizeXL) return ExtraLarge.Get();
            if (length <= bufferSizeXXL) return ExtraExtraLarge.Get();
            if (length <= bufferSizeXXXL) return ExtraExtraExtraLarge.Get();
            return ExtraExtraExtraExtraLarge.Get();
        }

        private int _bufferSize;
        //private ConcurrentBag<BufferData> _queue = new ConcurrentBag<BufferData>();
        private BufferData[] _buffers = new BufferData[10240];
        private object Lock = new object();
        private int _poolSize = 0;
        private int _Count = 0;
        private int _leaseCount = 0;
        private int _poolCount = 0;
        private int _cursorEnqueue = -1;
        private int _cursorDequeue = -1;

        private BufferPool(int pSize, int bSize)
        {
            _poolSize = pSize;
            _bufferSize = bSize;
            for (int i = 0; i < _poolSize; i++)
            {
                _Count++;
                _poolCount++;
                _cursorEnqueue++;
                //_queue.Add(b);
                _buffers[i] = new BufferData(_bufferSize, this);
            }
            //Cleanup();
        }
        ~BufferPool()
        {
            Dispose();
        }

        public void Dispose()
        {
            //while (!_queue.IsEmpty)
            //{
            //    BufferData outQ;
            //    if (_queue.TryTake(out outQ)) outQ.DisposeInternal();
            //}
            for (var i = 0; i < _buffers.Length; i++)
            {
                var b = _buffers[i];
                _buffers[i] = null;
                if (b != null) b.DisposeInternal();
            }
        }

        public BufferData Get()
        {
            Interlocked.Increment(ref _leaseCount);
            var p = Interlocked.Decrement(ref _poolCount);
            if (p >= 0)
            {
                var idx = Increment(ref _cursorDequeue);
                var b = _buffers[idx];
                _buffers[idx] = null;
                return b;
            }
            Interlocked.Increment(ref _poolCount);
            //BufferData outQ;
            //if (_queue.TryTake(out outQ))
            //{
            //    return outQ;
            //}

            Interlocked.Increment(ref _Count);
            return new BufferData(_bufferSize, this);
        }

        public void PoolInternal(BufferData buf)
        {
            Interlocked.Decrement(ref _leaseCount);
            if (_poolSize < _poolCount)
            {
                Interlocked.Decrement(ref _Count);
                buf.DisposeInternal();
                return;
            }
            buf.Initialize();
            //_queue.Add(buf);
            var idx = Increment(ref _cursorEnqueue);
            _buffers[idx] = buf;
            Interlocked.Increment(ref _poolCount);
        }

        private int Increment(ref int _cursor)
        {
            var idx = Interlocked.Increment(ref _cursor);
            if(idx >= _buffers.Length)
            {
                //idx = idx | ~_buffers.Length-1;
                idx = idx % _buffers.Length;
                Interlocked.CompareExchange(ref _cursor, idx, _buffers.Length + idx);
            }
            return idx;
        }


    }
}
