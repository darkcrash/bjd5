using System;

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


        //private static int max = 1048560; //保持可能な最大数<=この辺りが適切な値かもしれない
        private const int max = 2000000; //保持可能な最大数
        //TODO modifyの動作に不安あり（これ必要なのか？） 
        bool _modify; //バッファに追加があった場合にtrueに変更される
        System.Threading.ManualResetEventSlim _modifyEvent = new System.Threading.ManualResetEventSlim(false);
        object _lock = new object();

        public int Max { get { return max; } }

        //空いているスペース
        //public int Space { get { return max - _db.Length; } }
        public int Space { get { return max - _length; } }

        //現在のキューに溜まっているデータ量
        //public int Length { get { return _db.Length; } }
        public int Length { get { return _length; } }

        private int AfterSpace { get { return max - _dbNext; } }
        private int AfterLength { get { return max - _dbStart; } }

        private void SetModify(bool modify)
        {
            _modify = modify;
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
            return new ArraySegment<byte>(_db, _dbNext, this.AfterSpace);
        }

        public void NotifyWrite(int len)
        {
            //空きスペースを越える場合は失敗する 0が返される
            if (AfterSpace < len)
            {
                throw new ArgumentOutOfRangeException("queue overflow");
            }

            lock (_lock)
            {
                _dbNext += len;
                _length += len;
                if (_dbNext >= max) _dbNext = 0;
                //_modify = true; //データベースの内容が変化した
                SetModify(true);
            }
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

            lock (_lock)
            {
                // 追加データをコピー
                var afterSpace = this.AfterSpace;
                if (afterSpace < len)
                {
                    // 分割コピー
                    Buffer.BlockCopy(buf, 0, _db, _dbNext, afterSpace);
                    _dbNext = 0;

                    // 分割点取得
                    var splitLen = len - afterSpace;
                    Buffer.BlockCopy(buf, 0, _db, 0, splitLen);
                    _dbNext += splitLen;
                }
                else
                {
                    // そのままコピー
                    Buffer.BlockCopy(buf, 0, _db, _dbNext, len);
                    _dbNext += len;
                }
                _length += len;
                if (_dbNext >= max) _dbNext = 0;
                //_modify = true; //データベースの内容が変化した
                SetModify(true);
            }

            return len;

        }

        public byte[] DequeueWait(int len, int millisecondsTimeout)
        {
            var result = Dequeue(len);
            if (result == empty)
            {
                _modifyEvent.Wait(millisecondsTimeout);
                result = Dequeue(len);
            }
            return result;
        }

        //キューからのデータ取得
        public byte[] Dequeue(int len)
        {
            //if (_db.Length == 0 || len == 0 || !_modify)
            if (_length == 0 || len == 0 || !_modify)
            {
                return empty;
            }

            lock (_lock)
            {
                //要求サイズが現有数を超える場合はカットする
                if (_length < len)
                {
                    len = _length;
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

                _length -= len;
                if (_dbStart >= max) _dbStart = 0;

                if (_length == 0)
                {
                    // 次に何か受信するまで処理の必要はない
                    //_modify = false;
                    SetModify(false);
                }
                return retBuf;
            }

        }

        public byte[] DequeueLineWait(int millisecondsTimeout)
        {
            var result = DequeueLine();
            if (result == empty)
            {
                _modifyEvent.Wait(millisecondsTimeout);
                result = DequeueLine();
            }
            return result;
        }


        //キューからの１行取り出し(\r\nを削除しない)
        public byte[] DequeueLine()
        {
            if (!_modify)
            {
                return empty;
            }

            lock (_lock)
            {
                var splited = _dbStart > _dbNext || (_length == max && _dbStart == _dbNext);
                var end = (splited ? max : _dbNext);

                // 分割なしの範囲
                for (var i = _dbStart; i < end; i++)
                {
                    if (_db[i] != '\n') continue;

                    // 出力サイズとバッファ
                    var len = i + 1 - _dbStart;
                    var retBuf = new byte[len]; //\r\nを削除しない

                    Buffer.BlockCopy(_db, _dbStart, retBuf, 0, len);
                    _dbStart += len;
                    _length -= len;
                    if (_dbStart >= max) _dbStart = 0;

                    return retBuf;
                }

                // 分割ありの場合
                if (splited)
                {
                    for (var i = 0; i < _dbNext; i++)
                    {
                        if (_db[i] != '\n') continue;

                        // 出力サイズとバッファ
                        var splitCount = (max - _dbStart);
                        var splitEnd = i + 1;
                        var len = splitEnd + splitCount;
                        var retBuf = new byte[len]; //\r\nを削除しない

                        Buffer.BlockCopy(_db, _dbStart, retBuf, 0, splitCount);
                        Buffer.BlockCopy(_db, 0, retBuf, splitCount, splitEnd);
                        _dbStart = splitEnd;
                        _length -= len;

                        return retBuf;
                    }

                }

                //_modify = false; //次に何か受信するまで処理の必要はない
                SetModify(false);

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

