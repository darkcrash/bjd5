using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Bjd.Common.Memory
{
    public class CharsData : IPoolBuffer
    {
        public static readonly CharsData Empty = new CharsData(0, null);
        public char[] Data;
        public int DataSize;
        public readonly int Length;
        private CharsPool _pool;
        private GCHandle handle;

        public ref char this[int i] => ref Data[i];

        internal CharsData(int length, CharsPool pool)
        {
            Length = length;
            Data = new char[length];
            DataSize = 0;
            _pool = pool;
        }

        void IPoolBuffer.Initialize()
        {
            DataSize = 0;
            handle = GCHandle.Alloc(Data, GCHandleType.Pinned);
        }


        public void Dispose()
        {
            handle.Free();
            _pool.PoolInternal(this);
        }
        void IPoolBuffer.DisposeInternal()
        {
            Data = null;
            _pool = null;
        }
    }
}
