using System;

namespace Bjd.Net.Sockets
{
    //SockTcpで使用されるデータキュー
    public class SockQueue : IDisposable
    {
        byte[] _db = new byte[max]; //現在のバッファの内容
        int _dbEnd = 0;
        int _dbStart = 0;


        //private static int max = 1048560; //保持可能な最大数<=この辺りが適切な値かもしれない
        private const int max = 2000000; //保持可能な最大数
        //TODO modifyの動作に不安あり（これ必要なのか？） 
        bool _modify; //バッファに追加があった場合にtrueに変更される
        object _lock = new object();

        public int Max { get { return max; } }

        //空いているスペース
        //public int Space { get { return max - _db.Length; } }
        public int Space { get { return max - this.Length; } }

        //現在のキューに溜まっているデータ量
        //public int Length { get { return _db.Length; } }
        public int Length { get { if (_dbStart <= _dbEnd) return _dbEnd - _dbStart; return _dbEnd + (max - _dbStart); } }

        private int AfterSpace { get { return max - _dbEnd; } }
        private int BeforeSpace { get { return max - _dbStart; } }

        public ArraySegment<byte> GetWriteSegment()
        {
            return new ArraySegment<byte>(_db, _dbEnd, this.AfterSpace);
        }

        public void NotifyWrite(ArraySegment<byte> target, int len)
        {
            if (target.Array != this._db) return;
            lock (_lock)
            {
                _dbEnd += len;
                _modify = true; //データベースの内容が変化した
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
                    Buffer.BlockCopy(buf, 0, _db, _dbEnd, afterSpace);
                    _dbEnd = 0;

                    // 分割点取得
                    var splitLen = len - afterSpace;
                    Buffer.BlockCopy(buf, 0, _db, 0, splitLen);
                    _dbEnd += splitLen;

                }
                else
                {
                    // そのままコピー
                    Buffer.BlockCopy(buf, 0, _db, _dbEnd, len);
                    _dbEnd += len;
                }
                _modify = true; //データベースの内容が変化した
            }

            return len;

        }

        //キューからのデータ取得
        public byte[] Dequeue(int len)
        {
            //if (_db.Length == 0 || len == 0 || !_modify)
            if (this.Length == 0 || len == 0 || !_modify)
            {
                return new byte[0];
            }

            lock (_lock)
            {
                //要求サイズが現有数を超える場合はカットする
                if (this.Length < len)
                {
                    len = this.Length;
                }

                // 出力用バッファ
                var retBuf = new byte[len];


                var beforeSpace = this.BeforeSpace;
                if (beforeSpace < len)
                {
                    // 分割あり
                    Buffer.BlockCopy(_db, _dbStart, retBuf, 0, beforeSpace);
                    _dbStart = 0;

                    // 分割点取得
                    var splitLen = len - beforeSpace;
                    Buffer.BlockCopy(_db, _dbStart, retBuf, 0, beforeSpace);
                    _dbStart += splitLen;
                }
                else
                {
                    // 分割なし
                    Buffer.BlockCopy(_db, _dbStart, retBuf, 0, len);
                    _dbStart += len;
                    if (_dbStart >= max) _dbStart = 0;
                }

                if (this.Length == 0)
                {
                    // 次に何か受信するまで処理の必要はない
                    _modify = false;
                }
                return retBuf;
            }

        }

        //キューからの１行取り出し(\r\nを削除しない)
        public byte[] DequeueLine()
        {
            if (!_modify)
            {
                return new byte[0];
            }

            lock (_lock)
            {
                var ed = _dbEnd;

                var splited = _dbStart > ed;
                var end = (splited ? max - 1 : ed);

                // 分割なしの範囲
                for (var i = _dbStart; i < end; i++)
                {
                    if (_db[i] != '\n') continue;

                    // 出力サイズとバッファ
                    var len = i + 1 - _dbStart;
                    var retBuf = new byte[len]; //\r\nを削除しない

                    Buffer.BlockCopy(_db, _dbStart, retBuf, 0, len);
                    _dbStart += len;
                    if (_dbStart >= max) _dbStart = 0;

                    return retBuf;
                }

                // 分割ありの場合
                if (splited)
                {
                    for (var i = 0; i < ed; i++)
                    {
                        if (_db[i] != '\n') continue;

                        var splitCount = (max - _dbStart) - 1;
                        var splitEnd = i + 1;

                        // 出力サイズとバッファ
                        var len = splitEnd + splitCount;
                        var retBuf = new byte[len]; //\r\nを削除しない

                        Buffer.BlockCopy(_db, _dbStart, retBuf, 0, splitCount);
                        Buffer.BlockCopy(_db, 0, retBuf, splitCount, splitEnd);
                        _dbStart = splitEnd;

                        return retBuf;
                    }

                }

                _modify = false; //次に何か受信するまで処理の必要はない

            }

            return new byte[0];
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

