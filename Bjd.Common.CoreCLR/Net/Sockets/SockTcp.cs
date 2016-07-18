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

namespace Bjd.Net.Sockets
{
    public class SockTcp : SockObj
    {

        private Socket _socket;
        private Ssl _ssl;
        private OneSsl _oneSsl;
        private SockQueue _sockQueue;
        private bool isSsl = false;

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

            _socket = new Socket((ip.InetKind == InetKind.V4) ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                var tConnect = _socket.ConnectAsync(ip.IPAddress, port);
                tConnect.ContinueWith(_ => CallbackConnect(), this.CancelToken);
            }
            catch
            {
                SetError("BeginConnect() faild");
            }
            //[C#] 接続が完了するまで待機する
            while (SockState == SockState.Idle)
            {
                Thread.Sleep(10);
            }
            //ここまでくると接続が完了している
        }

        private void CallbackConnect()
        {
            if (_socket.Connected)
            {
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
                SetConnectionInfo();
                BeginReceive(); //接続完了処理（受信待機開始）
            }
            else
            {
                SetError("CallbackConnect() faild");
            }
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
            //read待機
            //接続完了処理（受信待機開始）
            var t = new Task(() => { BeginReceive(); }, this.CancelToken, TaskCreationOptions.LongRunning);
            t.Start();
        }

        public int Length()
        {
            return _sockQueue.Length;
        }

        private void SetConnectionInfo()
        {
            //受信バッファは接続完了後に確保される
            //_sockQueue = new SockQueue();
            _sockQueue = SockQueuePool.Instance.Get();

            try
            {
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
        private void BeginReceive()
        {
            System.Diagnostics.Trace.TraceInformation("SockTcp.BeginReceive");

            // Using the LocalEndPoint property.
            //string s = string.Format("My local IpAddress is :" + IPAddress.Parse(((IPEndPoint)_socket.LocalEndPoint).Address.ToString()) + "I am connected on port number " + ((IPEndPoint)_socket.LocalEndPoint).Port.ToString());

            //受信待機の開始(oneSsl!=nullの場合、受信バイト数は0に設定する)
            try
            {
                var recvBufSeg = _sockQueue.GetWriteSegment();
                Task<int> tReceive;
                if (isSsl)
                {
                    tReceive = _oneSsl.ReadAsync(recvBufSeg, this.CancelToken);
                }
                else
                {
                    tReceive = _socket.ReceiveAsync(recvBufSeg, SocketFlags.None);
                }
                var ts = TaskScheduler.Current;
                tReceive.ContinueWith(_ => this.EndReceive(_), this.CancelToken, TaskContinuationOptions.LongRunning, ts);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"SockTcp.EndReceive {ex.Message}");
                System.Diagnostics.Trace.TraceError($"SockTcp.EndReceive {ex.StackTrace}");
                this.SetErrorReceive();
            }
        }

        public void EndReceive(Task<int> result)
        {
            System.Diagnostics.Trace.TraceInformation("SockTcp.EndReceive");

            if (result.IsFaulted || !result.IsCompleted)
            {
                System.Diagnostics.Trace.TraceError(result.Exception.Message);
                System.Diagnostics.Trace.TraceError(result.Exception.StackTrace);
                this.SetErrorReceive();
                return;
            }

            //受信完了
            //ポインタを移動する場合は、排他制御が必要
            try
            {
                //Ver5.9.2 Java fix
                int bytesRead = result.Result;
                if (bytesRead <= 0)
                {
                    //  切断されている場合は、0が返される?
                    this.SetErrorReceive();
                    return;
                }
                _sockQueue.NotifyWrite(bytesRead);

            }
            catch (Exception ex)
            {
                //受信待機のままソケットがクローズされた場合は、ここにくる
                System.Diagnostics.Trace.TraceError($"SockTcp.EndReceive {ex.Message}");
                this.SetErrorReceive();
                return;
            }

            //バッファがいっぱい 空の受信待機をかける
            //受信待機
            while ((_sockQueue.Space) == 0)
            {
                Thread.Sleep(10); //他のスレッドに制御を譲る  
                if (disposedValue) return;
                if (CancelToken.IsCancellationRequested) return;
                if (SockState != SockState.Connect)
                {
                    System.Diagnostics.Trace.TraceInformation($"SockTcp.EndReceive Not Connected");
                    this.SetErrorReceive();
                    return;
                }
            }

            //受信待機の開始
            BeginReceive();

        }

        private void SetErrorReceive()
        {
            //err: //エラー発生
            //【2009.01.12 追加】相手が存在しなくなっている
            SetError("disconnect");
            //state = SocketObjState.Disconnect;
            //Close();クローズは外部から明示的に行う
        }

        //受信<br>
        //切断・タイムアウトでnullが返される
        public byte[] Recv(int len, int sec, ILife iLife)
        {
            var toutms = sec * 1000;
            var result = _sockQueue.DequeueWait(len, toutms, this.CancelToken);
            if (result.Length == 0 && SockState != SockState.Connect) return null;
            return result;
        }

        //1行受信
        //切断・タイムアウトでnullが返される
        public byte[] LineRecv(int sec, ILife iLife)
        {
            var toutms = sec * 1000;
            var result = _sockQueue.DequeueLineWait(toutms, this.CancelToken);
            if (result.Length == 0) return null;
            return result;
        }

        //１行のString受信
        public string StringRecv(string charsetName, int sec, ILife iLife)
        {
            try
            {
                byte[] bytes = LineRecv(sec, iLife);

                //[C#]
                if (bytes == null)
                {
                    return null;
                }
                var enc = CodePagesEncodingProvider.Instance.GetEncoding(charsetName);
                if (enc == null)
                    enc = Encoding.GetEncoding(charsetName);
                return enc.GetString(bytes);
            }
            catch (Exception e)
            {
                Util.RuntimeException(e.Message);
            }
            return null;
        }

        //１行受信(ASCII)
        public string StringRecv(int sec, ILife iLife)
        {
            return StringRecv("ASCII", sec, iLife);
        }

        public int Send(byte[] buf, int length)
        {
            try
            {
                if (buf.Length != length)
                {
                    var b = new byte[length];
                    Buffer.BlockCopy(buf, 0, b, 0, length);
                    Trace(TraceKind.Send, b, false);
                }
                else
                {
                    Trace(TraceKind.Send, buf, false);
                }
                //Ver5.9.2 Java fix
                if (isSsl)
                {
                    return _oneSsl.Write(buf, buf.Length);
                }
                else
                {
                    //return _socket.Send(buf, 0, length, SocketFlags.None);
                    var t = _socket.SendAsync(new ArraySegment<byte>(buf, 0, length), SocketFlags.None);
                    t.Wait(this.CancelToken);
                    return t.Result;
                }
            }
            catch (Exception e)
            {
                SetException(e);
                return -1;
            }
        }

        public int Send(byte[] buf)
        {
            return Send(buf, buf.Length);
        }

        //1行送信
        //内部でCRLFの２バイトが付かされる
        public int LineSend(byte[] buf)
        {
            var b = new byte[buf.Length + 2];
            Buffer.BlockCopy(buf, 0, b, 0, buf.Length);
            b[buf.Length] = 0x0d;
            b[buf.Length + 1] = 0x0a;
            return Send(b);
        }

        //１行のString送信(ASCII)  (\r\nが付加される)
        public bool StringSend(String str)
        {
            return StringSend(str, "ASCII");
        }

        //１行のString送信 (\r\nが付加される)
        public bool StringSend(string str, string charsetName)
        {
            try
            {
                var enc = CodePagesEncodingProvider.Instance.GetEncoding(charsetName);
                if (enc == null)
                    enc = Encoding.GetEncoding(charsetName);
                var buf = enc.GetBytes(str);
                //byte[] buf = str.getBytes(charsetName);
                LineSend(buf);
                return true;
            }
            catch (Exception e)
            {
                Util.RuntimeException(e.Message);
            }
            return false;
        }

        //１行送信(ASCII)  (\r\nが付加される)
        public bool SstringSend(string str)
        {
            return StringSend(str, "ASCII");
        }

        public override void Close()
        {
            if (this.disposedValue) return;
            this.Dispose();
        }

        //【送信】(トレースなし)
        //リモートサーバがトレース内容を送信するときに更にトレースするとオーバーフローするため
        //RemoteObj.Send()では、こちらを使用する
        public int SendNoTrace(byte[] buffer)
        {
            try
            {
                if (isSsl)
                {
                    return _oneSsl.Write(buffer, buffer.Length);
                }

                if (_socket.Connected)
                {
                    var arry = new ArraySegment<byte>(buffer, 0, buffer.Length);
                    var result = _socket.SendAsync(arry, SocketFlags.None);
                    result.Wait(this.CancelToken);
                    if (result.IsCompleted)
                        return result.Result;
                    //return _socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                SetError($"SendNoTrace Length={buffer.Length} {ex.Message}");
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
                    var result = _socket.SendAsync(buffer, SocketFlags.None);
                    result.Wait(this.CancelToken);
                    if (result.IsCompleted)
                        return result.Result;
                    //return _socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                SetError($"SendNoTrace Length={buffer.Count} {ex.Message}");
                //Logger.Set(LogKind.Error, this, 9000046, string.Format("Length={0} {1}", buffer.Length, ex.Message));
            }
            return -1;
        }


        //【送信】テキスト（バイナリかテキストかが不明な場合もこちら）
        public int SendUseEncode(byte[] buf)
        {
            //テキストである可能性があるのでエンコード処理は省略できない
            Trace(TraceKind.Send, buf, false); //noEncode = false テキストである可能性があるのでエンコード処理は省略できない
            //実際の送信処理にテキストとバイナリの区別はない
            return SendNoTrace(buf);
        }

        //【送信】テキスト（バイナリかテキストかが不明な場合もこちら）
        public int SendUseEncode(ArraySegment<byte> buf)
        {
            //テキストである可能性があるのでエンコード処理は省略できない
            Trace(TraceKind.Send, buf, false); //noEncode = false テキストである可能性があるのでエンコード処理は省略できない
            //実際の送信処理にテキストとバイナリの区別はない
            return SendNoTrace(buf);
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
            return LineSend(buf);
        }

        //AsciiSendを使用したいが、文字コードがASCII以外の可能性がある場合、こちらを使用する  (\r\nが付加される)
        //public int SjisSend(string str, OperateCrlf operateCrlf) {
        public int SjisSend(string str)
        {
            _lastLineSend = str;
            var enc = CodePagesEncodingProvider.Instance.GetEncoding(932);
            var buf = enc.GetBytes(str);
            //return LineSend(buf, operateCrlf);
            //とりあえずCrLfの設定を無視している
            return LineSend(buf);
        }

        // 【１行受信】
        //切断されている場合、nullが返される
        //public string AsciiRecv(int timeout, OperateCrlf operateCrlf, ILife iLife) {
        public string AsciiRecv(int timeout, ILife iLife)
        {
            var buf = LineRecv(timeout, iLife);
            return buf == null ? null : Encoding.ASCII.GetString(buf);
        }

        //【送信】バイナリ
        public int SendNoEncode(byte[] buf)
        {
            //バイナリであるのでエンコード処理は省略される
            Trace(TraceKind.Send, buf, true); //noEncode = true バイナリであるのでエンコード処理は省略される
            //実際の送信処理にテキストとバイナリの区別はない
            return SendNoTrace(buf);
        }

        public int SendNoEncode(ArraySegment<byte> buf)
        {
            //バイナリであるのでエンコード処理は省略される
            Trace(TraceKind.Send, buf, true); //noEncode = true バイナリであるのでエンコード処理は省略される
            //実際の送信処理にテキストとバイナリの区別はない
            return SendNoTrace(buf);
        }


        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                //TCPのサーバソケットをシャットダウンするとエラーになる（無視する）
                try { this.Cancel(); }
                catch { }
                //TCPのサーバソケットをシャットダウンするとエラーになる（無視する）
                try { if (this._socket != null && this._socket.Connected) this._socket.Shutdown(SocketShutdown.Both); }
                catch { }
                if (_socket != null)
                {
                    //_socket.Close();
                    _socket.Poll(500000, SelectMode.SelectRead);
                    _socket.Dispose();
                    _socket = null;
                }
                if (_oneSsl != null)
                {
                    _oneSsl.Close();
                    _oneSsl = null;
                }

                this._lastLineSend = null;
                if (this._sockQueue != null)
                {
                    //this._sockQueue.Dispose();
                    SockQueuePool.Instance.Pool(ref this._sockQueue);
                    this._sockQueue = null;
                }
                if (this._ssl != null)
                {
                    this._ssl.Dispose();
                    this._ssl = null;
                }
                disposedValue = true;
                SetError("Dispose");
            }

            base.Dispose(disposing);
        }

    }
}
