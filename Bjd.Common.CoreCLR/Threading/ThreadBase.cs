using System;
using System.Threading;
using Bjd.Logs;

namespace Bjd.Threading
{

    //スレッドの起動停止機能を持った基本クラス
    public abstract class ThreadBase : IDisposable, ILogger, ILife
    {

        private event EventHandler ThreadCancel;
        protected void OnThreadCancel()
        {
            if (ThreadCancel == null) return;
            try { ThreadCancel(this, EventArgs.Empty); }
            catch { }
        }



        private Thread _t;
        private ThreadBaseKind _threadBaseKind = ThreadBaseKind.Before;
        private ManualResetEventSlim RunningWait = new ManualResetEventSlim(false);
        private ManualResetEventSlim AfterWait = new ManualResetEventSlim(false);

        public ThreadBaseKind ThreadBaseKind
        {
            get { return _threadBaseKind; }
            protected set
            {
                if (_threadBaseKind == value) return;
                var before = _threadBaseKind;
                _threadBaseKind = value;
                if (value == ThreadBaseKind.Running) { RunningWait.Set(); }
                if (before == ThreadBaseKind.Running) { RunningWait.Reset(); }
                if (value == ThreadBaseKind.After) { AfterWait.Set(); }
                if (before == ThreadBaseKind.After) { AfterWait.Reset(); }
            }
        }
        private bool _life = false; //スレッドを停止するためのスイッチ
        private Logger _logger;
        protected Kernel _kernel; //SockObjのTraceのため
        private CancellationTokenSource _cancelTokenSource;
        protected CancellationToken _cancelToken;
        private bool isDisposed = false;


        //logger　スレッド実行中に例外がスローされたとき表示するためのLogger(nullを設定可能)
        protected ThreadBase(Kernel kernel, Logger logger)
        {
            _kernel = kernel;
            _logger = logger;

            // タスクのキャンセルにサーバー停止イベントを登録
            _kernel.Events.Cancel += Events_Cancel;

        }

        private void Events_Cancel(object sender, EventArgs e)
        {
            this.Cancel();
        }

        protected void Cancel()
        {
            if (isDisposed) return;
            this.Stop();
        }

        //時間を要するループがある場合、ループ条件で値がtrueであることを確認する<br>
        // falseになったら直ちにループを中断する
        public bool IsLife()
        {
            return _life;
        }

        //終了処理
        //Override可能
        public virtual void Dispose()
        {
            _kernel.Events.Cancel -= Events_Cancel;
            Stop();
            if (RunningWait != null) RunningWait.Dispose();
            RunningWait = null;
            if (AfterWait != null) AfterWait.Dispose();
            AfterWait = null;
            _logger = null;
            _kernel = null;
            isDisposed = true;
        }

        //【スレッド開始前処理】
        //falseでスレッド起動をやめる
        protected abstract bool OnStartThread();

        //開始処理
        //Override可能
        public void Start()
        {
            System.Diagnostics.Trace.TraceWarning($"{this.GetType().FullName}.Start Begin");
            if (_threadBaseKind == ThreadBaseKind.Running)
                return;

            if (!OnStartThread())
            {
                //Ver5.9.8
                this.Cancel();
                return;
            }

            try
            {
                //Ver5.9.0
                ThreadBaseKind = ThreadBaseKind.Before;

                _cancelTokenSource = new CancellationTokenSource();
                _cancelToken = _cancelTokenSource.Token;
                _life = true;
                _t = new Thread(Loop) { IsBackground = true, Name = this.GetType().FullName };
                _t.Start();

                //スレッドが起動してステータスがRUNになるまで待機する
                RunningWait.Wait();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning($"{this.GetType().FullName}.Start {ex.Message}");
            }
            System.Diagnostics.Trace.TraceWarning($"{this.GetType().FullName}.Start End");
        }

        //【スレッド終了処理】
        protected abstract void OnStopThread();

        //停止処理
        //Override可能
        public void Stop()
        {
            System.Diagnostics.Trace.TraceWarning($"{this.GetType().FullName}.Stop Begin");
            if (_t != null && _threadBaseKind == ThreadBaseKind.Running)
            {
                //起動されている場合
                OnThreadCancel();
                _life = false;
                _cancelTokenSource.Cancel();
                _cancelTokenSource.Dispose();
                _cancelTokenSource = null;
                OnStopThread();
                //_life = false;//スイッチを切るとLoop内の無限ループからbreakする
                //while (_threadBaseKind != ThreadBaseKind.After)
                //{
                //    Thread.Sleep(100);//breakした時点でIsRunがfalseになるので、ループを抜けるまでここで待つ
                //}
                AfterWait.Wait();
            }
            _t = null;
            System.Diagnostics.Trace.TraceWarning($"{this.GetType().FullName}.Stop End");
        }

        protected abstract void OnRunThread();

        private void Loop()
        {
            //[Java] 現在、Javaでは、ここでThreadBaseKindをRunnigにしている
            try
            {
                //[C#] C#の場合は、Start()が終了してしまうのを避けるため、OnRunThreadの中で、準備が完了してから
                OnRunThread();
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Trace.TraceInformation("stop Thread");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                if (_logger != null)
                {
                    _logger.Set(LogKind.Error, null, 1, ex.Message);
                    _logger.Exception(ex, null, 2);
                }
            }
            finally
            {
                //life = true;//Stop()でスレッドを停止する時、life=falseでループから離脱させ、このlife=trueで処理終了を認知する
                ThreadBaseKind = ThreadBaseKind.After;
            }
        }

        public abstract string GetMsg(int no);

    }
}
