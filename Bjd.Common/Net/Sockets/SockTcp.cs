using System;
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
    public class SockTcp : SockObj
    {

        private static readonly byte[] CrLf = new byte[] { 0x0D, 0x0A };

        private Socket _socket;
        private Ssl _ssl;
        private OneSsl _oneSsl;
        internal SockQueue _sockQueueRecv;
        internal SockQueue _sockQueueSend;
        private Task<int> receiveTask;
        private Task receiveCompleteTask;
        private Task sendTask;
        private Task sendCompleteTask;
        private bool isSsl = false;
        private int hash;
        private string hashText;
        private SocketAsyncEventArgs recvEventArgs;
        private SocketAsyncEventArgs sendEventArgs;
        //private ManualResetEventSlim sendComplete = new ManualResetEventSlim(true, 0);
        private SimpleResetEvent recvComplete = SimpleResetPool.GetResetEvent(false);
        private SimpleResetEvent sendComplete = SimpleResetPool.GetResetEvent(true);
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
            hash = this.GetHashCode();
            hashText = hash.ToString();

            _socket = new Socket((ip.InetKind == InetKind.V4) ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
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
            catch
            {
                SetError("BeginConnect() faild");
            }

            SetConnectionInfo();
            BeginReceive(); //接続完了処理（受信待機開始）

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
            hash = this.GetHashCode();
            hashText = hash.ToString();

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

        private void SetConnectionInfo()
        {
            //受信バッファは接続完了後に確保される
            _sockQueueRecv = SockQueuePool.Instance.Get();
            _sockQueueSend = SockQueuePool.Instance.Get();

            try
            {
                _socket.NoDelay = true;

                //Ver5.6.0
                Set(SockState.Connect, (IPEndPoint)_socket.LocalEndPoint, (IPEndPoint)_socket.RemoteEndPoint);
            }
            catch
            {
                SetError("set IPENdPoint faild.");
                return;
            }
        }

        //接続完了処理（受信待機開始）
        internal void BeginReceive()
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
                        recvBuffer = BufferPool.GetMaximum(65536);
                        receiveCompleteTask = Task.CompletedTask;
                        recvEventArgs = new SocketAsyncEventArgs();
                        recvEventArgs.SocketFlags = SocketFlags.None;
                        recvEventArgs.Completed += E_Completed;
                        recvEventArgs.SetBuffer(recvBuffer.Data, 0, recvBuffer.Length);
                    }
                    var s = _socket;
                    if (s == null)
                    {
                        SetErrorReceive();
                        return;
                    }
                    s.ReceiveAsync(recvEventArgs);
                }
            }
            catch (Exception ex)
            {
                Kernel?.Logger.TraceError($"{hashText} SockTcp.BeginReceive ExceptionMessage:{ex.Message}");
                Kernel?.Logger.TraceError($"{hashText} SockTcp.BeginReceive StackTrace:{ex.StackTrace}");
                SetErrorReceive();
            }
        }

        private void E_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (disposedValue || CancelToken.IsCancellationRequested)
            {
                recvComplete?.Set();
                return;
            }

            if (e.SocketError == SocketError.Success)
            {
                recvBuffer.DataSize = e.BytesTransferred;
                EndReceive(e.BytesTransferred);
                return;
            }

            SetErrorReceive();
            Kernel.Logger.TraceError($"{hashText} SockTcp.E_Completed {e.SocketError}");

        }

        public void EndReceive(Task<int> result)
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
                SetErrorReceive();
                return;
            }

            EndReceive(result.Result);
        }

        public void EndReceive(int result)
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
                    SetErrorReceive();
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
                SetErrorReceive();
                return;
            }

            //バッファがいっぱい 空の受信待機をかける
            //受信待機
            while (_sockQueueRecv == null || _sockQueueRecv.Space == 0)
            {
                Thread.Sleep(5); //他のスレッドに制御を譲る  
                if (disposedValue) return;
                if (CancelToken.IsCancellationRequested) return;
                if (SockState != SockState.Connect)
                {
                    Kernel.Logger.TraceWarning($"{hashText} SockTcp.EndReceive Not Connected");
                    SetErrorReceive();
                    return;
                }
            }

            //受信待機の開始
            BeginReceive();

        }

        private void SetErrorReceive()
        {
            recvComplete?.Set();
            sendComplete?.Set();
            SetError($"{hashText} SockTcp.disconnect");
        }
        private void SetErrorSend()
        {
            recvComplete?.Set();
            sendComplete?.Set();
            SetError($"{hashText} SockTcp.disconnect");
        }



        //受信<br>
        //切断・タイムアウトでnullが返される
        public byte[] Recv(int len, int sec, ILife iLife)
        {
            var toutms = sec * 1000;
            var result = _sockQueueRecv.DequeueWait(len, toutms, this.CancelToken);
            if (result.Length == 0 && SockState != SockState.Connect) return null;
            var length = (result != null ? result.Length.ToString() : "null");
            Kernel.Logger.DebugInformation(hashText, " SockTcp.Recv ", length);
            return result;
        }

        //1行受信
        //切断・タイムアウトでnullが返される
        public byte[] LineRecv(int sec, ILife iLife)
        {
            var toutms = sec * 1000;
            var result = _sockQueueRecv.DequeueLineWait(toutms, this.CancelToken);
            if (result.Length == 0) return null;
            var length = (result != null ? result.Length.ToString() : "null");
            Kernel.Logger.DebugInformation(hashText, " SockTcp.LineRecv ", length);
            return result;
        }

        public BufferData LineBufferRecv(int sec, ILife iLife)
        {
            var toutms = sec * 1000;
            var result = _sockQueueRecv.DequeueLineBufferWait(toutms, this.CancelToken);
            if (result.DataSize == 0) return null;
            var length = (result != null ? result.DataSize.ToString() : "null");
            Kernel.Logger.DebugInformation(hashText, " SockTcp.LineBufferRecv ", length);
            return result;
        }

        public Task<BufferData> LineBufferRecvAsync()
        {
            Kernel.Logger.DebugInformation(hashText, " SockTcp.LineBufferRecvAsync ");
            var result = _sockQueueRecv.DequeueLineBufferAsync(this.CancelToken);
            return result;
        }

        //１行のString受信
        public string StringRecv(Encoding enc, int sec, ILife iLife)
        {
            try
            {
                using (var buffer = LineBufferRecv(sec, iLife))
                {
                    if (buffer == null)
                    {
                        return null;
                    }
                    return enc.GetString(buffer.Data, 0, buffer.DataSize);
                }
            }
            catch (Exception e)
            {
                Util.RuntimeException(e.Message);
            }
            return null;
        }

        //１行のString受信(ASCII)
        public string StringRecv(int sec, ILife iLife)
        {
            return StringRecv(Encoding.ASCII, sec, iLife);
        }
        //１行のString受信
        public string StringRecv(string charsetName, int sec, ILife iLife)
        {
            try
            {
                var enc = CodePagesEncodingProvider.Instance.GetEncoding(charsetName);
                if (enc == null)
                    enc = Encoding.GetEncoding(charsetName);
                return StringRecv(enc, sec, iLife);
            }
            catch (Exception e)
            {
                Util.RuntimeException(e.Message);
            }
            return null;
        }

        // 【１行受信】
        //切断されている場合、nullが返される
        //public string AsciiRecv(int timeout, OperateCrlf operateCrlf, ILife iLife) {
        public string AsciiRecv(int timeout, ILife iLife)
        {
            using (var buf = LineBufferRecv(timeout, iLife))
            {
                return buf == null ? null : Encoding.ASCII.GetString(buf.Data, 0, buf.DataSize);
            }
        }

        // 【１行受信】
        //切断されている場合、nullが返される
        public CharsData AsciiRecvChars(int timeout, ILife iLife)
        {
            using (var buf = LineBufferRecv(timeout, iLife))
            {
                return buf == null ? null : buf.ToAsciiCharsData();
            }
        }

        public Task<CharsData> AsciiRecvCharsAsync()
        {
            Kernel.Logger.DebugInformation(hashText, " SockTcp.AsciiRecvCharsAsync ");
            var result = _sockQueueRecv.DequeueLineBufferAsync(CancelToken);
            return result.ContinueWith<CharsData>(AsciiFunc, CancelToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private static Func<Task<BufferData>, CharsData> _AsciiFunc;
        private static Func<Task<BufferData>, CharsData> AsciiFunc
        {
            get
            {
                if (_AsciiFunc != null)
                {
                    _AsciiFunc = _ =>
                    {
                        try
                        {
                            return _.Result.ToAsciiCharsData();
                        }
                        finally
                        {
                            _.Result.Dispose();
                        }
                    };
                }
                return _AsciiFunc;
            }
        }


        private void BeginSend(BufferData buf)
        {
            currentSend = buf;

            //受信待機の開始(oneSsl!=nullの場合、受信バイト数は0に設定する)
            try
            {
                if (disposedValue || this.CancelToken.IsCancellationRequested)
                {
                    sendComplete?.Set();
                    return;
                }

                Kernel?.Logger.DebugInformation(hashText, " SockTcp.BeginSend");

                if (isSsl)
                {
                    sendTask = _oneSsl.WriteAsync(buf.Data, buf.DataSize, this.CancelToken);
                    sendCompleteTask = sendTask.ContinueWith(WriteAsync_Completed, CancelToken);
                }
                else
                {
                    if (sendEventArgs == null)
                    {
                        sendBuffer = BufferPool.GetMaximum(65536);
                        sendCompleteTask = Task.CompletedTask;
                        sendEventArgs = new SocketAsyncEventArgs();
                        sendEventArgs.Completed += SendEventArgs_Completed;
                        sendEventArgs.SocketFlags = SocketFlags.None;
                        sendEventArgs.SetBuffer(sendBuffer.Data, 0, 0);
                    }
                    var len = buf.DataSize;
                    Buffer.BlockCopy(buf.Data, 0, sendBuffer.Data, 0, len);
                    sendEventArgs.SetBuffer(0, len);
                    var s = _socket;
                    if (s == null)
                    {
                        SetErrorSend();
                        return;
                    }
                    s.SendAsync(sendEventArgs);
                }
            }
            catch (Exception ex)
            {
                Kernel?.Logger.TraceError($"{hashText} SockTcp.BeginSend ExceptionMessage:{ex.Message}");
                Kernel?.Logger.TraceError($"{hashText} SockTcp.BeginSend StackTrace:{ex.StackTrace}");
                SetErrorSend();
            }

        }

        private BufferData currentSend;
        private int sendCounter = 0;

        private void WriteAsync_Completed(Task before)
        {
            try
            {
                if (disposedValue || SockState != SockState.Connect || CancelToken.IsCancellationRequested)
                {
                    sendComplete?.Set();
                    return;
                }

                var len = 0;
                if (currentSend != null)
                {
                    len += currentSend.DataSize;
                    currentSend.Dispose();
                    currentSend = null;
                    if (Interlocked.Add(ref sendCounter, -len) == 0)
                    {
                        sendComplete.Set();
                        return;
                    }
                }

                //var b = _sockQueueSend.DequeueBufferWait(65536, 100, CancelToken);
                var b = _sockQueueSend?.DequeueBuffer(65536);
                if (b == null || b == BufferData.Empty)
                {
                    sendComplete?.Set();
                    return;
                }
                BeginSend(b);

            }
            catch (Exception ex)
            {
                Kernel?.Logger.TraceError($"{hashText} SockTcp.WriteAsync_Completed ExceptionMessage:{ex.Message}");
                Kernel?.Logger.TraceError($"{hashText} SockTcp.WriteAsync_Completed StackTrace:{ex.StackTrace}");
                SetErrorSend();
            }
        }


        private void SendEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {

                if (disposedValue || SockState != SockState.Connect || CancelToken.IsCancellationRequested)
                {
                    sendComplete?.Set();
                    return;
                }

                var len = 0;
                if (currentSend != null)
                {
                    len += currentSend.DataSize;
                    currentSend.Dispose();
                    currentSend = null;

                    if (Interlocked.Add(ref sendCounter, -len) == 0)
                    {
                        sendComplete.Set();
                        return;
                    }

                }


                //var b = _sockQueueSend?.DequeueBufferWait(65536, 3000, CancelToken);
                var b = _sockQueueSend?.DequeueBuffer(65536);
                if (b == null || b == BufferData.Empty)
                {
                    sendComplete?.Set();
                    return;
                }
                BeginSend(b);
            }
            catch (Exception ex)
            {
                Kernel?.Logger.TraceError($"{hashText} SockTcp.SendEventArgs_Completed ExceptionMessage:{ex.Message}");
                Kernel?.Logger.TraceError($"{hashText} SockTcp.SendEventArgs_Completed StackTrace:{ex.StackTrace}");
                SetErrorSend();
            }
        }


        public void SendAsync(BufferData buf)
        {
            IfThrowOnDisposed();
            if (disposedValue || SockState != SockState.Connect || CancelToken.IsCancellationRequested)
            {
                buf.Dispose();
                return;
            }
            _sockQueueSend.Enqueue(buf);
            var currentSize = Interlocked.Add(ref sendCounter, buf.DataSize);

            if (currentSize == buf.DataSize)
            {
                //SendEventArgs_Completed(_socket, null);
                //sendComplete.Reset();
                //BeginSend(buf);
                sendComplete.Reset();
                var b = _sockQueueSend?.DequeueBuffer(65536);
                BeginSend(b);
            }
        }

        public void SendAsyncWait()
        {
            if (SockState != SockState.Connect) return;
            sendComplete.Wait();
        }
        public bool IsSending()
        {
            if (SockState != SockState.Connect) return false;
            return sendComplete.IsLocked();
        }



        public int Send(byte[] buf, int length)
        {
            IfThrowOnDisposed();
            var lengthtxt = length.ToString();
            Kernel.Logger.DebugInformation(hashText, " SockTcp.Send ", lengthtxt);
            try
            {
                //Ver5.9.2 Java fix
                if (isSsl)
                {
                    return _oneSsl.Write(buf, buf.Length);
                }
                else
                {
                    return _socket.Send(buf, length, SocketFlags.None);
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



        public int Send(byte[] buf)
        {
            return Send(buf, buf.Length);
        }

        //1行送信
        //内部でCRLFの２バイトが付かされる
        public int LineSend(byte[] buf)
        {
            var d = new[] { new ArraySegment<byte>(buf), new ArraySegment<byte>(CrLf) };
            return Send(d);
        }

        //１行のString送信 (\r\nが付加される)
        public bool StringSend(string str, Encoding enc)
        {
            try
            {
                var buf = enc.GetBytes(str);
                LineSend(buf);
                return true;
            }
            catch (Exception e)
            {
                Util.RuntimeException(e.Message);
            }
            return false;
        }
        //１行のString送信(ASCII)  (\r\nが付加される)
        public bool StringSend(string str)
        {
            return StringSend(str, Encoding.ASCII);
        }

        //１行のString送信 (\r\nが付加される)
        public bool StringSend(string str, string charsetName)
        {
            try
            {
                var enc = CodePagesEncodingProvider.Instance.GetEncoding(charsetName);
                if (enc == null)
                    enc = Encoding.GetEncoding(charsetName);
                return StringSend(str, enc);
            }
            catch (Exception e)
            {
                Util.RuntimeException(e.Message);
            }
            return false;
        }


        //【送信】テキスト（バイナリかテキストかが不明な場合もこちら）
        public int SendUseEncode(byte[] buf)
        {
            //実際の送信処理にテキストとバイナリの区別はない
            return Send(buf);
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


        public void AsciiSendAsync(CharsData data)
        {
            using (CharsData d = data)
            {
                var buf = data.ToAsciiBufferData();
                SendAsync(buf);
            }
        }

        public void AsciiLineSendAsync(CharsData data)
        {
            using (CharsData d = data)
            {
                var buf = data.ToAsciiLineBufferData();
                SendAsync(buf);
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
            return LineSend(buf);
        }

        //【送信】バイナリ
        public int SendNoEncode(byte[] buf)
        {
            //実際の送信処理にテキストとバイナリの区別はない
            return SendNoTrace(buf);
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
                    _socket.Shutdown(SocketShutdown.Both);
                    //_socket.Shutdown(SocketShutdown.Receive);
                }
                catch (Exception ex)
                { Kernel?.Logger.TraceError($"{hashText} Dispose Error Shutdown {ex.Message} {ex.StackTrace} "); }

                try
                {
                    if (recvEventArgs != null)
                    {
                        recvEventArgs.Completed -= E_Completed;
                        recvEventArgs.Dispose();
                        recvEventArgs = null;
                    }
                    if (recvBuffer != null)
                    {
                        recvBuffer.Dispose();
                        recvBuffer = null;
                    }
                    recvComplete.Dispose();
                    recvComplete = null;
                }
                catch (Exception ex)
                { Kernel?.Logger.TraceError($"{hashText} Dispose Error recvComplete {ex.Message} {ex.StackTrace} "); }

                try
                {
                    if (sendEventArgs != null)
                    {
                        sendEventArgs.Completed -= SendEventArgs_Completed;
                        sendEventArgs.Dispose();
                        sendEventArgs = null;
                    }

                    if (sendBuffer != null)
                    {
                        sendBuffer.Dispose();
                        sendBuffer = null;
                    }
                    if (currentSend != null)
                    {
                        currentSend.Dispose();
                        currentSend = null;
                    }
                    sendComplete.Dispose();
                    sendComplete = null;
                }
                catch (Exception ex)
                { Kernel?.Logger.TraceError($"{hashText} Dispose Error sendComplete {ex.Message} {ex.StackTrace} "); }

                _lastLineSend = null;
                if (_sockQueueRecv != null)
                {
                    SockQueuePool.Instance.Pool(ref this._sockQueueRecv);
                    _sockQueueRecv = null;
                }
                if (_sockQueueSend != null)
                {
                    SockQueuePool.Instance.Pool(ref this._sockQueueSend);
                    _sockQueueSend = null;
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
