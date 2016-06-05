﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bjd.Net;
using Bjd.Traces;
using Bjd.Utils;

namespace Bjd.Net.Sockets
{
    public class SockTcp : SockObj
    {

        //private Selector selector = null;
        //private SocketChannel channel = null; //ACCEPTの場合は、コンストラクタでコピーされる
        private Socket _socket;
        private Ssl _ssl;

        private OneSsl _oneSsl;
        private SockQueue _sockQueue = new SockQueue();
        //ByteBuffer recvBuf = ByteBuffer.allocate(sockQueue.Max);
        private byte[] _recvBuf; //１行処理のためのテンポラリバッファ
        private ArraySegment<byte> _recvBufSegment;

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

            _socket = new Socket((ip.InetKind == InetKind.V4) ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                //socket.Connect(ip.IPAddress, port);
                //_socket.BeginConnect(ip.IPAddress, port, CallbackConnect, this);
                var tConnect = _socket.ConnectAsync(ip.IPAddress, port);
                tConnect.ContinueWith(_ => CallbackConnect(), kernel.CancelToken);
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
            //************************************************
            //ここまでくると接続が完了している
            //************************************************
        }

        private void CallbackConnect()
        {
            if (_socket.Connected)
            {
                //ここまでくると接続が完了している
                if (_ssl != null)
                {
                    //SSL通信の場合は、SSLのネゴシエーションが行われる
                    _oneSsl = _ssl.CreateClientStream(_socket);
                    if (_oneSsl == null)
                    {
                        SetError("_ssl.CreateClientStream() faild");
                        return;
                    }
                }
                BeginReceive(); //接続完了処理（受信待機開始）
            }
            else {
                SetError("CallbackConnect() faild");
            }
        }

        //通常のサーバでは、このファンクションを外部で作成する
        //private void CallbackConnect(IAsyncResult ar)
        //{
        //    if (_socket.Connected)
        //    {
        //        _socket.EndConnect(ar);
        //        //ここまでくると接続が完了している
        //        if (_ssl != null)
        //        {
        //            //SSL通信の場合は、SSLのネゴシエーションが行われる
        //            _oneSsl = _ssl.CreateClientStream(_socket);
        //            if (_oneSsl == null)
        //            {
        //                SetError("_ssl.CreateClientStream() faild");
        //                return;
        //            }
        //        }
        //        BeginReceive(); //接続完了処理（受信待機開始）
        //    }
        //    else {
        //        SetError("CallbackConnect() faild");
        //    }
        //}


        //ACCEPT
        //Ver5.9.2 Java fix
        //public SockTcp(Kernel kernel, Socket s) : base(kernel){
        public SockTcp(Kernel kernel, Ssl ssl, Socket s) : base(kernel)
        {

            //************************************************
            //selector/channel生成
            //************************************************
            _socket = s;
            _ssl = ssl;

            //既に接続を完了している
            if (_ssl != null)
            {
                //SSL通信の場合は、SSLのネゴシエーションが行われる
                _oneSsl = _ssl.CreateServerStream(_socket);
                if (_oneSsl == null)
                {
                    SetError("_ssl.CreateServerStream() faild");
                    return;
                }
            }

            //************************************************
            //ここまでくると接続が完了している
            //************************************************
            //Set(SockState.Connect, (InetSocketAddress) channel.socket().getLocalSocketAddress(), (InetSocketAddress) channel.socket().getRemoteSocketAddress());

            //************************************************
            //read待機
            //************************************************
            BeginReceive(); //接続完了処理（受信待機開始）
        }

        public int Length()
        {
            Thread.Sleep(1); //次の動作が実行されるようにsleepを置く
            return _sockQueue.Length;
        }

        //接続完了処理（受信待機開始）
        private void BeginReceive()
        {
            System.Diagnostics.Trace.TraceInformation("SockTcp.BeginReceive");
            //受信バッファは接続完了後に確保される
            _sockQueue = new SockQueue();
            _recvBuf = new byte[_sockQueue.Space]; //キューが空なので、Spaceはバッファの最大サイズになっている
            _recvBufSegment = new ArraySegment<byte>(_recvBuf);

            // Using the LocalEndPoint property.
            string s = string.Format("My local IpAddress is :" + IPAddress.Parse(((IPEndPoint)_socket.LocalEndPoint).Address.ToString()) + "I am connected on port number " + ((IPEndPoint)_socket.LocalEndPoint).Port.ToString());

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

            //受信待機の開始(oneSsl!=nullの場合、受信バイト数は0に設定する)
            //socket.BeginReceive(tcpBuffer, 0, (oneSsl != null) ? 0 : tcpQueue.Space, SocketFlags.None, new AsyncCallback(EndReceive), this);

            try
            {
                if (_ssl != null)
                {
                    //Ver5.9.2 Java fix
                    _oneSsl.BeginRead(_recvBuf, 0, _sockQueue.Space, EndReceiveSsl, this, Kernel.CancelToken);
                }
                else {
                    //_socket.BeginReceive(_recvBuf, 0, _sockQueue.Space, SocketFlags.None, EndReceive, this);
                    var tReceive = _socket.ReceiveAsync(_recvBufSegment, SocketFlags.None);
                    tReceive.ContinueWith(_ => this.EndReceive(_), Kernel.CancelToken);
                }
            }
            catch
            {
                SetError("BeginRecvive() faild.");
            }
        }

        public void EndReceive(Task<int> result)
        {
            //System.Diagnostics.Trace.TraceInformation("SockTcp.EndReceive");

            if (!result.IsCompleted)
            {
                System.Diagnostics.Trace.TraceError(result.Exception.Message);
                System.Diagnostics.Trace.TraceError(result.Exception.StackTrace);
                this.SetErrorReceive();
                return;
            }

            //受信完了
            lock (this)
            {
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
                    _sockQueue.Enqueue(_recvBuf, bytesRead); //キューへの格納
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError($"SockTcp.EndReceive {ex.Message}");
                    //受信待機のままソケットがクローズされた場合は、ここにくる
                    this.SetErrorReceive();
                    return;
                }
            }

            //バッファがいっぱい 空の受信待機をかける
            //受信待機
            while ((_sockQueue.Space) == 0)
            {
                Thread.Sleep(10); //他のスレッドに制御を譲る  
                if (SockState != SockState.Connect)
                {
                    this.SetErrorReceive();
                    return;
                }
            }

            //受信待機の開始
            try
            {
                //Ver5.9.2 Java fix
                //_socket.BeginReceive(_recvBuf, 0, _sockQueue.Space, SocketFlags.None, EndReceive, this);
                var tReceive = _socket.ReceiveAsync(new ArraySegment<byte>(_recvBuf, 0, _sockQueue.Space), SocketFlags.None);
                tReceive.ContinueWith(_ => this.EndReceive(_));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation($"SockTcp.EndReceive {ex.Message}");
                this.SetErrorReceive();
                return;
            }
        }

        private void SetErrorReceive()
        {
            //err: //エラー発生
            //【2009.01.12 追加】相手が存在しなくなっている
            SetError("disconnect");
            //state = SocketObjState.Disconnect;

            //Close();クローズは外部から明示的に行う
        }

        //受信処理・受信待機
        public void EndReceiveSsl(IAsyncResult ar)
        {
            System.Diagnostics.Trace.TraceInformation("SockTcp.EndReceive");
            if (ar == null)
            {
                //受信待機
                while ((_sockQueue.Space) == 0)
                {
                    Thread.Sleep(10); //他のスレッドに制御を譲る  
                    if (SockState != SockState.Connect)
                    {
                        this.SetErrorReceive();
                        return;
                    }
                }
            }
            else {
                //受信完了
                lock (this)
                {
                    //ポインタを移動する場合は、排他制御が必要
                    try
                    {
                        //Ver5.9.2 Java fix
                        int bytesRead = _oneSsl.EndRead(ar);
                        if (bytesRead == 0)
                        {
                            //  切断されている場合は、0が返される?
                            if (_ssl == null)
                            {
                                this.SetErrorReceive();
                                return;
                            }
                            Thread.Sleep(10); //Ver5.0.0-a19
                        }
                        else if (bytesRead < 0)
                        {
                            this.SetErrorReceive();
                            return;
                        }
                        else {
                            _sockQueue.Enqueue(_recvBuf, bytesRead); //キューへの格納
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError($"SockTcp.EndReceive {ex.Message}");
                        //受信待機のままソケットがクローズされた場合は、ここにくる
                        this.SetErrorReceive();
                        return;
                    }
                }
            }

            if (_sockQueue.Space == 0)
            {
                //バッファがいっぱい 空の受信待機をかける
                EndReceiveSsl(null);
            }
            else
            {
                //受信待機の開始
                try
                {
                    _oneSsl.BeginRead(_recvBuf, 0, _sockQueue.Space, EndReceiveSsl, this, Kernel.CancelToken);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"SockTcp.EndReceive {ex.Message}");
                    System.Diagnostics.Trace.WriteLine($"SockTcp.EndReceive {ex.StackTrace}");
                    this.SetErrorReceive();
                    return;
                }
            }
        }


        //受信<br>
        //切断・タイムアウトでnullが返される
        public byte[] Recv(int len, int sec, ILife iLife)
        {

            var tout = new Utils.Timeout(sec);

            var buffer = new byte[0];
            try
            {
                if (len <= _sockQueue.Length)
                {
                    // キューから取得する
                    buffer = _sockQueue.Dequeue(len);

                }
                else {
                    while (iLife.IsLife())
                    {
                        Thread.Sleep(0);
                        if (0 < _sockQueue.Length)
                        {
                            //Java fix 
                            tout.Update(); //少しでも受信があった場合は、タイムアウトを更新する

                            //size=受信が必要なバイト数
                            int size = len - buffer.Length;

                            //受信に必要なバイト数がバッファにない場合
                            if (size > _sockQueue.Length)
                            {
                                size = _sockQueue.Length; //とりあえずバッファサイズ分だけ受信する
                            }
                            byte[] tmp = _sockQueue.Dequeue(size);
                            buffer = Bytes.Create(buffer, tmp);

                            //Java fix Ver5.8.2
                            if (buffer.Length != 0)
                            {
                                break;
                            }
                        }
                        else {
                            if (SockState != SockState.Connect)
                            {
                                return null;
                            }
                            Thread.Sleep(10);
                        }
                        if (tout.IsFinish())
                        {
                            buffer = _sockQueue.Dequeue(len); //タイムアウト
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"SockTcp.EndReceive {ex.Message}");
                System.Diagnostics.Trace.TraceError($"SockTcp.EndReceive {ex.StackTrace}");
                //ex.printStackTrace();
                return null;
            }
            Trace(TraceKind.Recv, buffer, false);

            return buffer;
        }

        //1行受信
        //切断・タイムアウトでnullが返される
        public byte[] LineRecv(int sec, ILife iLife)
        {
            var tout = new Utils.Timeout(sec);

            while (iLife.IsLife())
            {
                //Ver5.1.6
                if (_sockQueue.Length == 0)
                {
                    Thread.Sleep(100);
                }
                byte[] buf = _sockQueue.DequeueLine();
                //noEncode = false;//テキストである事が分かっている
                Trace(TraceKind.Recv, buf, false);
                if (buf.Length != 0)
                {
                    //Ver5.8.6 Java fix
                    tout.Update(); //タイムアウトの更新
                    return buf;
                }
                if (SockState != SockState.Connect)
                {
                    return null;
                }
                if (tout.IsFinish())
                {
                    return null; //タイムアウト
                }
                Thread.Sleep(1);
            }
            return null;
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
                else {
                    Trace(TraceKind.Send, buf, false);
                }
                //Ver5.9.2 Java fix
                if (_oneSsl != null)
                {
                    return _oneSsl.Write(buf, buf.Length);
                }
                else {
                    //return _socket.Send(buf, 0, length, SocketFlags.None);
                    var t = _socket.SendAsync(new ArraySegment<byte>(buf, 0, length), SocketFlags.None);
                    t.Wait(this.Kernel.CancelToken);
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
            try
            {
                this._socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                //TCPのサーバソケットをシャットダウンするとエラーになる（無視する）
            }
            if (_socket != null)
            {
                //_socket.Close();
                _socket.Dispose();
                _socket = null;
            }
            if (_oneSsl != null)
            {
                _oneSsl.Close();
                _oneSsl = null;
            }

            SetError("close()");
        }

        //【送信】(トレースなし)
        //リモートサーバがトレース内容を送信するときに更にトレースするとオーバーフローするため
        //RemoteObj.Send()では、こちらを使用する
        public int SendNoTrace(byte[] buffer)
        {
            try
            {
                if (_oneSsl != null)
                {
                    return _oneSsl.Write(buffer, buffer.Length);
                }
                if (_socket.Connected)
                    return _socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
            catch (Exception ex)
            {
                SetError(string.Format("Length={0} {1}", buffer.Length, ex.Message));
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

        protected override void Dispose(bool disposing)
        {
            this._lastLineSend = null;
            this._recvBuf = null;
            if (this._socket != null)
            {
                this._socket.Dispose();
                this._socket = null;
            }
            if (this._oneSsl != null)
            {
                this._oneSsl.Close();
                this._oneSsl = null;
            }
            this._sockQueue = null;
            this._ssl = null;

            base.Dispose(disposing);
        }

    }
}
