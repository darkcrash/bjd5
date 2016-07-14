using System;
using System.IO;
using System.Text;

namespace Bjd.Logs
{
    //生成時に１つのファイルをオープンしてset()で１行ずつ格納するクラス
    public class OneLogFile : IDisposable
    {
        private FileStream _fs;
        private StreamWriter _sw;
        private readonly string _fileName;
        private int disposeCount = 0;
        private object Lock = new object();

        public OneLogFile(String fileName)
        {
            _fileName = fileName;
            _fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            _sw = new StreamWriter(_fs, Encoding.UTF8);
            _fs.Seek(0, SeekOrigin.End);
        }

        public void Dispose()
        {
            lock (Lock)
            {
                disposeCount++;
                if (_sw != null)
                {
                    _sw.Flush();
                    _sw.Dispose();
                    _sw = null;
                }
                if (_fs != null)
                {
                    _fs.Dispose();
                    _fs = null;
                }
            }
        }

        public void Set(String str)
        {
            _sw.WriteLine(str);
            _sw.Flush();
        }

    }
}

