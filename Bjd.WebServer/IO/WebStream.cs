using Bjd.Memory;
using System;
using System.IO;

namespace Bjd.WebServer.IO
{
    class WebStream : Stream, IDisposable
    {
        WebStreamDisk _disk;
        WebStreamMemory _memory;
        private readonly int _limit;
        private long _position = 0;

        public override bool CanRead { get { return true; } }
        public override bool CanWrite { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override long Position { get { return _position; } set { throw new NotSupportedException(); } }


        public override long Length
        {
            get
            {
                if (_disk != null)
                    return _disk.Length;
                return _memory.Length;
            }
        } //long(約2Gbyteまで対応）

        //最終的なサイズが分かっている場合は、limit(分からない場合は-1)を指定する
        public WebStream(int limit)
        {
            _limit = limit;

            if (_limit > 256000)
            {//サイズが大きい場合は、ファイルで保持する
                _disk = new WebStreamDisk();
                _memory = null;
            }
            else
            {//リミットが分からないときは、とりあえず256KByteで初期化する
                _disk = null;
                _memory = new WebStreamMemory((_limit < 0) ? 256000 : _limit);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disk != null)
                _disk.Dispose();
            if (_memory != null)
                _memory.Dispose();

            base.Dispose(disposing);
        }

        public override void Flush()
        {
            if (_disk != null)
                _disk.Flush();
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            var readSize = 0;
            if (_disk != null)
            {
                readSize = _disk.Read(buffer, offset, count);
            }
            else
            {
                readSize = _memory.Read(buffer, offset, count);
            }
            _position += readSize;
            return readSize;
        }
        public byte[] GetBytes()
        {
            if (_disk != null)
                return _disk.GetBytes();
            return _memory.GetBytes();
        }
        public bool Add(byte[] b)
        {
            if (b == null)
                return false;
            return Add2(b, 0, b.Length);
        }
        public bool Add(BufferData b)
        {
            if (b == null) return false;
            return Add2(b.Data, 0, b.DataSize);
        }

        public bool Add2(byte[] b, int offset, int length)
        {
            //b==nullの場合は、下位クラスに向かう前に、ここではじく
            if (b == null)
                return false;

            _position += length;

            if (_disk != null)
                return _disk.Add(b, offset, length);

            if (_limit == -1 || (_memory.Length + length) >= _limit)
            {
                //暫定の初期化でサイズをオーバした場合
                var buf = new byte[_memory.Length];
                _memory.Read(buf, 0, _memory.Length);
                _memory.Dispose();
                _memory = null;
                //ディスクに変更
                _disk = new WebStreamDisk();
                _disk.Add(buf, 0, buf.Length);
                return _disk.Add(b, offset, length);
            }
            return _memory.Add(b, offset, length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Add2(buffer, offset, count);
        }

    }
}
