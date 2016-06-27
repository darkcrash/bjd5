using System;

namespace Bjd.Net.Sockets
{
    //SockTcpで使用されるデータキュー
    public class SockQueue :IDisposable
    {
        //byte[] _db = new byte[0]; //現在のバッファの内容
        byte[] _db1 = new byte[max]; //現在のバッファの内容
        byte[] _db2 = new byte[max]; //退避のバッファの内容
        int _dbp = 0;

        //private static int max = 1048560; //保持可能な最大数<=この辺りが適切な値かもしれない
        private const int max = 2000000; //保持可能な最大数
        //TODO modifyの動作に不安あり（これ必要なのか？） 
        bool _modify; //バッファに追加があった場合にtrueに変更される
        object _lock = new object();

        public int Max { get { return max; } }

        //空いているスペース
        //public int Space { get { return max - _db.Length; } }
        public int Space { get { return max - _dbp; } }

        //現在のキューに溜まっているデータ量
        //public int Length { get { return _db.Length; } }
        public int Length { get { return _dbp; } }

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
                //var tmpBuf = new byte[_db.Length + len]; //テンポラリバッファ
                //Buffer.BlockCopy(_db, 0, tmpBuf, 0, _db.Length);//現有DBのデータをテンポラリ前部へコピー
                //Buffer.BlockCopy(buf, 0, tmpBuf, _db.Length, len);//追加のデータをテンポラリ後部へコピー
                //_db = tmpBuf; //テンポラリを現用DBへ変更


                Buffer.BlockCopy(buf, 0, _db1, _dbp, len);
                _dbp += len;


                _modify = true; //データベースの内容が変化した
            }
            return len;

        }

        //キューからのデータ取得
        public byte[] Dequeue(int len)
        {
            //if (_db.Length == 0 || len == 0 || !_modify)
            if (_dbp == 0 || len == 0 || !_modify)
            {
                return new byte[0];
            }

            lock (_lock)
            {
                ////要求サイズが現有数を超える場合はカットする
                //if (_db.Length < len)
                //{
                //    len = _db.Length;
                //}
                //var retBuf = new byte[len]; //出力用バッファ
                //var tmpBuf = new byte[_db.Length - len]; //テンポラリバッファ
                //Buffer.BlockCopy(_db, 0, retBuf, 0, len);//現有DBから出力用バッファへコピー
                //Buffer.BlockCopy(_db, len, tmpBuf, 0, _db.Length - len);//残りのデータをテンポラリへ
                //_db = tmpBuf; //テンポラリを現用DBへ変更

                //if (_db.Length == 0)
                //{
                //    _modify = false; //次に何か受信するまで処理の必要はない
                //}

                //要求サイズが現有数を超える場合はカットする
                if (_dbp < len)
                {
                    len = _dbp;
                }
                var retBuf = new byte[len]; //出力用バッファ

                Buffer.BlockCopy(_db1, 0, retBuf, 0, len);
                Buffer.BlockCopy(_db1, 0, _db2, 0, _dbp);
                _dbp -= len;
                Buffer.BlockCopy(_db2, len, _db1, 0, _dbp);

                if (_dbp == 0)
                {
                    _modify = false; //次に何か受信するまで処理の必要はない
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
                //for (var i = 0; i < _db.Length; i++)
                //{
                //    if (_db[i] != '\n')
                //    {
                //        continue;
                //    }
                //    var retBuf = new byte[i + 1]; //\r\nを削除しない
                //    Buffer.BlockCopy(_db, 0, retBuf, 0, i + 1);//\r\nを削除しない
                //    var tmpBuf = new byte[_db.Length - (i + 1)]; //テンポラリバッファ
                //    Buffer.BlockCopy(_db, (i + 1), tmpBuf, 0, _db.Length - (i + 1));//残りのデータをテンポラリへ
                //    _db = tmpBuf; //テンポラリを現用DBへ変更

                //    return retBuf;

                //}

                for (var i = 0; i < _dbp; i++)
                {
                    if (_db1[i] != '\n')
                    {
                        continue;
                    }
                    var len = i + 1;
                    var retBuf = new byte[len]; //\r\nを削除しない
                    Buffer.BlockCopy(_db1, 0, retBuf, 0, len);
                    Buffer.BlockCopy(_db1, 0, _db2, 0, _dbp);
                    _dbp -= len;
                    Buffer.BlockCopy(_db2, len, _db1, 0, _dbp);

                    return retBuf;

                }

                _modify = false; //次に何か受信するまで処理の必要はない
            }
            return new byte[0];
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected  void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                _db1 = null;
                _db2 = null;

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

