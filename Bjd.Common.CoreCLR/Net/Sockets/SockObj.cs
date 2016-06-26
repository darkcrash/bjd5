using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Traces;
using Bjd.Utils;
using System.Threading;

namespace Bjd.Net.Sockets
{
    //SockTcp 及び SockUdp の基底クラス
    public abstract class SockObj : IDisposable
    {

        //****************************************************************
        // アドレス関連
        //****************************************************************
        public IPEndPoint RemoteAddress { get; set; }
        public IPEndPoint LocalAddress { get; set; }
        public String RemoteHostname { get; private set; }

        private System.Threading.CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        protected System.Threading.CancellationToken CancelToken { get; private set; }

        //このKernelはTrace()のためだけに使用されているので、Traceしない場合は削除することができる
        protected Kernel Kernel;


        public event EventHandler SocketStateChanged;
        protected void OnSocketStateChanged()
        {
            if (this.SocketStateChanged == null) return;
            var t = System.Threading.Tasks.Task.Factory.StartNew(() => this.SocketStateChanged(this, EventArgs.Empty));
        }

        public Ip LocalIp
        {
            get
            {
                var strIp = "0.0.0.0";
                if (LocalAddress != null)
                {
                    strIp = LocalAddress.Address.ToString();
                }
                return new Ip(strIp);
            }
        }

        public Ip RemoteIp
        {
            get
            {
                var strIp = "0.0.0.0";
                if (RemoteAddress != null)
                {
                    strIp = RemoteAddress.Address.ToString();
                }
                return new Ip(strIp);
            }
        }

        protected SockObj(Kernel kernel)
        {
            Kernel = kernel;
            SockState = SockState.Idle;
            LocalAddress = null;
            RemoteAddress = null;
            this.CancelToken = cancelTokenSource.Token;
            this.Kernel.CancelToken.Register(this.Cancel);
        }

        protected internal virtual void Cancel()
        {
            this.cancelTokenSource.Cancel();
        }

        protected bool IsCancel
        {
            get
            {
                return this.CancelToken.IsCancellationRequested;
            }
        }


        //****************************************************************
        // LastError関連
        //****************************************************************
        private String _lastError = "";

        //LastErrorの取得
        public String GetLastEror()
        {
            return _lastError;
        }

        //****************************************************************
        // SockState関連
        //****************************************************************
        public SockState SockState
        {
            get
            {
                return _SockState;
            }

            protected set
            {
                if (value == _SockState) return;
                OnSocketStateChanged();
                _SockState = value;
            }
        }
        private SockState _SockState;


        //ステータスの設定
        //Connect/bindで使用する
        protected void Set(SockState sockState, IPEndPoint localAddress, IPEndPoint remoteAddress)
        {
            SockState = sockState;
            LocalAddress = localAddress;
            RemoteAddress = remoteAddress;
        }

        //****************************************************************
        // エラー（切断）発生時にステータスの変更とLastErrorを設定するメソッド
        //****************************************************************
        protected void SetException(Exception ex)
        {
            System.Diagnostics.Trace.TraceError($"SockObj.SetException {ex.Message}");
            _lastError = string.Format("[{0}] {1}", ex.Source, ex.Message);
            SockState = SockState.Error;
        }

        protected void SetError(String msg)
        {
            System.Diagnostics.Trace.TraceError($"{this.GetType().Name}.SetError {msg}");
            _lastError = msg;
            SockState = SockState.Error;
        }

        //TODO メソッドの配置はここでよいか？
        public void Resolve(bool useResolve, Logger logger)
        {
            if (useResolve)
            {
                RemoteHostname = "resolve error!";
                try
                {
                    RemoteHostname = Kernel.DnsCache.GetHostName(RemoteAddress.Address, Kernel.CreateLogger("SockObj", true, null));
                }
                catch (Exception ex)
                {
                    logger.Set(LogKind.Error, null, 9000053, ex.Message);
                }
            }
            else
            {
                String ipStr = RemoteAddress.Address.ToString();
                if (ipStr[0] == '/')
                {
                    ipStr = ipStr.Substring(1);
                }
                RemoteHostname = ipStr;
            }

        }

        public abstract void Close();


        //バイナリデータであることが判明している場合は、noEncodeをtrueに設定する
        //これによりテキスト判断ロジックを省略できる
        protected void Trace(TraceKind traceKind, byte[] buf, bool noEncode)
        {

            if (buf == null || buf.Length == 0)
            {
                return;
            }

            bool isText = false; //対象がテキストかどうかの判断
            Encoding encoding = null;

            if (!noEncode)
            {
                //エンコード試験が必要な場合
                try
                {
                    encoding = MLang.GetEncoding(buf);
                }
                catch
                {
                    encoding = null;
                }
                if (encoding != null)
                {
                    //int codePage = encoding.CodePage;
                    if (encoding.CodePage == 20127 || encoding.CodePage == 65001 || encoding.CodePage == 51932 || encoding.CodePage == 1200 || encoding.CodePage == 932 || encoding.CodePage == 50220)
                    {
                        //"US-ASCII" 20127
                        //"Unicode (UTF-8)" 65001
                        //"日本語(EUC)" 51932
                        //"Unicode" 1200
                        //"日本語(シフトJIS)" 932
                        //日本語(JIS) 50220
                        isText = true;
                    }
                }
            }

            var ar = new List<String>();
            if (isText)
            {
                var lines = Inet.GetLines(buf);
                ar.AddRange(lines.Select(line => encoding.GetString(Inet.TrimCrlf(line))));
            }
            else
            {
                ar.Add(noEncode ? string.Format("binary {0} byte", buf.Length) : string.Format("Binary {0} byte", buf.Length));
            }
            foreach (var str in ar)
            {
                Ip ip = RemoteIp;

                if (Kernel.RemoteConnect != null)
                {
                    //リモートサーバへもデータを送る（クライアントが接続中の場合は、クライアントへ送信される）
                    Kernel.RemoteConnect.AddTrace(traceKind, str, ip);
                }
            }

        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                this.Cancel();
                this._lastError = null;
                this.Kernel = null;

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~SockObj()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

