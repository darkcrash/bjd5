using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Bjd.Memory
{
    public class BufferData : IPoolBuffer
    {
        public static readonly BufferData Empty = new BufferData(0, null);
        public byte[] Data;
        public int DataSize;
        public readonly int Length;
        private BufferPool _pool;
        private GCHandle handle;
        private int byteCount = 0;

        public ref byte this[int i] => ref Data[i];

        internal BufferData(int length, BufferPool pool)
        {
            Length = length;
            Data = new byte[length];
            _pool = pool;
            handle = GCHandle.Alloc(Data, GCHandleType.Pinned);
            byteCount = Buffer.ByteLength(Data);
        }

        void IPoolBuffer.Initialize()
        {
            DataSize = 0;
        }


        public ArraySegment<byte> GetSegment()
        {
            return new ArraySegment<byte>(Data, 0, DataSize);
        }

        public ArraySegment<byte> GetSegment(int capacity)
        {
            var l = capacity;
            if (l > Length) l = Length;
            return new ArraySegment<byte>(Data, 0, l);
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
    }
}
