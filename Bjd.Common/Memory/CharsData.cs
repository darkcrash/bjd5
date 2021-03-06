﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Memory
{
    public unsafe class CharsData : IPoolBuffer
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
        private char* DataPoint = null;

        public ref char this[int i] => ref Data[i];

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public static bool operator ==(CharsData a, string b)
        {
            //if (a is null && b == null) return true;
            //if (a is null) return false;
            //if (b is null) return false;

            var dsa = a?.DataSize;
            var dsb = b?.Length;
            if (!dsa.HasValue && !dsb.HasValue) return true;

            if (dsa != dsb) return false;

            for (var i = 0; i < dsa; i++)
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
            if (length > 0)
            {
                DataPoint = (char*)handle.AddrOfPinnedObject().ToPointer();
                //fixed (char* pt = &Data[0])
                //{
                //    DataPoint = pt;
                //}
            }
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

        public void CopyTo(CharsData destnation, int offsetSource, int size)
        {
            if (destnation == null) throw new NullReferenceException("destnation is null");
            if (destnation.Length < size ) throw new OverflowException("destnation is overflow");
            if (DataSize < (size + offsetSource)) throw new OverflowException("source is overflow");
            var srcPt = DataPoint + (offsetSource);
            var dstPt = destnation.DataPoint;
            Buffer.MemoryCopy(srcPt, dstPt, size * 2, size * 2);
            destnation.DataSize = size;
        }

        public void CopyTo(CharsData destnation, int offsetSource, int offsetDestnation, int size)
        {
            if (destnation == null) throw new NullReferenceException("destnation is null");
            if (destnation.Length < (size + offsetDestnation)) throw new OverflowException("destnation is overflow");
            if (DataSize < (size + offsetSource)) throw new OverflowException("source is overflow");
            var srcPt = DataPoint + offsetSource;
            var dstPt = destnation.DataPoint + offsetDestnation;
            Buffer.MemoryCopy(srcPt, dstPt, size * 2, size * 2);
            destnation.DataSize = offsetDestnation + size;
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
            //return new string(Data, 0, DataSize);
            return new string(DataPoint, 0, DataSize);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
