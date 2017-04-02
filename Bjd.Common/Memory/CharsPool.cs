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

        private readonly static CharsPool ExtraExtraExtraExtraLarge = new CharsPool(16, 1024, bufferSizeXXXXL);
        private readonly static CharsPool ExtraExtraExtraLarge = new CharsPool(16, 1024, bufferSizeXXXL);
        private readonly static CharsPool ExtraExtraLarge = new CharsPool(32, 4096, bufferSizeXXL);
        private readonly static CharsPool ExtraLarge = new CharsPool(32, 4096, bufferSizeXL);
        private readonly static CharsPool Large = new CharsPool(128, 16384, bufferSizeL);
        private readonly static CharsPool Medium = new CharsPool(128, 16384, bufferSizeM);
        private readonly static CharsPool Small = new CharsPool(512, 65536, bufferSizeS);
        private readonly static CharsPool ExtraSmall = new CharsPool(512, 65536, bufferSizeXS);


        public static CharsData Get(long length)
        {
            if (length <= bufferSizeXS) return ExtraSmall.Get();
            if (length <= bufferSizeS) return Small.Get();
            if (length <= bufferSizeM) return Medium.Get();
            if (length <= bufferSizeL) return Large.Get();
            return ExtraLarge.Get();
        }
        public static CharsData GetMaximum(long length)
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

    }
}
