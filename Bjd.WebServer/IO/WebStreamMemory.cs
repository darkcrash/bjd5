using System;
using System.IO;

namespace Bjd.WebServer.IO
{

    class WebStreamMemory : IDisposable
    {
        readonly byte[] _buf;
        public int Length { get; private set; } //使用できる上限が決まっているのでintで対応できる
        int _pos;//使用できる上限が決まっているのでintで対応できる

        public WebStreamMemory(int limit)
        {
            _pos = 0;
            _buf = new byte[limit];

        }


        public void Dispose()
        {
        }
        public bool Add(byte[] b, int offset, int length)
        {
            if (Length + length > _buf.Length)
                return false;

            Buffer.BlockCopy(b, offset, _buf, Length, length);
            Length += length;

            return true;
        }
        public int Read(byte[] buffer, int offset, int count)
        {
            //mode==メモリの場合は、Posをintにキャストしても問題ない
            var len = _buf.Length - _pos; //残りのサイズ
            if (len > count)
            {
                len = count; //残りのサイズが読み出しサイズより大きい場合
            }
            Buffer.BlockCopy(_buf, _pos, buffer, offset, len);
            _pos += len;
            return len;
        }
        public byte[] GetBytes()
        {
            var b = new byte[Length];
            Buffer.BlockCopy(_buf, 0, b, 0, Length);
            return b;
        }
    }
}
