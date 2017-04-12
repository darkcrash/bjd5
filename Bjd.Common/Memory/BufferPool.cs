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

        const long L_bufferSizeXXXXL = bufferSizeXXXXL;
        const long L_bufferSizeXXXL = bufferSizeXXXL;
        const long L_bufferSizeXXL = bufferSizeXXL;
        const long L_bufferSizeXL = bufferSizeXL;
        const long L_bufferSizeL = bufferSizeL;
        const long L_bufferSizeM = bufferSizeM;
        const long L_bufferSizeS = bufferSizeS;
        const long L_bufferSizeXS = bufferSizeXS;

        private readonly static BufferPool ExtraExtraExtraExtraLarge = new BufferPool(0, 4, bufferSizeXXXXL);
        private readonly static BufferPool ExtraExtraExtraLarge = new BufferPool(0, 4, bufferSizeXXXL);
        private readonly static BufferPool ExtraExtraLarge = new BufferPool(0, 32, bufferSizeXXL);
        private readonly static BufferPool ExtraLarge = new BufferPool(0, 64, bufferSizeXL);
        private readonly static BufferPool Large;
        private readonly static BufferPool Medium;
        private readonly static BufferPool Small;
        private readonly static BufferPool ExtraSmall;

        static BufferPool()
        {
            var L = System.Environment.ProcessorCount * 64 + 256;
            var M = System.Environment.ProcessorCount * 32 + 128;
            var S = System.Environment.ProcessorCount * 16 + 64;
            var c = 8;

            Large = new BufferPool(S, S * c, bufferSizeL);
            Medium = new BufferPool(M, M * c, bufferSizeM);
            Small = new BufferPool(L, L * c, bufferSizeS);
            ExtraSmall = new BufferPool(L, L * c, bufferSizeXS);

        }

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
            if (length <= L_bufferSizeXS) return ExtraSmall.Get();
            if (length <= L_bufferSizeS) return Small.Get();
            if (length <= L_bufferSizeM) return Medium.Get();
            if (length <= L_bufferSizeL) return Large.Get();
            return ExtraLarge.Get();
        }
        public static BufferData GetMaximum(long length)
        {
            if (length <= L_bufferSizeXS) return ExtraSmall.Get();
            if (length <= L_bufferSizeS) return Small.Get();
            if (length <= L_bufferSizeM) return Medium.Get();
            if (length <= L_bufferSizeL) return Large.Get();
            if (length <= L_bufferSizeXL) return ExtraLarge.Get();
            if (length <= L_bufferSizeXXL) return ExtraExtraLarge.Get();
            if (length <= L_bufferSizeXXXL) return ExtraExtraExtraLarge.Get();
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
