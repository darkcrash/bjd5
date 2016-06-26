﻿using System;
using System.Threading;
using Bjd.Logs;

namespace Bjd.Threading
{

    //スレッドの起動停止機能を持った基本クラス
    public abstract class ThreadBase : IDisposable, ILogger, ILife
    {

        Thread _t;
        private ThreadBaseKind _threadBaseKind = ThreadBaseKind.Before;

        public ThreadBaseKind ThreadBaseKind
        {
            get { return _threadBaseKind; }
            protected set { _threadBaseKind = value; }
        }
        private bool _life = false; //スレッドを停止するためのスイッチ
        readonly Logger _logger;
        protected Kernel _kernel; //SockObjのTraceのため
        private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        protected CancellationToken CancelToken { get; private set; }


        //logger　スレッド実行中に例外がスローされたとき表示するためのLogger(nullを設定可能)
        protected ThreadBase(Kernel kernel, Logger logger)
        {
            this._kernel = kernel;
            _logger = logger;

            // タスクのキャンセルにサーバー停止イベントを登録
            this.CancelToken = cancelTokenSource.Token;
            this._kernel.CancelToken.Register(this.Cancel);

        }

        protected void Cancel()
        {
            _life = false;
            this.cancelTokenSource.Cancel();
        }

        //時間を要するループがある場合、ループ条件で値がtrueであることを確認する<br>
        // falseになったら直ちにループを中断する
        public bool IsLife()
        {
            return _life;
        }

        //終了処理
        //Override可能
        public void Dispose()
        {
            Stop();
        }

        //【スレッド開始前処理】
        //falseでスレッド起動をやめる
        protected abstract bool OnStartThread();

        //開始処理
        //Override可能
        public void Start()
        {
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
                _threadBaseKind = ThreadBaseKind.Before;

                _life = true;
                _t = new Thread(Loop) { IsBackground = true };
                _t.Start();

                //スレッドが起動してステータスがRUNになるまで待機する
                while (_threadBaseKind == ThreadBaseKind.Before)
                {
                    Thread.Sleep(100);
                }
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning($"ThreadBase.Start {ex.Message}");
            }
        }

        //【スレッド終了処理】
        protected abstract void OnStopThread();

        //停止処理
        //Override可能
        public void Stop()
        {

            if (_t != null && _threadBaseKind == ThreadBaseKind.Running)
            {//起動されている場合
                //_life = false;//スイッチを切るとLoop内の無限ループからbreakする
                this.Cancel();
                while (_threadBaseKind != ThreadBaseKind.After)
                {
                    Thread.Sleep(100);//breakした時点でIsRunがfalseになるので、ループを抜けるまでここで待つ
                }
            }
            _t = null;
            OnStopThread();
        }

        protected abstract void OnRunThread();

        private void Loop()
        {
            //[Java] 現在、Javaでは、ここでThreadBaseKindをRunnigにしている
            try
            {

                //[C#] C#の場合は、Start()が終了してしまうのを避けるため、OnRunThreadの中で、準備が完了してから
                //ThreadBaseKindをRunningにする
                OnRunThread();
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Trace.TraceInformation("スレッドの中止");
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

            //life = true;//Stop()でスレッドを停止する時、life=falseでループから離脱させ、このlife=trueで処理終了を認知する
            _threadBaseKind = ThreadBaseKind.After;
        }

        public abstract string GetMsg(int no);

    }
}
