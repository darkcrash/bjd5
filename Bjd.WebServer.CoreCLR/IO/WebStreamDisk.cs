using System;
using System.IO;

namespace Bjd.WebServer.IO
{
    class WebStreamDisk : IDisposable
    {
        public long Length { get; private set; } //long(約2Gbyteまで対応）
        long _pos; //long(約2Gbyteまで対応）
        private readonly FileStream _fs;
        private readonly string _fileName;
        public WebStreamDisk()
        {
            _pos = 0;
            _fileName = string.Format("{0}", Path.GetTempFileName());
            _fs = new FileStream(_fileName, FileMode.Create, FileAccess.ReadWrite);
        }
        public void Dispose()
        {
            _fs.Flush();
            //_fs.Close();
            _fs.Dispose();
            File.Delete(_fileName);
        }
        public void Flush()
        {
            _fs.Flush();
        }

        public bool Add(byte[] b, int offset, int length)
        {
            _fs.Write(b, offset, length);
            Length += length;
            return true;
        }
        public int Read(byte[] buffer, int offset, int count)
        {
            _fs.Seek(_pos, SeekOrigin.Begin); //ファイルの先頭にシークする
            var len = _fs.Length - _pos; //残りのサイズ(long)
            if (len > count)
            {
                len = count; //残りのサイズが読み出しサイズより大きい場合
            }
            if (len > 6553500)
            {
                len = 6553500; //intにキャストできるようにサイズを制限
            }
            _pos += len;
            return _fs.Read(buffer, offset, (int)len);
        }
        public byte[] GetBytes()
        {
            _fs.Seek(0, SeekOrigin.Begin); //ファイルの先頭にシークする
            var len = _fs.Length;//long
            if (len > Int32.MaxValue)
            {
                len = Int32.MaxValue; //intにキャストできるようにサイズを制限
            }
            var b = new byte[len];
            _fs.Read(b, 0, (int)len);
            return b;
        }
    }
}
