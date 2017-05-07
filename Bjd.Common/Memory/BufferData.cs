using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Bjd.Memory
{
    public unsafe class BufferData : IPoolBuffer
    {

        private static Dictionary<int, BufferData> _internal = new Dictionary<int, BufferData>(65536);
        private static int _internalCount = -1;
        public static readonly BufferData Empty = new BufferData(0, null);

        public byte[] Data;
        public int DataSize;
        public readonly int Length;
        private BufferPool _pool;
        private GCHandle handle;
        private int byteCount = 0;
        private int Id = 0;
        private byte* DataPoint = null;

        public ref byte this[int i] => ref Data[i];

        internal BufferData(int length, BufferPool pool)
        {
            Length = length;
            Data = new byte[length];
            _pool = pool;
            handle = GCHandle.Alloc(Data, GCHandleType.Pinned);
            byteCount = Buffer.ByteLength(Data);
            Id = Interlocked.Increment(ref _internalCount);
            _internal[Id] = this;
            if (length > 0)
            {
                fixed (byte* pt = &Data[0])
                {
                    DataPoint = pt;
                }
            }
        }

        ~BufferData()
        {
            _internal[Id] = null;
            if (Data != null)
            {
                handle.Free();
                Data = null;
                DataPoint = null;
            }
            if (_pool != null)
            {
                _pool.Finalized(this);
                _pool = null;
            }
        }

        public void CopyTo(BufferData destnation)
        {
            if (destnation == null) throw new NullReferenceException("destnation is null");
            Buffer.MemoryCopy(DataPoint, destnation.DataPoint, destnation.Length, DataSize);
            destnation.DataSize = DataSize;
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
            _internal[Id] = null;
            handle.Free();
            Data = null;
            _pool = null;
            GC.SuppressFinalize(this);
        }

        public void Pool()
        {
            Data.Initialize();
            _pool.PoolInternal(this);
        }


    }
}
