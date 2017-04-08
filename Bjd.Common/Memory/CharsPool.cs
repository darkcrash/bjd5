using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Memory
{
    public class CharsPool : PoolBase<CharsData>
    {
        const int bufferSizeXXXXL = 4096;
        const int bufferSizeXXXL = 2048;
        const int bufferSizeXXL = 1024;
        const int bufferSizeXL = 512;
        const int bufferSizeL = 256;
        const int bufferSizeM = 128;
        const int bufferSizeS = 64;
        const int bufferSizeXS = 32;

        const long L_bufferSizeXXXXL = bufferSizeXXXXL;
        const long L_bufferSizeXXXL = bufferSizeXXXL;
        const long L_bufferSizeXXL = bufferSizeXXL;
        const long L_bufferSizeXL = bufferSizeXL;
        const long L_bufferSizeL = bufferSizeL;
        const long L_bufferSizeM = bufferSizeM;
        const long L_bufferSizeS = bufferSizeS;
        const long L_bufferSizeXS = bufferSizeXS;

        private readonly static CharsPool ExtraExtraExtraExtraLarge = new CharsPool(0, 256, bufferSizeXXXXL);
        private readonly static CharsPool ExtraExtraExtraLarge = new CharsPool(0, 256, bufferSizeXXXL);
        private readonly static CharsPool ExtraExtraLarge;
        private readonly static CharsPool ExtraLarge;
        private readonly static CharsPool Large;
        private readonly static CharsPool Medium;
        private readonly static CharsPool Small;
        private readonly static CharsPool ExtraSmall;

        static CharsPool()
        {
            var L = System.Environment.ProcessorCount * 8 + 128;
            var M = System.Environment.ProcessorCount * 4 + 64;
            var S = System.Environment.ProcessorCount * 2 + 32;
            var c = 8;

            ExtraExtraLarge = new CharsPool(S, S * c, bufferSizeXXL);
            ExtraLarge = new CharsPool(S, S * c, bufferSizeXL);
            Large = new CharsPool(S, S * c, bufferSizeL);
            Medium = new CharsPool(M, M * c, bufferSizeM);
            Small = new CharsPool(L, L * c, bufferSizeS);
            ExtraSmall = new CharsPool(L, L * c, bufferSizeXS);
            Small.debugwrite = true;

        }


        public static CharsData Get(long length)
        {
            if (length <= L_bufferSizeXS) return ExtraSmall.Get();
            if (length <= L_bufferSizeS) return Small.Get();
            if (length <= L_bufferSizeM) return Medium.Get();
            if (length <= L_bufferSizeL) return Large.Get();
            return ExtraLarge.Get();
        }
        public static CharsData GetMaximum(long length)
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

        private CharsPool(int pSize, int pMaxSize, int bSize) : base()
        {
            _bufferSize = bSize;
            InitializePool(pSize, pMaxSize);
        }

        ~CharsPool()
        {
            Dispose();
        }

        protected override CharsData CreateBuffer()
        {
            return new CharsData(_bufferSize, this);
        }

        protected override int BufferSize => _bufferSize;

    }
}
