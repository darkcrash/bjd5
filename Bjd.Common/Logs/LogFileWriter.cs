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
        private int bufferSize = 16384;
        private bool isAsync = false;

        public LogFileWriter(string fileName)
        {
            _fileName = fileName;
            _fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, bufferSize, true);
            _sw = new StreamWriter(_fs, Encoding.UTF8, bufferSize, true);
            _fs.Seek(0, SeekOrigin.End);
            isAsync = _fs.IsAsync;
            FlushTask();
        }

        private void FlushTask()
        {
            if (_sw == null) return;
            if (isAsync)
            {
                _sw.FlushAsync()
                    .ContinueWith(_ => Task.Delay(500).Wait())
                    .ContinueWith(_ => FlushTask());
            }
            else
            {
                _sw.Flush();
                Task.Delay(500).ContinueWith(_ => FlushTask());
            }
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
            if (isAsync)
            {
                _sw.WriteLineAsync(message);
            }
            else
            {
                _sw.WriteLine(message);
            }
            //_sw.Flush();
        }
    }
}

