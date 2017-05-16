using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using Bjd.Memory;
using Bjd.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Bjd.Net.Sockets
{
    //SockTcpで使用されるデータキュー
    public class SockQueue : IPoolBuffer
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
        bool useLfCount = false;

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
        SimpleResetEvent _modifySizeEvent = SimpleResetPool.GetResetEvent(false);
        SimpleResetEvent _modifyLfEvent = SimpleResetPool.GetResetEvent(false);

        public SocketAsyncEventArgs RecvAsyncEventArgs;
        public BufferData RecvAsyncBuffer;
        public EventHandler<SocketAsyncEventArgs> RecvAsyncCallback;

        public SocketAsyncEventArgs SendAsyncEventArgs;
        public BufferData SendAsyncBuffer;
        public EventHandler<SocketAsyncEventArgs> SendAsyncCallback;

        public int Max { get { return max; } }

        public int UseBlocks { get { return _useBlocks; } }

        //空いているスペース
        public int Space { get { return max - _length; } }

        //現在のキューに溜まっているデータ量
        public int Length { get { return _length; } }

        public bool IsEmpty { get { return _modifySizeEvent.IsLocked; } }


        SockQueuePool _pool;

        internal SockQueue() : this(null)
        {
        }

        internal SockQueue(SockQueuePool pool)
        {
            _pool = pool;

            RecvAsyncBuffer = BufferPool.GetMaximum(16384);
            RecvAsyncEventArgs = new SocketAsyncEventArgs();
            RecvAsyncEventArgs.SocketFlags = SocketFlags.None;
            RecvAsyncEventArgs.Completed += RecvAsyncCompleted;
            RecvAsyncEventArgs.SetBuffer(RecvAsyncBuffer.Data, 0, RecvAsyncBuffer.Length);

            SendAsyncBuffer = BufferPool.GetMaximum(16384);
            SendAsyncEventArgs = new SocketAsyncEventArgs();
            SendAsyncEventArgs.SocketFlags = SocketFlags.None;
            SendAsyncEventArgs.Completed += SendAsyncCompleted;
            SendAsyncEventArgs.SetBuffer(SendAsyncBuffer.Data, 0, SendAsyncBuffer.Length);

        }

        private void RecvAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            RecvAsyncCallback?.Invoke(sender, e);
        }

        private void SendAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            SendAsyncCallback?.Invoke(sender, e);
        }
        private void Clear()
        {
            RecvAsyncCallback = null;
            SendAsyncCallback = null;
            RecvAsyncEventArgs.UserToken = null;
            SendAsyncEventArgs.UserToken = null;
        }

        internal void Initialize()
        {
            SetModify(false);
            _modifySizeEvent.Reset();
            _modifyLfEvent.Reset();
            useLfCount = false;
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

        private void AddLfCount(int count)
        {
            var cnt = Interlocked.Add(ref lfCount, count);
            if (cnt > 0)
            {
                _modifyLfEvent.Set();
            }
            else
            {
                _modifyLfEvent.Reset();
            }
        }

        private void AddLength(int len)
        {
            var length = Interlocked.Add(ref _length, len);
            if (length > 0)
            {
                _modifySizeEvent.Set();
            }
            else
            {
                _modifySizeEvent.Reset();
            }

        }


        public void UseLf()
        {
            useLfCount = true;
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
            System.Threading.Interlocked.Increment(ref _useBlocks);
            //System.Threading.Interlocked.Add(ref _length, len);

            _totallength += len;

            if (enqueueCounter == int.MaxValue) enqueueCounter = 0;
            enqueueCounter++;

            beforeSize = len;

            //データベースの内容が変化した
            AddLength(len);
            SetModify(true);
            if (useLfCount)
            {
                var cntLf = buf.CountLf();
                //Interlocked.Add(ref lfCount, cntLf);
                AddLfCount(cntLf);
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
                if (useLfCount) cntLf += b.CountLf();

                //while (!_db.TryAdd(b)) { }
                _blocks[_nextBlocks++] = b;
                if (_nextBlocks >= MaxBlockSize)
                {
                    _nextBlocks = 0;
                }
                System.Threading.Interlocked.Increment(ref _useBlocks);

                leftOversSize -= size;
            }


            //System.Threading.Interlocked.Add(ref _length, len);
            _totallength += len;

            if (enqueueCounter == int.MaxValue) enqueueCounter = 0;
            enqueueCounter++;

            //_modify = true; //データベースの内容が変化した
            AddLength(len);
            SetModify(true);
            if (useLfCount)
            {
                //Interlocked.Add(ref lfCount, cntLf);
                AddLfCount(cntLf);
            }
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
            //System.Threading.Interlocked.Add(ref _length, len);
            _totallength += len;

            if (enqueueCounter == int.MaxValue) enqueueCounter = 0;
            enqueueCounter++;

            //_modify = true; //データベースの内容が変化した
            AddLength(len);
            SetModify(true);
            if (useLfCount)
            {
                var cntLf = buf.CountLf();
                //Interlocked.Add(ref lfCount, cntLf);
                AddLfCount(cntLf);
            }

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
            //newBuf.DataSize = len;
            //Buffer.BlockCopy(buf.Data, 0, newBuf.Data, 0, len);
            buf.CopyTo(newBuf);

            _blocks[_nextBlocks] = newBuf;

            _nextBlocks++;
            if (_nextBlocks >= MaxBlockSize)
            {
                _nextBlocks = 0;
            }

            System.Threading.Interlocked.Increment(ref _useBlocks);
            //System.Threading.Interlocked.Add(ref _length, len);
            _totallength += len;

            if (enqueueCounter == int.MaxValue) enqueueCounter = 0;
            enqueueCounter++;

            //_modify = true; //データベースの内容が変化した
            AddLength(len);
            SetModify(true);
            if (useLfCount)
            {
                var cntLf = buf.CountLf();
                //Interlocked.Add(ref lfCount, cntLf);
                AddLfCount(cntLf);
            }

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
            //System.Threading.Interlocked.Add(ref _length, -len);
            AddLength(-len);

            return retBuf;

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
            //System.Threading.Interlocked.Add(ref _length, -len);
            AddLength(-len);

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
                    try
                    {
                        if (useLfCount)
                        {
                            if (!_modifyLfEvent.Wait(millisecondsTimeout, cancellationToken)) return empty;
                        }
                        else
                        {
                            if (!_modifyEvent.Wait(millisecondsTimeout, cancellationToken)) return empty;
                        }
                    }
                    catch (OperationCanceledException) { return empty; }
                    continue;
                }
                return result;
            }
        }

        //キューからの１行取り出し(\r\nを削除しない)
        public byte[] DequeueLine()
        {
            if (!useLfCount)
            {
                if (recvLength == _length && dequeueCounter == enqueueCounter && recvUseBlocks == _useBlocks)
                {
                    return empty;
                }
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

                    //System.Threading.Interlocked.Add(ref _length, -len);
                    AddLength(-len);
                    if (useLfCount)
                    {
                        //var cntLf = Interlocked.Decrement(ref lfCount);
                        AddLfCount(-1);
                    }

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
                    try
                    {
                        if (useLfCount)
                        {
                            if (!_modifyLfEvent.Wait(millisecondsTimeout, cancellationToken)) return BufferData.Empty;
                        }
                        else
                        {
                            if (!_modifyEvent.Wait(millisecondsTimeout, cancellationToken)) return BufferData.Empty;
                        }
                    }
                    catch (OperationCanceledException) { return BufferData.Empty; }
                    continue;
                }
                return result;
            }
        }


        //キューからの１行取り出し(\r\nを削除しない)
        public BufferData DequeueLineBuffer()
        {
            if (!useLfCount)
            {
                if (recvLength == _length && dequeueCounter == enqueueCounter && recvUseBlocks == _useBlocks)
                {
                    return BufferData.Empty;
                }
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

                    //System.Threading.Interlocked.Add(ref _length, -len);
                    AddLength(-len);
                    if (useLfCount)
                    {
                        //var cntLf = Interlocked.Decrement(ref lfCount);
                        AddLfCount(-1);
                    }

                    return retBuf;

                }
                size += s;
                alloc.Add(offset);
            }


            return BufferData.Empty;
        }


        public async ValueTask<BufferData> DequeueBufferAsync(int len)
        {
            var result = await _modifySizeEvent.WaitAsyncValueTask();
            if (!result) return BufferData.Empty;

            return DequeueBuffer(len);
        }

        public async ValueTask<BufferData> DequeueBufferAsync(int len, CancellationToken cancellationToken)
        {
            var result = await _modifySizeEvent.WaitAsyncValueTask(cancellationToken);
            if (!result) return BufferData.Empty;

            return DequeueBuffer(len);
        }

        public async ValueTask<BufferData> DequeueBufferAsync(int len, int millisecondsTimeout)
        {
            var result = await _modifySizeEvent.WaitAsyncValueTask(millisecondsTimeout);
            if (!result) return BufferData.Empty;

            return DequeueBuffer(len);
        }

        public async ValueTask<BufferData> DequeueBufferAsync(int len, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var result = await _modifySizeEvent.WaitAsyncValueTask(millisecondsTimeout, cancellationToken);
            if (!result) return BufferData.Empty;

            return DequeueBuffer(len);
        }


        public async ValueTask<BufferData> DequeueLineBufferAsync(int millisecondsTimeout)
        {
            if (!useLfCount) throw new InvalidOperationException("require invoke useLf() before call");

            var result = await _modifyLfEvent.WaitAsyncValueTask(millisecondsTimeout);
            if (!result) return BufferData.Empty;

            return DequeueLineBuffer();

        }

        public async ValueTask<BufferData> DequeueLineBufferAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (!useLfCount) throw new InvalidOperationException("require invoke useLf() before call");

            var result = await _modifyLfEvent.WaitAsyncValueTask(millisecondsTimeout, cancellationToken);
            if (!result) return BufferData.Empty;

            return DequeueLineBuffer();
        }

        public async ValueTask<BufferData> DequeueLineBufferAsync(CancellationToken cancellationToken)
        {
            if (!useLfCount) throw new InvalidOperationException("require invoke useLf() before call");

            var result = await _modifyLfEvent.WaitAsyncValueTask(cancellationToken);
            if (!result) return BufferData.Empty;

            return DequeueLineBuffer();

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

                RecvAsyncBuffer?.Dispose();
                RecvAsyncBuffer = null;
                RecvAsyncEventArgs?.Dispose();
                RecvAsyncEventArgs = null;

                SendAsyncBuffer?.Dispose();
                SendAsyncBuffer = null;
                SendAsyncEventArgs?.Dispose();
                SendAsyncEventArgs = null;

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }


        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            Clear();
            if (_pool != null)
            {
                _pool.PoolInternal(this);
                return;
            }
            Dispose(true);
        }

        void IPoolBuffer.Initialize()
        {
            this.Initialize();
        }

        void IPoolBuffer.DisposeInternal()
        {
            Clear();
            Dispose(true);
        }

        #endregion
    }
}

