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
        const int bufferSizeX = 16777216;
        const int bufferSizeXXL = 4194304;
        const int bufferSizeXL = 1048576;
        const int bufferSizeL = 262144;
        const int bufferSizeM = 65536;
        const int bufferSizeS = 4096;

        private readonly static BufferPool Extra = new BufferPool(0, bufferSizeX);
        private readonly static BufferPool ExtraExtraLarge = new BufferPool(5, bufferSizeXXL);
        private readonly static BufferPool ExtraLarge = new BufferPool(20, bufferSizeXL);
        private readonly static BufferPool Large = new BufferPool(80, bufferSizeL);
        private readonly static BufferPool Medium = new BufferPool(320, bufferSizeM);
        private readonly static BufferPool Small = new BufferPool(1280, bufferSizeS);

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
        public static BufferData Get(long length)
        {
            if (length <= bufferSizeS) return Small.Get();
            if (length <= bufferSizeM) return Medium.Get();
            if (length <= bufferSizeL) return Large.Get();
            return ExtraLarge.Get();
        }
        public static BufferData GetMaximum(long length)
        {
            if (length <= bufferSizeS) return Small.Get();
            if (length <= bufferSizeM) return Medium.Get();
            if (length <= bufferSizeL) return Large.Get();
            if (length <= bufferSizeXL) return ExtraLarge.Get();
            if (length <= bufferSizeXXL) return ExtraExtraLarge.Get();
            return Extra.Get();
        }

        private int _bufferSize;
        private ConcurrentBag<BufferData> _queue = new ConcurrentBag<BufferData>();
        private int _poolSize = 0;
        private int _Count = 0;
        private int _leaseCount = 0;

        private BufferPool(int pSize, int bSize)
        {
            _poolSize = pSize;
            _bufferSize = bSize;
            for (int i = 0; i < _poolSize; i++)
            {
                _Count++;
                _queue.Add(new BufferData(_bufferSize, this));
            }
            //Cleanup();
        }
        ~BufferPool()
        {
            Dispose();
        }

        public void Dispose()
        {
            while (!_queue.IsEmpty)
            {
                BufferData outQ;
                if (_queue.TryTake(out outQ)) outQ.DisposeInternal();
            }
        }

        public BufferData Get()
        {
            Interlocked.Increment(ref _leaseCount);
            BufferData outQ;
            if (_queue.TryTake(out outQ))
            {
                return outQ;
            }
            Interlocked.Increment(ref _Count);
            return new BufferData(_bufferSize, this);
        }

        public void PoolInternal(BufferData buf)
        {
            Interlocked.Decrement(ref _leaseCount);
            if (_poolSize < (_Count - _leaseCount))
            {
                Interlocked.Decrement(ref _Count);
                buf.DisposeInternal();
                return;
            }
            buf.Initialize();
            _queue.Add(buf);
        }

    }
}
