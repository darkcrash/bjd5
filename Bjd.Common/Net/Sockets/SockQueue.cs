using System;
using System.Threading;

namespace Bjd.Net.Sockets
{
    //SockTcpで使用されるデータキュー
    public class SockQueue : IDisposable
    {
        static byte[] empty = new byte[0];

        byte[] _db = new byte[max]; //現在のバッファの内容
        int _dbNext = 0;
        int _dbStart = 0;
        int _length = 0;

        int sendCounter = 0;
        int recvCounter = 0;
        int recvLength = 0;

        const byte Cr = 0x0d;
        const byte Lf = 0x0A;

        //private static int max = 1048560; //保持可能な最大数<=この辺りが適切な値かもしれない
        private const int max = 2000000; //保持可能な最大数

        ManualResetEventSlim _modifyEvent = new ManualResetEventSlim(false);
        object _lock = new object();

        public int Max { get { return max; } }

        //空いているスペース
        //public int Space { get { return max - _db.Length; } }
        public int Space { get { return max - _length; } }

        //現在のキューに溜まっているデータ量
        //public int Length { get { return _db.Length; } }
        public int Length { get { return _length; } }

        internal int AfterSpace { get { return max - _dbNext; } }
        internal int AfterLength { get { return max - _dbStart; } }

        internal void Initialize()
        {
            SetModify(false);
            _dbNext = 0;
            _dbStart = 0;
            _length = 0;
            sendCounter = 0;
            recvCounter = 0;
            recvLength = 0;
        }

        private void SetModify(bool modify)
        {
            if (modify)
            {
                _modifyEvent.Set();
            }
            else
            {
                _modifyEvent.Reset();
            }
        }

        public ArraySegment<byte> GetWriteSegment()
        {
            var sp = this.AfterSpace;
            var space = Space;
            if (space < sp) sp = space;
            return new ArraySegment<byte>(_db, _dbNext, sp);
        }

        public void NotifyWrite(int len)
        {
            //空きスペースを越える場合は失敗する 0が返される
            if (AfterSpace < len)
            {
                throw new ArgumentOutOfRangeException("queue overflow");
            }

            int workdbNext = _dbNext;
            workdbNext += len;

            if (workdbNext >= max) workdbNext = 0;
            _dbNext = workdbNext;

            //_length += len;
            System.Threading.Interlocked.Add(ref _length, len);

            if (sendCounter == int.MaxValue) sendCounter = 0;
            sendCounter++;

            //_modify = true; //データベースの内容が変化した
            SetModify(true);
        }

        //キューへの追加
        public int Enqueue(byte[] buf, int len)
        {

            if (Space == 0)
            {
                return 0;
            }
            //空きスペースを越える場合は失敗する 0が返される
            if (Space < len)
            {
                return 0;
            }

            // 追加データをコピー
            var afterSpace = this.AfterSpace;
            int workdbNext = _dbNext;
            if (afterSpace < len)
            {
                // 分割コピー
                Buffer.BlockCopy(buf, 0, _db, workdbNext, afterSpace);
                workdbNext = 0;

                // 分割点取得
                var splitLen = len - afterSpace;
                Buffer.BlockCopy(buf, 0, _db, 0, splitLen);
                workdbNext += splitLen;
            }
            else
            {
                // そのままコピー
                Buffer.BlockCopy(buf, 0, _db, workdbNext, len);
                workdbNext += len;
            }


            if (workdbNext >= max) workdbNext = 0;
            _dbNext = workdbNext;

            //_length += len;
            System.Threading.Interlocked.Add(ref _length, len);

            if (sendCounter == int.MaxValue) sendCounter = 0;
            sendCounter++;

            //_modify = true; //データベースの内容が変化した
            SetModify(true);

            return len;

        }

        public byte[] DequeueWait(int len, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return empty;
                var result = Dequeue(len);
                if (result == empty)
                {
                    try { if (!_modifyEvent.Wait(millisecondsTimeout, cancellationToken)) return empty; }
                    catch (OperationCanceledException) { return empty; }
                    continue;
                }
                return result;
            }
        }

        //キューからのデータ取得
        public byte[] Dequeue(int len)
        {
            //if (_db.Length == 0 || len == 0 || !_modify)
            //if (_length == 0 || len == 0 || !_modify)
            if ((recvLength == _length && recvCounter == sendCounter))
            {
                return empty;
            }
            // 次に何か受信するまで処理の必要はない
            SetModify(false);
            recvCounter = sendCounter;
            recvLength = _length;

            if (recvLength == 0 || len == 0)
            {
                return empty;
            }

            //要求サイズが現有数を超える場合はカットする
            if (recvLength < len)
            {
                len = recvLength;
            }

            // 出力用バッファ
            var retBuf = new byte[len];


            var afterLength = this.AfterLength;
            if (afterLength < len)
            {
                // 分割あり
                Buffer.BlockCopy(_db, _dbStart, retBuf, 0, afterLength);
                _dbStart = 0;

                // 分割点取得
                var splitLen = len - afterLength;
                Buffer.BlockCopy(_db, _dbStart, retBuf, afterLength, splitLen);
                _dbStart += splitLen;

            }
            else
            {
                // 分割なし
                Buffer.BlockCopy(_db, _dbStart, retBuf, 0, len);
                _dbStart += len;
            }

            //_length -= len;
            System.Threading.Interlocked.Add(ref _length, -len);
            if (_dbStart >= max) _dbStart = 0;

            return retBuf;

        }

        public byte[] DequeueLineWait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return empty;
                var result = DequeueLine();
                if (result == empty)
                {
                    try { if (!_modifyEvent.Wait(millisecondsTimeout, cancellationToken)) return empty; }
                    catch (OperationCanceledException) { return empty; }
                    continue;
                }
                return result;
            }
        }


        //キューからの１行取り出し(\r\nを削除しない)
        public byte[] DequeueLine()
        {
            if (recvLength == _length && recvCounter == sendCounter)
            {
                return empty;
            }
            //_modify = false; //次に何か受信するまで処理の必要はない
            SetModify(false);
            recvCounter = sendCounter;
            recvLength = _length;

            int dbNext = _dbNext;
            int dbStart = _dbStart;
            int length = recvLength;

            var splited = dbStart > dbNext || (length == max && dbStart == dbNext);
            var end = (splited ? max : dbNext);

            // 分割なしの範囲
            for (var i = dbStart; i < end; i++)
            {
                //if (_db[i] != '\n') continue;
                if (_db[i] != Lf) continue;

                // 出力サイズとバッファ
                var len = i + 1 - dbStart;
                var retBuf = new byte[len]; //\r\nを削除しない

                Buffer.BlockCopy(_db, dbStart, retBuf, 0, len);

                _dbStart += len;
                //_length -= len;
                System.Threading.Interlocked.Add(ref _length, -len);
                if (_dbStart >= max) _dbStart = 0;

                return retBuf;
            }

            // 分割ありの場合
            if (splited)
            {
                for (var i = 0; i < dbNext; i++)
                {
                    //if (_db[i] != '\n') continue;
                    if (_db[i] != Lf) continue;

                    // 出力サイズとバッファ
                    var splitCount = (max - dbStart);
                    var splitEnd = i + 1;
                    var len = splitEnd + splitCount;
                    var retBuf = new byte[len]; //\r\nを削除しない

                    Buffer.BlockCopy(_db, dbStart, retBuf, 0, splitCount);
                    Buffer.BlockCopy(_db, 0, retBuf, splitCount, splitEnd);


                    _dbStart = splitEnd;
                    //_length -= len;
                    System.Threading.Interlocked.Add(ref _length, -len);

                    return retBuf;
                }

            }


            return empty;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                _modifyEvent.Dispose();
                _modifyEvent = null;
                _db = null;

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }


        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}

