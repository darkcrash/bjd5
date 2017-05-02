using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using Bjd.Memory;
using Bjd.Threading;
using System.Threading.Tasks;

namespace Bjd.Net.Sockets
{
    //SockTcpで使用されるデータキュー
    public class SockQueue : IDisposable
    {
        static byte[] empty = new byte[0];

        const int MaxBlockSize = 2048;
        const byte Cr = 0x0D;
        const byte Lf = 0x0A;


        // 現在のバッファの内容
        BufferData[] _blocks = new BufferData[MaxBlockSize];
        int _nextBlocks = 0;
        int _readBlocks = 0;
        int _useBlocks = 0;

        BufferData _current = null;

        int _length = 0;
        long _totallength = 0;
        int beforeSize = int.MaxValue;
        int lfCount = 0;
        object lfLock = new object();

        int enqueueCounter = 0;
        int dequeueCounter = 0;
        int recvLength = 0;
        int recvUseBlocks = 0;
        bool recvMust = false;
        List<int> alloc = new List<int>(MaxBlockSize);

        //private static int max = 1048560; //保持可能な最大数<=この辺りが適切な値かもしれない
        //private const int max = 2000000; //保持可能な最大数
        private const int max = 4000000;

        //ManualResetEventSlim _modifyEvent = new ManualResetEventSlim(false);
        //ManualResetEventSlim _modifyEvent = new ManualResetEventSlim(false, 0);
        SimpleResetEvent _modifyEvent = SimpleResetPool.GetResetEvent(false);
        SimpleResetEvent _modifyLfEvent = SimpleResetPool.GetResetEvent(false);

        public int Max { get { return max; } }

        public int UseBlocks { get { return _useBlocks; } }

        //空いているスペース
        public int Space { get { return max - _length; } }

        //現在のキューに溜まっているデータ量
        public int Length { get { return _length; } }

        internal void Initialize()
        {
            SetModify(false);
            _modifyLfEvent.Reset();
            lfCount = 0;
            _length = 0;
            _totallength = 0;
            alloc.Clear();

            if (_current != null) _current.Dispose();
            _current = null;
            _nextBlocks = 0;
            _readBlocks = 0;
            _useBlocks = 0;

            enqueueCounter = 0;
            dequeueCounter = 0;
            recvLength = 0;
            recvUseBlocks = 0;
            recvMust = false;

            for (var i = 0; i < MaxBlockSize; i++)
            {
                if (_blocks[i] == null) continue;
                _blocks[i].Dispose();
                _blocks[i] = null;
            }
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

        private void SetLfModify()
        {
            var isLock = (lfCount == 0);
            if (_modifyLfEvent.IsLocked == isLock) return;
            lock (lfLock)
            {
                if (lfCount > 0)
                {
                    _modifyLfEvent.Set();
                }
                else
                {
                    _modifyLfEvent.Reset();
                }
            }

        }


        public ArraySegment<byte> GetWriteSegment()
        {
            if (Space == 0)
            {
                throw new OverflowException("space overflow");
            }

            if (_current != null)
            {
                _current.Dispose();
                //throw new InvalidOperationException("conflict GetWriteSegment");
            }
            var sp = _totallength;
            if (sp > Space) sp = Space;
            if (sp > 65536) sp = 65536;
            if (sp > beforeSize * 4) sp = beforeSize * 4;
            _current = BufferPool.Get(sp);
            return _current.GetSegment(Space);
        }

        public void NotifyWrite(int len)
        {
            //空きスペースを越える場合は失敗する
            if (Space < len)
            {
                throw new OverflowException("queue overflow");
            }

            if (_current == null)
            {
                throw new InvalidOperationException("not call GetWriteSegment");
            }

            if (MaxBlockSize <= _useBlocks)
            {
                throw new OverflowException("queue block overflow");
            }

            var buf = _current;
            _current = null;

            // empty
            if (len == 0)
            {
                buf.Dispose();
                return;
            }

            if (buf.Length < len)
            {
                buf.Dispose();
                throw new OverflowException("queue block data overflow");
            }

            buf.DataSize = len;
            _blocks[_nextBlocks] = buf;
            if (_nextBlocks++ >= MaxBlockSize)
            {
                _nextBlocks = 0;
            }
            System.Threading.Interlocked.Add(ref _length, len);
            System.Threading.Interlocked.Increment(ref _useBlocks);
            _totallength += len;

            if (enqueueCounter == int.MaxValue) enqueueCounter = 0;
            enqueueCounter++;

            beforeSize = len;

            //データベースの内容が変化した
            SetModify(true);
            var cntLf = buf.CountLf();
            Interlocked.Add(ref lfCount, cntLf);
            SetLfModify();

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
            int leftOversSize = len;
            int cntLf = 0;

            while (leftOversSize > 0)
            {
                var size = leftOversSize;
                var b = BufferPool.Get(len);
                if (size > b.Length) size = b.Length;
                Buffer.BlockCopy(buf, 0, b.Data, 0, size);
                b.DataSize = size;
                cntLf += b.CountLf();

                //while (!_db.TryAdd(b)) { }
                _blocks[_nextBlocks++] = b;
                if (_nextBlocks >= MaxBlockSize)
                {
                    _nextBlocks = 0;
                }
                System.Threading.Interlocked.Increment(ref _useBlocks);

                leftOversSize -= size;
            }


            System.Threading.Interlocked.Add(ref _length, len);
            _totallength += len;

            if (enqueueCounter == int.MaxValue) enqueueCounter = 0;
            enqueueCounter++;

            //_modify = true; //データベースの内容が変化した
            SetModify(true);
            Interlocked.Add(ref lfCount, cntLf);
            SetLfModify();

            return len;


        }

        public int Enqueue(BufferData buf)
        {
            if (Space == 0)
            {
                return 0;
            }
            //空きスペースを越える場合は失敗する 0が返される
            if (Space < buf.DataSize)
            {
                return 0;
            }

            var len = buf.DataSize;

            _blocks[_nextBlocks] = buf;

            _nextBlocks++;
            if (_nextBlocks >= MaxBlockSize)
            {
                _nextBlocks = 0;
            }

            System.Threading.Interlocked.Increment(ref _useBlocks);
            System.Threading.Interlocked.Add(ref _length, len);
            _totallength += len;

            if (enqueueCounter == int.MaxValue) enqueueCounter = 0;
            enqueueCounter++;

            //_modify = true; //データベースの内容が変化した
            SetModify(true);
            var cntLf = buf.CountLf();
            Interlocked.Add(ref lfCount, cntLf);
            SetLfModify();

            return len;


        }

        public int EnqueueImport(BufferData buf)
        {
            if (Space == 0)
            {
                return 0;
            }
            //空きスペースを越える場合は失敗する 0が返される
            if (Space < buf.DataSize)
            {
                return 0;
            }

            var len = buf.DataSize;

            var newBuf = BufferPool.GetMaximum(len);
            newBuf.DataSize = len;
            Buffer.BlockCopy(buf.Data, 0, newBuf.Data, 0, len);

            _blocks[_nextBlocks] = newBuf;

            _nextBlocks++;
            if (_nextBlocks >= MaxBlockSize)
            {
                _nextBlocks = 0;
            }

            System.Threading.Interlocked.Increment(ref _useBlocks);
            System.Threading.Interlocked.Add(ref _length, len);
            _totallength += len;

            if (enqueueCounter == int.MaxValue) enqueueCounter = 0;
            enqueueCounter++;

            //_modify = true; //データベースの内容が変化した
            SetModify(true);
            var cntLf = buf.CountLf();
            Interlocked.Add(ref lfCount, cntLf);
            SetLfModify();

            return len;


        }


        public byte[] DequeueWait(int len, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            ////要求サイズが現有数を超える場合は待機する
            //var result = Dequeue(len, true);
            //if (result != empty) return result;
            //bool wait;
            //try { wait = _modifyEvent.Wait(millisecondsTimeout, cancellationToken); }
            //catch (OperationCanceledException) { return empty; }
            //return Dequeue(len, false);

            while (true)
            {
                if (len == 0) return empty;
                if (cancellationToken.IsCancellationRequested) return empty;
                var result = Dequeue(len, true);
                if (result == empty)
                {
                    try { if (!_modifyEvent.Wait(millisecondsTimeout, cancellationToken)) return Dequeue(len, false); }
                    catch (OperationCanceledException) { return empty; }
                    continue;
                }
                return result;
            }
        }

        //キューからのデータ取得
        public byte[] Dequeue(int len)
        {
            return Dequeue(len, false);
        }

        //キューからのデータ取得
        private byte[] Dequeue(int len, bool must)
        {
            if (len == 0) return empty;
            if (recvMust == must && recvLength == _length && dequeueCounter == enqueueCounter && recvUseBlocks == _useBlocks)
            {
                return empty;
            }
            // 次に何か受信するまで処理の必要はない
            SetModify(false);
            recvLength = _length;
            dequeueCounter = enqueueCounter;
            recvUseBlocks = _useBlocks;
            recvMust = must;

            if (recvLength == 0)
            {
                return empty;
            }

            //要求サイズが現有数を超える場合はカットする
            if (recvLength < len)
            {
                // 要求サイズまで待機ならEmptyを返す
                if (must) return empty;
                len = recvLength;
            }

            // 出力用バッファ
            var retBuf = new byte[len];
            var writeSize = 0;
            var leftOversSize = len;

            while (leftOversSize > 0)
            {
                var size = leftOversSize;
                var b = _blocks[_readBlocks];


                if (size >= b.DataSize)
                {
                    size = b.DataSize;
                    _blocks[_readBlocks] = null;
                    _readBlocks++;
                    if (_readBlocks >= MaxBlockSize)
                    {
                        _readBlocks = 0;
                    }
                    Buffer.BlockCopy(b.Data, 0, retBuf, writeSize, size);
                    b.Dispose();
                    System.Threading.Interlocked.Decrement(ref _useBlocks);
                }
                else
                {
                    Buffer.BlockCopy(b.Data, 0, retBuf, writeSize, size);
                    var tempSize = b.DataSize - size;
                    Buffer.BlockCopy(b.Data, size, b.Data, 0, tempSize);
                    b.DataSize = tempSize;
                }
                writeSize += size;
                leftOversSize -= size;


            }

            alloc.Clear();
            System.Threading.Interlocked.Add(ref _length, -len);

            return retBuf;

        }


        public BufferData DequeueBufferWait(int len, int millisecondsTimeout, CancellationToken cancellationToken)
        {

            while (true)
            {
                if (len == 0) return BufferData.Empty;
                if (cancellationToken.IsCancellationRequested) return BufferData.Empty;
                var result = DequeueBuffer(len, false);
                if (result == BufferData.Empty)
                {
                    try { if (!_modifyEvent.Wait(millisecondsTimeout, cancellationToken)) return DequeueBuffer(len, false); }
                    catch (OperationCanceledException) { return BufferData.Empty; }
                    continue;
                }
                return result;
            }
        }

        //キューからのデータ取得
        public BufferData DequeueBuffer(int len)
        {
            return DequeueBuffer(len, false);
        }


        //キューからのデータ取得
        private BufferData DequeueBuffer(int len, bool must)
        {
            if (len == 0) return BufferData.Empty;
            if (recvMust == must && recvLength == _length && dequeueCounter == enqueueCounter && recvUseBlocks == _useBlocks)
            {
                return BufferData.Empty;
            }
            // 次に何か受信するまで処理の必要はない
            SetModify(false);
            recvLength = _length;
            dequeueCounter = enqueueCounter;
            recvUseBlocks = _useBlocks;
            recvMust = must;

            if (recvLength == 0)
            {
                return BufferData.Empty;
            }

            //要求サイズが現有数を超える場合はカットする
            if (recvLength < len)
            {
                if (must) return BufferData.Empty;
                len = recvLength;
            }

            // 出力用バッファ
            //var retBuf = new byte[len];
            var retBuf = BufferPool.GetMaximum(len);
            var writeSize = 0;
            var leftOversSize = len;

            while (leftOversSize > 0)
            {
                var size = leftOversSize;
                var b = _blocks[_readBlocks];


                if (size >= b.DataSize)
                {
                    size = b.DataSize;
                    _blocks[_readBlocks] = null;
                    _readBlocks++;
                    if (_readBlocks >= MaxBlockSize)
                    {
                        _readBlocks = 0;
                    }
                    Buffer.BlockCopy(b.Data, 0, retBuf.Data, writeSize, size);
                    b.Dispose();
                    System.Threading.Interlocked.Decrement(ref _useBlocks);
                }
                else
                {
                    Buffer.BlockCopy(b.Data, 0, retBuf.Data, writeSize, size);
                    var tempSize = b.DataSize - size;
                    Buffer.BlockCopy(b.Data, size, b.Data, 0, tempSize);
                    b.DataSize = tempSize;
                }
                writeSize += size;
                leftOversSize -= size;


            }

            retBuf.DataSize = len;

            alloc.Clear();
            System.Threading.Interlocked.Add(ref _length, -len);

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
                    try { if (!_modifyLfEvent.Wait(millisecondsTimeout, cancellationToken)) return empty; }
                    catch (OperationCanceledException) { return empty; }
                    continue;
                }
                return result;
            }
        }

        //キューからの１行取り出し(\r\nを削除しない)
        public byte[] DequeueLine()
        {
            if (recvLength == _length && dequeueCounter == enqueueCounter && recvUseBlocks == _useBlocks)
            {
                return empty;
            }
            //次に何か受信するまで処理の必要はない
            SetModify(false);
            dequeueCounter = enqueueCounter;
            recvLength = _length;
            recvUseBlocks = _useBlocks;

            var size = 0;
            var maxBlocks = recvUseBlocks + _readBlocks;
            for (var i = _readBlocks; i < maxBlocks; i++)
            {
                var offset = i % MaxBlockSize;

                if (alloc.Contains(offset))
                {
                    size += _blocks[offset].DataSize;
                    continue;
                }

                var item = _blocks[offset];
                var d = item.Data;
                var s = item.DataSize;

                for (var c = 0; c < s; c++)
                {
                    if (d[c] != Lf) continue;

                    // 出力サイズとバッファ
                    var len = size + c + 1;
                    //\r\nを削除しない
                    var retBuf = new byte[len];
                    var writeSize = 0;
                    foreach (var idx in alloc)
                    {
                        var buf = _blocks[idx];
                        _blocks[idx] = null;
                        var wSize = buf.DataSize;
                        Buffer.BlockCopy(buf.Data, 0, retBuf, writeSize, wSize);
                        writeSize += wSize;
                        buf.Dispose();
                        System.Threading.Interlocked.Decrement(ref _useBlocks);
                    }

                    Buffer.BlockCopy(d, 0, retBuf, writeSize, c + 1);
                    _readBlocks = offset;

                    var blockLeftOvers = s - (c + 1);
                    if (blockLeftOvers == 0)
                    {
                        _blocks[offset] = null;
                        item.Dispose();
                        System.Threading.Interlocked.Decrement(ref _useBlocks);
                        if (_readBlocks++ >= MaxBlockSize) _readBlocks = 0;
                    }
                    else
                    {
                        Buffer.BlockCopy(d, c + 1, d, 0, blockLeftOvers);
                        item.DataSize = blockLeftOvers;
                    }

                    alloc.Clear();

                    System.Threading.Interlocked.Add(ref _length, -len);
                    var cntLf = Interlocked.Decrement(ref lfCount);
                    SetLfModify();

                    return retBuf;

                }
                size += s;
                alloc.Add(offset);
            }


            return empty;
        }


        public BufferData DequeueLineBufferWait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return BufferData.Empty;
                var result = DequeueLineBuffer();
                if (result == BufferData.Empty)
                {
                    try { if (!_modifyLfEvent.Wait(millisecondsTimeout, cancellationToken)) return BufferData.Empty; }
                    catch (OperationCanceledException) { return BufferData.Empty; }
                    continue;
                }
                return result;
            }
        }


        //キューからの１行取り出し(\r\nを削除しない)
        public BufferData DequeueLineBuffer()
        {
            if (recvLength == _length && dequeueCounter == enqueueCounter && recvUseBlocks == _useBlocks)
            {
                return BufferData.Empty;
            }
            //次に何か受信するまで処理の必要はない
            SetModify(false);
            dequeueCounter = enqueueCounter;
            recvLength = _length;
            recvUseBlocks = _useBlocks;

            var size = 0;
            var maxBlocks = recvUseBlocks + _readBlocks;
            for (var i = _readBlocks; i < maxBlocks; i++)
            {
                var offset = i % MaxBlockSize;

                if (alloc.Contains(offset))
                {
                    size += _blocks[offset].DataSize;
                    continue;
                }

                var item = _blocks[offset];
                var d = item.Data;
                var s = item.DataSize;

                for (var c = 0; c < s; c++)
                {
                    if (d[c] != Lf) continue;

                    // 出力サイズとバッファ
                    var len = size + c + 1;
                    //\r\nを削除しない
                    //var retBuf = new byte[len];
                    var retBuf = BufferPool.GetMaximum(len);

                    var writeSize = 0;
                    foreach (var idx in alloc)
                    {
                        var buf = _blocks[idx];
                        _blocks[idx] = null;
                        var wSize = buf.DataSize;
                        Buffer.BlockCopy(buf.Data, 0, retBuf.Data, writeSize, wSize);
                        writeSize += wSize;
                        buf.Dispose();
                        System.Threading.Interlocked.Decrement(ref _useBlocks);
                    }

                    Buffer.BlockCopy(d, 0, retBuf.Data, writeSize, c + 1);
                    writeSize += c + 1;
                    retBuf.DataSize = writeSize;
                    _readBlocks = offset;

                    var blockLeftOvers = s - (c + 1);
                    if (blockLeftOvers == 0)
                    {
                        _blocks[offset] = null;
                        item.Dispose();
                        System.Threading.Interlocked.Decrement(ref _useBlocks);
                        if (_readBlocks++ >= MaxBlockSize) _readBlocks = 0;
                    }
                    else
                    {
                        Buffer.BlockCopy(d, c + 1, d, 0, blockLeftOvers);
                        item.DataSize = blockLeftOvers;
                    }

                    alloc.Clear();

                    System.Threading.Interlocked.Add(ref _length, -len);
                    var cntLf = Interlocked.Decrement(ref lfCount);
                    SetLfModify();

                    return retBuf;

                }
                size += s;
                alloc.Add(offset);
            }


            return BufferData.Empty;
        }


        public Task<BufferData> DequeueLineBufferAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var cc = new CancellationTokenSource(millisecondsTimeout);
            var cancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cc.Token.Register(() => { cancel.Cancel(); });
            var t = _modifyLfEvent.WaitAsync().ContinueWith(_ActionEmpty, cancel.Token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            var resultTask = t.ContinueWith<BufferData>(TaskFuncContinue, TaskContinuationOptions.ExecuteSynchronously);
            resultTask.ContinueWith(_ => { cc.Dispose(); cancel.Dispose(); });
            return resultTask;
        }

        public Task<BufferData> DequeueLineBufferAsync(CancellationToken cancellationToken)
        {
            var t = _modifyLfEvent.WaitAsync().ContinueWith(_ActionEmpty, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return t.ContinueWith<BufferData>(TaskFuncContinue, TaskContinuationOptions.ExecuteSynchronously);

        }


        private static Action<Task> _ActionEmpty = _ => { };

        private Func<Task, BufferData> _TaskFuncContinue;
        private Func<Task, BufferData> TaskFuncContinue
        {
            get
            {
                if (_TaskFuncContinue == null)
                {
                    _TaskFuncContinue = (t) =>
                    {
                        if (t.IsCanceled) return BufferData.Empty;
                        return DequeueLineBuffer();
                    };
                }
                return _TaskFuncContinue;
            }
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

                if (_current != null) _current.Dispose();

                for (var i = 0; i < MaxBlockSize; i++)
                {
                    if (_blocks[i] == null) continue;
                    _blocks[i].Dispose();
                    _blocks[i] = null;
                }

                _modifyEvent.Dispose();
                _modifyEvent = null;
                //_db = null;
                _blocks = null;

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

