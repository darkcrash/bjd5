using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Bjd.Common.IO
{
    public class CachedReadonlyStream : Stream
    {
        private string _filePath;
        private FileStream _fs;

        public override bool CanRead => _fs.CanRead;

        public override bool CanSeek => _fs.CanSeek;

        public override bool CanWrite => _fs.CanWrite;

        public override long Length => _fs.Length;

        public override long Position { get => _fs.Position; set => _fs.Position = value; }

        internal CachedReadonlyStream(string filePath, FileStream stream) : base()
        {
            _filePath = filePath;
            _fs = stream;
            stream.Seek(0, SeekOrigin.Begin);
        }


        protected override void Dispose(bool disposing)
        {
            CachedFileStream.Poll(_filePath, _fs);
            _fs = null;
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            _fs.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _fs.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _fs.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _fs.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _fs.Write(buffer, offset, count);
        }
    }
}
