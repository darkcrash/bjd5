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
        const int bufferSizeXXXXL = 67108864;
        const int bufferSizeXXXL = 16777216;
        const int bufferSizeXXL = 4194304;
        const int bufferSizeXL = 1048576;
        const int bufferSizeL = 262144;
        const int bufferSizeM = 65536;
        const int bufferSizeS = 4096;
        const int bufferSizeXS = 1024;

        private readonly static CharsPool ExtraExtraExtraExtraLarge = new CharsPool(0, bufferSizeXXXXL);
        private readonly static CharsPool ExtraExtraExtraLarge = new CharsPool(0, bufferSizeXXXL);
        private readonly static CharsPool ExtraExtraLarge = new CharsPool(5, bufferSizeXXL);
        private readonly static CharsPool ExtraLarge = new CharsPool(20, bufferSizeXL);
        private readonly static CharsPool Large = new CharsPool(80, bufferSizeL);
        private readonly static CharsPool Medium = new CharsPool(320, bufferSizeM);
        private readonly static CharsPool Small = new CharsPool(1280, bufferSizeS);
        private readonly static CharsPool ExtraSmall = new CharsPool(5120, bufferSizeXS);

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

        private CharsPool(int pSize, int bSize) : base()
        {
            _bufferSize = bSize;
            InitializePool(pSize);
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
