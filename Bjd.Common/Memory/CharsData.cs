using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bjd.Memory
{
    public class CharsData : IPoolBuffer
    {
        public static readonly CharsData Empty = new CharsData(0, null);
        public char[] Data;
        public int DataSize;
        public readonly int Length;
        private CharsPool _pool;
        private GCHandle handle;
        private int byteCount = 0;

        public ref char this[int i] => ref Data[i];

        internal CharsData(int length, CharsPool pool)
        {
            Length = length;
            Data = new char[length];
            DataSize = 0;
            _pool = pool;
            handle = GCHandle.Alloc(Data, GCHandleType.Pinned);
            byteCount = Buffer.ByteLength(Data);
        }

        void IPoolBuffer.Initialize()
        {
            DataSize = 0;
        }


        public void Dispose()
        {
            Data.Initialize();
            _pool.PoolInternal(this);
        }
        void IPoolBuffer.DisposeInternal()
        {
            handle.Free();
            Data = null;
            _pool = null;
            GC.SuppressFinalize(this);
        }



        public override string ToString()
        {
            return new string(Data, 0, DataSize);
        }
    }
}
