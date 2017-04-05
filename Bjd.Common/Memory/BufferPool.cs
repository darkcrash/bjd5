using Bjd.Logs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Memory
{
    public class BufferPool : PoolBase<BufferData>
    {
        const int bufferSizeXXXXL = 67108864;
        const int bufferSizeXXXL = 16777216;
        const int bufferSizeXXL = 4194304;
        const int bufferSizeXL = 1048576;
        const int bufferSizeL = 262144;
        const int bufferSizeM = 65536;
        const int bufferSizeS = 4096;
        const int bufferSizeXS = 1024;

        private readonly static BufferPool ExtraExtraExtraExtraLarge = new BufferPool(0, 4, bufferSizeXXXXL);
        private readonly static BufferPool ExtraExtraExtraLarge = new BufferPool(0, 4, bufferSizeXXXL);
        private readonly static BufferPool ExtraExtraLarge = new BufferPool(0, 32, bufferSizeXXL);
        private readonly static BufferPool ExtraLarge = new BufferPool(0, 64, bufferSizeXL);
        private readonly static BufferPool Large = new BufferPool(32, 512, bufferSizeL);
        private readonly static BufferPool Medium = new BufferPool(128, 1024, bufferSizeM);
        private readonly static BufferPool Small = new BufferPool(256, 1024, bufferSizeS);
        private readonly static BufferPool ExtraSmall = new BufferPool(256, 1024, bufferSizeXS);

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

        private BufferPool(int pSize, int pMaxSize, int bSize) : base()
        {
            _bufferSize = bSize;
            InitializePool(pSize, pMaxSize);
        }
        ~BufferPool()
        {
            Dispose();
        }

        protected override BufferData CreateBuffer()
        {
            return new BufferData(_bufferSize, this);
        }

        protected override int BufferSize => _bufferSize;

    }
}
