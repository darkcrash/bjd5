using Bjd.Memory;
using Bjd.Threading;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bjd.Logs
{
    //生成時に１つのファイルをオープンしてset()で１行ずつ格納するクラス
    public class LogFileWriter : IDisposable
    {
        private FileStream _fs;
        private StreamWriter _sw;
        private readonly string _fileName;
        private int disposeCount = 0;
        private object Lock = new object();
        private int bufferSize = 65536;

        public LogFileWriter(string fileName)
        {
            _fileName = fileName;
            _fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, bufferSize);
            _sw = new StreamWriter(_fs, Encoding.UTF8, bufferSize, true);
            _fs.Seek(0, SeekOrigin.End);
            FlushTask();
        }

        private void FlushTask()
        {
            if (_sw == null || disposeCount > 0) return;
            _sw.Flush();
            Task.Delay(1000).ContinueWith(_ => FlushTask());
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
                    _fs.Flush();
                    _fs.Dispose();
                    _fs = null;
                }
            }
        }

        public void WriteLine(string message)
        {
            if (_sw == null || disposeCount > 0) return;
            _sw.WriteLine(message);
        }

        public void WriteLine(CharsData message)
        {
            if (_sw == null || disposeCount > 0) return;
            _sw.WriteLine(message.Data, 0, message.DataSize);
        }

        public void WriteLine(StringBuilder message)
        {
            if (_sw == null || disposeCount > 0) return;
            using (var chars = message.ToCharsData())
            {
                _sw.WriteLine(chars.Data, 0, chars.DataSize);
            }
        }
    }
}

