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
using Bjd.Threading;
using System.Threading.Tasks;

namespace Bjd.Net.Sockets
{
    //SockTcp 及び SockUdp の基底クラス
    public abstract class SockObj : IDisposable, ISocketBase
    {

        private IPEndPoint _RemoteAddress;
        private IPEndPoint _LocalAddress;
        private Ip _LocalIp;
        private Ip _RemoteIp;
        private SimpleAsyncAwaiter cancelWaiter = SimpleAsyncAwaiterPool.GetResetEvent(false);

        //****************************************************************
        // アドレス関連
        //****************************************************************
        public IPEndPoint RemoteAddress
        {
            get { return _RemoteAddress; }
            set
            {
                _RemoteAddress = value;
                var strIp = "0.0.0.0";
                if (_RemoteAddress != null)
                {
                    strIp = _RemoteAddress.Address.ToString();
                }
                _RemoteIp = new Ip(strIp);
            }
        }
        public IPEndPoint LocalAddress
        {
            get { return _LocalAddress; }
            set
            {
                _LocalAddress = value;
                var strIp = "0.0.0.0";
                if (_LocalAddress != null)
                {
                    strIp = _LocalAddress.Address.ToString();
                }
                _LocalIp = new Ip(strIp);
            }
        }
        public String RemoteHostname { get; private set; }

        private System.Threading.CancellationTokenSource cancelTokenSource;
        protected System.Threading.CancellationToken CancelToken { get; private set; }

        //このKernelはTrace()のためだけに使用されているので、Traceしない場合は削除することができる
        protected Kernel Kernel;


        public event EventHandler SocketStateChanged;
        protected void OnSocketStateChanged()
        {
            if (this.SocketStateChanged == null) return;
            try { this.SocketStateChanged(this, EventArgs.Empty); }
            catch { }
            //var t = System.Threading.Tasks.Task.Factory.StartNew(() => this.SocketStateChanged(this, EventArgs.Empty));
        }

        public Ip LocalIp
        {
            get
            {
                return _LocalIp;
            }
        }

        public Ip RemoteIp
        {
            get
            {
                return _RemoteIp;
            }
        }

        protected SockObj(Kernel kernel)
        {
            Kernel = kernel;
            SockState = SockState.Idle;
            LocalAddress = null;
            RemoteAddress = null;
            cancelTokenSource = new CancellationTokenSource();
            CancelToken = cancelTokenSource.Token;
            Kernel.Events.Cancel += this.KernelCancel;
        }

        private void KernelCancel(object sender, EventArgs e)
        {
            this.Cancel();
        }

        protected internal virtual void Cancel()
        {
            if (this.disposedValue) return;
            Kernel.Logger.DebugInformation("SockObj.Cancel");
            this.cancelTokenSource.Cancel();
            cancelWaiter.Set();
        }

        protected bool IsCancel
        {
            get
            {
                return this.CancelToken.IsCancellationRequested;
            }
        }
        public Task CancelWaitAsync()
        {
            return cancelWaiter.WaitAsync();
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
                _SockState = value;
                OnSocketStateChanged();
            }
        }
        private SockState _SockState;

        //ステータスの設定
        //Connect/bindで使用する
        protected void Set(SockState sockState, IPEndPoint localAddress, IPEndPoint remoteAddress)
        {
            LocalAddress = localAddress;
            RemoteAddress = remoteAddress;
            SockState = sockState;
        }

        //****************************************************************
        // エラー（切断）発生時にステータスの変更とLastErrorを設定するメソッド
        //****************************************************************
        protected void SetException(Exception ex)
        {
            Kernel.Logger.TraceError($"{this.GetType().Name}.SetException {ex.Message}");
            _lastError = $"[{ex.Source}] {ex.Message}";
            SockState = SockState.Error;
        }

        protected void SetError(String msg)
        {
            if (!disposedValue)
            {
                Kernel?.Logger.TraceError($"{this.GetType().Name}.SetError {msg}");
                _lastError = msg;
            }
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
                if (Kernel != null)
                {
                    Kernel.Events.Cancel -= this.KernelCancel;
                }

                _RemoteAddress = null;
                _RemoteIp = null;
                _LocalAddress = null;
                _LocalIp = null;

                //if (!this.IsCancel) this.Cancel();
                if (cancelTokenSource != null) cancelTokenSource.Dispose();
                if (cancelWaiter != null) cancelWaiter.Dispose();
                cancelTokenSource = null;
                SocketStateChanged = null;
                _lastError = null;
                Kernel = null;

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
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

