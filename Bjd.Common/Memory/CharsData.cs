using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Memory
{
    public class CharsData : IPoolBuffer
    {
        private static Dictionary<int, CharsData> _internal = new Dictionary<int, CharsData>(65536);
        private static int _internalCount = -1;
        public static readonly CharsData Empty = new CharsData(0, null);

        public char[] Data;
        public int DataSize;
        public readonly int Length;
        private CharsPool _pool;
        private GCHandle handle;
        private int byteCount = 0;
        private int Id = 0;

        public ref char this[int i] => ref Data[i];

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }


        public static bool operator ==(CharsData a, string b)
        {
            if (a is null && b == null) return true;
            if (a is null) return false;
            if (b is null) return false;

            if (a.DataSize != b.Length) return false;

            for (var i = 0; i < a.DataSize; i++)
            {
                if (a.Data[i] != b[i]) return false;
            }
            return true;
        }
        public static bool operator !=(CharsData a, string b)
        {
            return !(a == b);
        }

        internal CharsData(int length, CharsPool pool)
        {
            Length = length;
            Data = new char[length];
            DataSize = 0;
            _pool = pool;
            handle = GCHandle.Alloc(Data, GCHandleType.Pinned);
            byteCount = Buffer.ByteLength(Data);
            Id = Interlocked.Increment(ref _internalCount);
            _internal[Id] = this;
        }

        ~CharsData()
        {
            _internal[Id] = null;
            if (Data != null)
            {
                handle.Free();
                Data = null;
            }
            if (_pool != null)
            {
                _pool.Finalized(this);
                _pool = null;
            }
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


        public override string ToString()
        {
            return new string(Data, 0, DataSize);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
