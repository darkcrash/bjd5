using System;
using System.Collections.Generic;
using System.Text;

namespace Bjd.Common.Memory
{
    public struct BufferData : IDisposable
    {
        public byte[] Data;
        public int StartPos;
        public int EndPos;
        public int Length;
        private BufferPool _pool;
        internal BufferData(int length, BufferPool pool)
        {
            Length = length;
            Data = new byte[length];
            StartPos = 0;
            EndPos = 0;
            _pool = pool;
        }

        internal void Initialize()
        {
            StartPos = 0;
            EndPos = 0;
        }

        public void Dispose()
        {
            _pool.PoolInternal(ref this);
        }
        public void DisposeInternal()
        {
            Data = null;
            _pool = null;
        }
    }
}
