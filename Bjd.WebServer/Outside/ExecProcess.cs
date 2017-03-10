﻿using Bjd.WebServer.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.WebServer.Outside
{
    class ExecProcess
    {
        readonly ProcessStartInfo _info;

        Process _p;
        WebStream _inputStream;
        readonly WebStream _outputStream = new WebStream(-1);
        bool _finish;//inputデータの終了フラグ
        Task readThread;
        Task writeThread;


        public ExecProcess(string cmd, string param, string dir, Env env)
        {

            _finish = false;
            readThread = new Task(ReadThread, TaskCreationOptions.LongRunning);
            writeThread = new Task(WriteThread, TaskCreationOptions.LongRunning);

            _info = new ProcessStartInfo
            {
                FileName = cmd,
                Arguments = param,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = dir
            };
            //コマンド名
            //パラメータ
            //子プロセスのウィンドウを表示しない。
            // StandardInput を使用する場合は、UseShellExecute が false になっている必要がある
            // 標準入力を使用する
            // 標準出力を使用する
            //標準エラー出力を使用する


            if (env != null)
            { //環境変数
                //Ver5.9.6
                //_info.EnvironmentVariables.Clear();

                foreach (var e in env)
                {
                    //Ver5.9.6
                    //if (_info.EnvironmentVariables.ContainsKey(e.Key)){
                    //    _info.EnvironmentVariables.Remove(e.Key);
                    //}
                    //_info.EnvironmentVariables.Add(e.Key, e.Val);
                    if (_info.Environment.ContainsKey(e.Key))
                    {
                        _info.Environment.Remove(e.Key);
                    }
                    _info.Environment.Add(e.Key, e.Val);
                }
            }
        }

        public bool Start(WebStream inputStream, out WebStream outputStream)
        {
            _inputStream = inputStream;

            _p = new Process
            {
                StartInfo = _info
            };
            _p.Start();
            //StartThread();// 標準入出力のRead/Writeスレッド起動
            readThread.Start();
            writeThread.Start();
            writeThread.Wait();
            //while (!_finish)
            //{
            //    Thread.Sleep(10);
            //}
            _p.WaitForExit();//終了待ち
            readThread.Wait();

            var ret = true;
            if (_p.ExitCode != 0)
            {
                //標準エラー出力からのデータ取得
                var errStr = _p.StandardError.ReadToEnd();
                //if (0 <= target.CgiCmd.ToUpper().IndexOf("PHP")) {
                //    //PHPの場合、エラーでの標準出力がある場合がある
                //    if (output.Length != 0) {
                //        return true;
                //    }
                //}
                if (_outputStream.Length == 0)
                {
                    _outputStream.Add(Encoding.ASCII.GetBytes(errStr));
                }
                ret = false;
            }
            outputStream = _outputStream;
            Trace.TraceInformation($"ExecProcess: outputLength{outputStream.Length}");
            return ret;
        }

        //outputをファイル化するスレッド
        void ReadThread()
        {
            const int max = 6553500;
            var tmp = new byte[max];
            while (true)
            {
                var len = _p.StandardOutput.BaseStream.Read(tmp, 0, max);
                if (len <= 0)
                {
                    if (_finish)
                        break;
                    Thread.Sleep(5);
                }
                _outputStream.Add2(tmp, 0, len);
            }
        }

        //inputにデータを送るスレッド
        void WriteThread()
        {
            if (_inputStream != null)
            {

                var sw = _p.StandardInput.BaseStream;
                var buf = new byte[6553500];
                while (true)
                {
                    var len = _inputStream.Read(buf, 0, buf.Length);
                    if (len <= 0)
                    {
                        break;
                    }
                    sw.Write(buf, 0, len);
                    //Thread.Sleep(1);
                }
                _p.StandardInput.Flush();
                //_p.StandardInput.Close();
                _p.StandardInput.Dispose();
                //Thread.Sleep(0);
            }
            _finish = true;
        }

        void StartThread()
        {


            //var readThread = new ThreadStart(ReadThread);
            //var writeThread = new ThreadStart(WriteThread);
            //var rThread = new Thread(readThread);
            //var wThread = new Thread(writeThread);
            //rThread.Name = "ReadThread";
            //wThread.Name = "WriteThread";
            //rThread.Start();
            //wThread.Start();

            //wThread.Join();
            //rThread.Join();
        }

    }
}
