﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bjd.Net;
using Bjd.Traces;
using Bjd.Utils;
using Bjd.Threading;
using Bjd.Memory;
using System.Collections.Generic;
using System.Linq;
using Bjd.Logs;

namespace Bjd.Net.Sockets
{
    public class SockTcp : SockObj, ISocket
    {
        private Socket _socket;
        private Ssl _ssl;
        private OneSsl _oneSsl;
        internal SockQueue _sockQueueRecv;
        private Task<int> receiveTask;
        private Task receiveCompleteTask;
        private bool isSsl = false;
        private SocketAsyncEventArgs recvEventArgs;
        private SocketAsyncEventArgs sendEventArgs;
        private SimpleAsyncAwaiter recvComplete;
        private SimpleAsyncAwaiter sendComplete;
        private BufferData recvBuffer;
        private BufferData sendBuffer;


        //***************************************************************************
        //パラメータのKernelはSockObjにおけるTrace()のためだけに使用されているので、
        //Traceしない場合は削除することができる
        //***************************************************************************

        protected SockTcp(Kernel kernel) : base(kernel)
        {
            //隠蔽
        }

        //CLIENT
        public SockTcp(Kernel kernel, Ip ip, int port, int timeout, Ssl ssl) : base(kernel)
        {
            //SSL通信を使用する場合は、このオブジェクトがセットされる 通常の場合は、null
            _ssl = ssl;
            isSsl = _ssl != null;

            var addressFamily = (ip.InetKind == InetKind.V4) ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;
            _socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.SendTimeout = timeout * 1000;
            _socket.ReceiveTimeout = timeout * 1000;

            try
            {
                _socket.Connect(ip.IPAddress, port);

                if (!_socket.Connected)
                {
                    SetError("CallbackConnect() faild");
                    return;
                }

                //ここまでくると接続が完了している
                if (isSsl)
                {
                    //SSL通信の場合は、SSLのネゴシエーションが行われる
                    _oneSsl = _ssl.CreateClientStream(_socket);
                    if (_oneSsl == null)
                    {
                        SetError("_ssl.CreateClientStream() faild");
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                SetError("BeginConnect() faild " + ex.Message);
                return;
            }

            SetConnectionInfo();
            BeginAsync();

        }

        //ACCEPT
        //Ver5.9.2 Java fix
        //public SockTcp(Kernel kernel, Socket s) : base(kernel){
        public SockTcp(Kernel kernel, Ssl ssl, Socket s) : base(kernel)
        {
            // set member
            _socket = s;
            _ssl = ssl;
            isSsl = _ssl != null;

            //既に接続を完了している
            if (isSsl)
            {
                //SSL通信の場合は、SSLのネゴシエーションが行われる
                _oneSsl = _ssl.CreateServerStream(_socket);
                if (_oneSsl == null)
                {
                    SetError("_ssl.CreateServerStream() faild");
                    return;
                }
            }

            SetConnectionInfo();

        }


        public int Length()
        {
            return _sockQueueRecv.Length;
        }

        // UDP only
        public byte[] RecvBuf
        {
            get { throw new NotImplementedException(); }
        }

        private void SetConnectionInfo()
        {
            //受信バッファは接続完了後に確保される
            recvComplete = SimpleAsyncAwaiterPool.GetResetEvent(false);
            sendComplete = SimpleAsyncAwaiterPool.GetResetEvent(false);
            _sockQueueRecv = SockQueuePool.Instance.Get();
            _sockQueueRecv.UseLf();
            _sockQueueRecv.SetCancelToken(CancelToken);

            try
            {
                _socket.NoDelay = true;
                _socket.ReceiveBufferSize = 16384;
                _socket.SendBufferSize = 16384;

                //Ver5.6.0
                Set(SockState.Connect, (IPEndPoint)_socket.LocalEndPoint, (IPEndPoint)_socket.RemoteEndPoint);
            }
            catch
            {
                SetError("set IPENdPoint faild.");
                return;
            }
        }

        internal void BeginAsync()
        {
            BeginReceive();
        }

        //接続完了処理（受信待機開始）
        private void BeginReceive()
        {
            if (disposedValue || CancelToken.IsCancellationRequested)
            {
                recvComplete?.Set();
                return;
            }

            Kernel?.Logger.DebugInformation(hashText, " SockTcp.BeginReceive");

            //受信待機の開始(oneSsl!=nullの場合、受信バイト数は0に設定する)
            try
            {
                if (isSsl)
                {
                    var recvBufSeg = _sockQueueRecv.GetWriteSegment();
                    receiveTask = _oneSsl.ReadAsync(recvBufSeg, this.CancelToken);
                    receiveCompleteTask = receiveTask.ContinueWith(this.EndReceive, this.CancelToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                }
                else
                {
                    if (recvEventArgs == null)
                    {
                        //recvBuffer = BufferPool.GetMaximum(16384);
                        //receiveCompleteTask = Task.CompletedTask;
                        //recvEventArgs = new SocketAsyncEventArgs();
                        //recvEventArgs.SocketFlags = SocketFlags.None;
                        //recvEventArgs.Completed += E_Completed;
                        //recvEventArgs.SetBuffer(recvBuffer.Data, 0, recvBuffer.Length);
                        recvBuffer = _sockQueueRecv.RecvAsyncBuffer;
                        recvEventArgs = _sockQueueRecv.RecvAsyncEventArgs;
                        recvEventArgs.UserToken = this;
                        _sockQueueRecv.RecvAsyncCallback = RecvAsyncComplete;
                    }
                    var s = _socket;
                    if (s == null)
                    {
                        SetErrorReceive("SockTcp.BeginReceive");
                        return;
                    }
                    s.ReceiveAsync(recvEventArgs);
                }
            }
            catch (Exception ex)
            {
                Kernel?.Logger.TraceError($"{hashText} SockTcp.BeginReceive ExceptionMessage:{ex.Message}");
                Kernel?.Logger.TraceError($"{hashText} SockTcp.BeginReceive StackTrace:{ex.StackTrace}");
                SetErrorReceive("SockTcp.BeginReceive");
            }
        }

        private static EventHandler<SocketAsyncEventArgs> RecvAsyncComplete =
            (s, e) =>
        {
            var ins = (SockTcp)e.UserToken;
            if (ins == null) return;

            if (ins.disposedValue || ins.CancelToken.IsCancellationRequested)
            {
                ins.recvComplete?.Set();
                return;
            }

            if (e.SocketError == SocketError.Success)
            {
                var recvBuf = ins.recvBuffer;
                if (recvBuf != null) recvBuf.DataSize = e.BytesTransferred;
                ins.EndReceive(e.BytesTransferred);
                return;
            }

            ins.SetErrorReceive(e.SocketError.ToString());
            ins.Kernel.Logger.TraceError($"{ins.hashText} SockTcp.RecvAsyncComplete {e.SocketError}");

        };


        private void E_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (disposedValue || CancelToken.IsCancellationRequested)
            {
                recvComplete?.Set();
                return;
            }

            if (e.SocketError == SocketError.Success)
            {
                var recvBuf = recvBuffer;
                if (recvBuf != null) recvBuf.DataSize = e.BytesTransferred;
                EndReceive(e.BytesTransferred);
                return;
            }

            SetErrorReceive(e.SocketError.ToString());
            Kernel.Logger.TraceError($"{hashText} SockTcp.E_Completed {e.SocketError}");

        }

        private void EndReceive(Task<int> result)
        {
            if (result.IsCanceled)
            {
                Kernel.Logger.TraceWarning($"{hashText} SockTcp.EndReceive IsCanceled=true");
                recvComplete.Set();
                return;
            }

            if (result.IsFaulted)
            {
                var ex = result.Exception.InnerException as SocketException;
                if (ex != null)
                {
                    Kernel.Logger.TraceError($"{hashText} SockTcp.EndReceive Result.SocketErrorCode:{ex.SocketErrorCode}");
                }

                Kernel.Logger.TraceError($"{hashText} SockTcp.EndReceive Result.ExceptionType:{result.Exception.InnerException.GetType().FullName}");
                Kernel.Logger.TraceError($"{hashText} SockTcp.EndReceive Result.ExceptionMessage:{result.Exception.InnerException.Message}");
                Kernel.Logger.TraceError($"{hashText} SockTcp.EndReceive Result.ExceptionMessage:{result.Exception.Message}");
                Kernel.Logger.TraceError($"{hashText} SockTcp.EndReceive Result.StackTrace:{result.Exception.StackTrace}");
                SetErrorReceive("SockTcp.EndReceive(Task<int> result)");
                return;
            }

            EndReceive(result.Result);
        }

        private void EndReceive(int result)
        {


            //受信完了
            //ポインタを移動する場合は、排他制御が必要
            try
            {
                //Ver5.9.2 Java fix
                int bytesRead = result;
                Kernel.Logger.DebugInformation(hashText, " SockTcp.EndReceive Length=", bytesRead.ToString());
                if (bytesRead <= 0)
                {
                    //  切断されている場合は、0が返される?
                    SetErrorReceive("SockTcp.EndReceive(int result) receive zero");
                    _sockQueueRecv.StopEnqueue();
                    return;
                }
                if (recvBuffer != null)
                {
                    _sockQueueRecv.EnqueueImport(recvBuffer);
                }
                else
                {
                    _sockQueueRecv.NotifyWrite(bytesRead);
                }

            }
            catch (Exception ex)
            {
                //受信待機のままソケットがクローズされた場合は、ここにくる
                Kernel?.Logger.TraceError($"{hashText} SockTcp.EndReceive ExceptionMessage:{ex.Message}");
                SetErrorReceive("SockTcp.EndReceive(int result)" + ex.Message);
                return;
            }

            //バッファがいっぱい 空の受信待機をかける
            //受信待機
            while ((_sockQueueRecv?.Space ?? 0) == 0)
            {
                Thread.Sleep(1); //他のスレッドに制御を譲る  
                if (disposedValue) return;
                if (CancelToken.IsCancellationRequested) return;
                if (SockState != SockState.Connect)
                {
                    Kernel?.Logger.TraceWarning($"{hashText} SockTcp.EndReceive Not Connected");
                    SetErrorReceive("SockTcp.EndReceive(int result) waiting for free space");
                    return;
                }
            }

            //受信待機の開始
            BeginReceive();

        }

        private void SetErrorReceive(string message)
        {
            recvComplete?.Set();
            SetError($"{hashText} SockTcp.SetErrorReceive " + message);
        }


        //受信<br>
        //切断・タイムアウトでnullが返される
        public byte[] Recv(int len, int sec, ILife iLife)
        {
            var toutms = sec * 1000;
            var t = _sockQueueRecv.DequeueAsync(len, toutms, CancelToken).AsTask();
            t.Wait();
            var result = t.Result;
            if (result.Length == 0 && SockState != SockState.Connect)
            {
                return null;
            }
            var length = (result != null ? result.Length.ToString() : "null");
            Kernel.Logger.DebugInformation(hashText, " SockTcp.Recv ", length);
            return result;
        }

        //受信<br>
        //切断・タイムアウトでnullが返される
        public async ValueTask<BufferData> BufferRecvAsync(int len, int sec)
        {
            var toutms = sec * 1000;
            var result = await _sockQueueRecv.DequeueBufferAsync(len, toutms, CancelToken);
            if (result.DataSize == 0 && SockState != SockState.Connect)
            {
                return null;
            }
            var length = (result != null ? result.DataSize.ToString() : "null");
            Kernel.Logger.DebugInformation(hashText, " SockTcp.BufferRecvAsync ", length);
            return result;
        }


        //1行受信
        //切断・タイムアウトでnullが返される
        public byte[] LineRecv(int sec, ILife iLife)
        {
            var toutms = sec * 1000;
            var t = _sockQueueRecv.DequeueLineAsync(toutms, CancelToken).AsTask();
            t.Wait();
            var result = t.Result;
            if (result.Length == 0) return null;
            var length = (result != null ? result.Length.ToString() : "null");
            Kernel.Logger.DebugInformation(hashText, " SockTcp.LineRecv ", length);
            return result;
        }

        public BufferData LineBufferRecv(int sec, ILife iLife)
        {
            var toutms = sec * 1000;
            var resultTask = _sockQueueRecv.DequeueLineBufferAsync(toutms, CancelToken).AsTask();
            resultTask.Wait();
            var result = resultTask.Result;
            if (result.DataSize == 0) return null;
            var length = (result != null ? result.DataSize.ToString() : "null");
            Kernel.Logger.DebugInformation(hashText, " SockTcp.LineBufferRecv ", length);
            return result;
        }

        public async ValueTask<BufferData> LineBufferRecvAsync(int timeoutSec)
        {
            var toutms = timeoutSec * 1000;
            Kernel.Logger.DebugInformation(hashText, " SockTcp.LineBufferRecvAsync ");
            //var result = _sockQueueRecv.DequeueLineBufferAsync(toutms);
            //return result;
            return await _sockQueueRecv.DequeueLineBufferAsync(toutms, CancelToken);
        }





        private async ValueTask<bool> SendAsyncInternal(BufferData buf)
        {
            if (sendEventArgs == null)
            {
                //sendBuffer = BufferPool.GetMaximum(16384);
                //sendEventArgs = new SocketAsyncEventArgs();
                //sendEventArgs.SetBuffer(sendBuffer.Data, 0, sendBuffer.DataSize);
                //sendEventArgs.SocketFlags = SocketFlags.None;
                //sendEventArgs.UserToken = this;
                //sendEventArgs.Completed += SendAsyncComplete;
                sendBuffer = _sockQueueRecv.SendAsyncBuffer;
                sendEventArgs = _sockQueueRecv.SendAsyncEventArgs;
                sendEventArgs.UserToken = this;
                _sockQueueRecv.SendAsyncCallback = SendAsyncComplete;
            }
            var len = buf.DataSize;
            var offset = 0;
            while (len > 0)
            {
                sendComplete.Reset();
                var size = (len > sendBuffer.Length ? sendBuffer.Length : len);

                buf.CopyTo(sendBuffer, offset, size);
                sendEventArgs.SetBuffer(0, size);
                var result = _socket.SendAsync(sendEventArgs);
                if (result) await sendComplete.WaitAsyncValueTask();
                len -= size;
                offset += size;
            }
            return true;
        }

        private static EventHandler<SocketAsyncEventArgs> SendAsyncComplete =
            (s, e) =>
            {
                var ins = (SockTcp)e.UserToken;
                ins.sendComplete.Set();
            };

        public async ValueTask<bool> SendAsync(BufferData buf)
        {
            IfThrowOnDisposed();
            if (disposedValue || SockState != SockState.Connect || CancelToken.IsCancellationRequested)
            {
                return true;
            }

            if (isSsl)
            {
                await _oneSsl.WriteAsync(buf.Data, buf.DataSize, this.CancelToken);
            }
            else
            {
                await SendAsyncInternal(buf);
            }

            return true;

        }

        public int Send(byte[] buf, int offset, int length)
        {
            IfThrowOnDisposed();
            var lengthtxt = length.ToString();
            Kernel.Logger.DebugInformation(hashText, " SockTcp.Send ", lengthtxt);
            try
            {
                //Ver5.9.2 Java fix
                if (isSsl)
                {
                    return _oneSsl.Write(buf, offset, buf.Length);
                }
                else
                {
                    return _socket.Send(buf, offset, length, SocketFlags.None);
                }
            }
            catch (Exception e)
            {
                SetException(e);
                return -1;
            }
        }

        public int Send(IList<ArraySegment<byte>> buffers)
        {
            IfThrowOnDisposed();
#if DEBUG
            var length = buffers.Sum(_ => _.Count);
            Kernel.Logger.DebugInformation(hashText, " SockTcp.Send IList<ArraySegment<byte>> ", length.ToString());
#endif
            try
            {
                //Ver5.9.2 Java fix
                if (isSsl)
                {
                    return _oneSsl.Write(buffers);
                }
                else
                {
                    return _socket.Send(buffers, SocketFlags.None);
                }
            }
            catch (Exception e)
            {
                SetException(e);
                return -1;
            }
        }

        public int Send(BufferData buf)
        {
            IfThrowOnDisposed();
            Kernel.Logger.DebugInformation(hashText, " SockTcp.Send ", buf.DataSize.ToString());
            try
            {
                //Ver5.9.2 Java fix
                if (isSsl)
                {
                    return _oneSsl.Write(buf.Data, buf.DataSize);
                }
                else
                {
                    return _socket.Send(buf.Data, buf.DataSize, SocketFlags.None);
                }
            }
            catch (Exception e)
            {
                SetException(e);
                return -1;
            }
        }

        //【送信】(トレースなし)
        //リモートサーバがトレース内容を送信するときに更にトレースするとオーバーフローするため
        //RemoteObj.Send()では、こちらを使用する
        public int SendNoTrace(byte[] buffer)
        {
            IfThrowOnDisposed();
            Kernel.Logger.DebugInformation(hashText, " SockTcp.SendNoTrace ", buffer.Length.ToString());
            try
            {
                if (isSsl)
                {
                    return _oneSsl.Write(buffer, buffer.Length);
                }

                if (_socket.Connected)
                {
                    return _socket.Send(buffer, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                SetError($"{hashText} SendNoTrace Length={buffer.Length} {ex.Message}");
                //Logger.Set(LogKind.Error, this, 9000046, string.Format("Length={0} {1}", buffer.Length, ex.Message));
            }
            return -1;
        }
        public int SendNoTrace(ArraySegment<byte> buffer)
        {
            try
            {
                if (isSsl)
                {
                    return _oneSsl.Write(buffer);
                }

                if (_socket.Connected)
                {
                    return _socket.Send(buffer.Array, buffer.Offset, buffer.Count, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                SetError($"{hashText} SendNoTrace Length={buffer.Count} {ex.Message}");
                //Logger.Set(LogKind.Error, this, 9000046, string.Format("Length={0} {1}", buffer.Count, ex.Message));
            }
            return -1;
        }


        /*******************************************************************/
        //以下、C#のコードを通すために設置（最終的に削除の予定）
        /*******************************************************************/
        private string _lastLineSend = "";

        public string LastLineSend
        {
            get
            {
                return _lastLineSend;
            }
        }

        //内部でASCIIコードとしてエンコードする１行送信  (\r\nが付加される)
        //LineSend()のオーバーライドバージョン
        //public int AsciiSend(string str, OperateCrlf operateCrlf) {
        public int AsciiSend(string str)
        {
            _lastLineSend = str;
            var buf = Encoding.ASCII.GetBytes(str);
            //return LineSend(buf, operateCrlf);
            //とりあえずCrLfの設定を無視している
            var d = new[] { new ArraySegment<byte>(buf), new ArraySegment<byte>(CrLf) };
            return Send(d);
        }

        public override void Close()
        {
            if (this.disposedValue) return;
            this.Dispose();
        }

        private static bool RequireWait(SocketError error)
        {
            if (error == SocketError.Shutdown) return false;
            //if (error == SocketError.ConnectionReset) return false;
            //if (error == SocketError.ConnectionAborted) return false;
            return true;
        }

        private void IfThrowOnDisposed()
        {
            if (disposedValue) throw new ObjectDisposedException("SockTcp");
        }

        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;

                try { this.Cancel(); }
                catch (Exception ex)
                { Kernel?.Logger.TraceError($"{hashText} Dispose Error Cancel {ex.Message} {ex.StackTrace} "); }

                if (receiveTask != null)
                {
                    receiveTask = null;
                }
                if (receiveCompleteTask != null)
                {
                    try { receiveCompleteTask.Wait(); }
                    catch { }
                    receiveCompleteTask = null;
                }


                //TCPのソケットをシャットダウンするとエラーになる（無視する）
                try
                {
                    if (_socket.Connected) _socket.Shutdown(SocketShutdown.Both);
                    //_socket.Shutdown(SocketShutdown.Receive);
                }
                catch (Exception ex)
                { Kernel?.Logger.TraceError($"{hashText} Dispose Error Shutdown {ex.Message} {ex.StackTrace} "); }

                try
                {
                    if (sendEventArgs != null)
                    {
                        //sendEventArgs.Completed -= SendAsyncComplete;
                        //sendEventArgs.Dispose();
                        sendEventArgs = null;
                    }
                    if (sendBuffer != null)
                    {
                        //sendBuffer.Dispose();
                        sendBuffer = null;
                    }
                    sendComplete?.Dispose();
                    sendComplete = null;
                }
                catch (Exception ex)
                { Kernel?.Logger.TraceError($"{hashText} Dispose Error sendComplete {ex.Message} {ex.StackTrace} "); }

                try
                {
                    if (recvEventArgs != null)
                    {
                        //recvEventArgs.Completed -= E_Completed;
                        //recvEventArgs.Dispose();
                        recvEventArgs = null;
                    }
                    if (recvBuffer != null)
                    {
                        //recvBuffer.Dispose();
                        recvBuffer = null;
                    }
                    recvComplete?.Dispose();
                    recvComplete = null;
                }
                catch (Exception ex)
                { Kernel?.Logger.TraceError($"{hashText} Dispose Error recvComplete {ex.Message} {ex.StackTrace} "); }

                _lastLineSend = null;
                if (_sockQueueRecv != null)
                {
                    //SockQueuePool.Instance.Pool(ref this._sockQueueRecv);
                    _sockQueueRecv.Dispose();
                    _sockQueueRecv = null;
                }

                if (_socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }
                if (_oneSsl != null)
                {
                    _oneSsl.Close();
                    _oneSsl = null;
                }

                if (_ssl != null)
                {
                    _ssl.Dispose();
                    _ssl = null;
                }

            }

            base.Dispose(disposing);
        }

    }
}
