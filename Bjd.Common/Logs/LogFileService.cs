using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Bjd.Utils;
using Bjd.Threading;
using System.Threading.Tasks;
using System.Threading;
using Bjd.Memory;

namespace Bjd.Logs
{
    public class LogFileService : IDisposable, ILogService
    {

        private readonly SequentialTaskScheduler sts = LogStaticMembers.TaskScheduler;
        private readonly String _saveDirectory;
        private readonly int _normalLogKind;
        private readonly int _secureLogKind;
        private readonly int _saveDays;
        //Ver6.0.7
        private readonly bool _useLogFile;
        private bool isDisposed = false;

        private SimpleResetEvent emptySignal = SimpleResetPool.GetResetEvent(true);
        private SimpleResetEvent tailSignal = SimpleResetPool.GetResetEvent(true);

        private LogFileWriter _normalLog; // 通常ログ
        private LogFileWriter _secureLog; // セキュアログ

        private DateTime _dt; //インスタンス生成時に初期化し、日付が変化したかどうかの確認に使用する
        private DateTime _lastDelete = new DateTime(0);
        //private readonly System.Timers.Timer _timer;
        private System.Threading.Timer _timer;

        //保存ディレクトリとファイル名の種類を指定する。例外が発生した場合は、事後のappend()等は、すべて失敗する
        //saveDirectory 保存ディレクトリ
        //normalFileKind　通常ログのファイルル名の種類
        //secureFileKind　セキュリティログのファイルル名の種類
        //saveDays ログの自動削除で残す日数　0を指定した場合、自動削除は行わない
        public LogFileService(string saveDirectory, int normalLogKind, int secureLogKind, int saveDays, bool useLogFile)
        {
            _saveDirectory = saveDirectory;
            _normalLogKind = normalLogKind;
            _secureLogKind = secureLogKind;
            _saveDays = saveDays;
            _useLogFile = useLogFile;

            if (!Directory.Exists(saveDirectory))
            {
                throw new IOException(string.Format("directory not found. \"{0}\"", saveDirectory));
            }

            //logOpenで例外が発生した場合も、タイマーは起動しない
            LogOpen();

            // 5分に１回のインターバルタイマ
            _timer = new System.Threading.Timer(this.TimerElapsed, null, 0, 1000 * 60 * 5);
        }

        public void Append(CharsData logChars, LogMessage oneLog)
        {
            if (isDisposed) return;
            try
            {

                tailSignal.Wait();
                emptySignal.Reset();


                var isSecureLog = _secureLog != null;
                var isNormalLog = _normalLog != null;
                if (isSecureLog || isNormalLog)
                {
                    // セキュリティログは、表示制限に関係なく書き込む
                    if (isSecureLog && oneLog.IsSecure())
                    {
                        //_secureLog.WriteLine(oneLog.ToString());
                        _secureLog.WriteLine(logChars);
                    }
                    // 通常ログの場合
                    if (isNormalLog)
                    {
                        // ルール適用除外　もしくは　表示対象になっている場合
                        //_normalLog.WriteLine(oneLog.ToString());
                        _normalLog.WriteLine(logChars);
                    }
                }

            }
            catch (IOException) { }
            finally
            {
                emptySignal.Set();
            }
        }


        public void TraceAppend(CharsData logChars, LogMessage oneLog)
        {
        }

        private void LogOpen()
        {
            // ログファイルオープン
            _dt = DateTime.Now;// 現在時間で初期化される

            string fileName = "";

            switch (_normalLogKind)
            {
                case 0: // bjd.yyyy.mm.dd.log
                    fileName = string.Format("bjd.{0:D4}.{1:D2}.{2:D2}.log", _dt.Year, _dt.Month, _dt.Day);
                    break;
                case 1: // bjd.yyyy.mm.log
                    fileName = string.Format("bjd.{0:D4}.{1:D2}.log", _dt.Year, _dt.Month);
                    break;
                case 2: // BlackJumboDog.Log
                    fileName = "BlackJumboDog.Log";
                    break;
                default:
                    Util.RuntimeException(string.Format("nomalLogKind={0}", _normalLogKind));
                    break;
            }
            fileName = System.IO.Path.Combine(_saveDirectory, fileName);
            try
            {
                //Ver6.0.7
                if (_useLogFile)
                {
                    _normalLog = new LogFileWriter(fileName);
                }
                else
                {
                    _normalLog = null;
                }
            }
            catch (IOException)
            {
                _normalLog = null;
                throw new IOException(string.Format("file open error. \"{0}\"", fileName));
            }

            switch (_secureLogKind)
            {
                case 0: // secure.yyyy.mm.dd.log
                    fileName = string.Format("secure.{0:D4}.{1:D2}.{2:D2}.log", _dt.Year, _dt.Month, _dt.Day);
                    break;
                case 1: // secure.yyyy.mm.log
                    fileName = string.Format("secure.{0:D4}.{1:D2}.log", _dt.Year, _dt.Month);
                    break;
                case 2: // secure.Log
                    fileName = "secure.Log";
                    break;
                default:
                    Util.RuntimeException(string.Format("secureLogKind={0}", _secureLogKind));
                    break;
            }
            fileName = System.IO.Path.Combine(_saveDirectory, fileName);
            try
            {
                //Ver6.0.7
                if (_useLogFile)
                {
                    _secureLog = new LogFileWriter(fileName);
                }
                else
                {
                    _secureLog = null;
                }
            }
            catch (IOException)
            {
                _secureLog = null;
                throw new IOException(string.Format("file open error. \"{0}\"", fileName));
            }
        }

        //オープンしているログファイルを全てクローズする
        private void LogClose()
        {

            if (_normalLog != null)
            {
                _normalLog.Dispose();
                _normalLog = null;
            }
            if (_secureLog != null)
            {
                _secureLog.Dispose();
                _secureLog = null;
            }

        }
        //過去ログの自動削除
        private void LogDelete()
        {

            // 0を指定した場合、自動削除を処理しない
            if (_saveDays == 0)
            {
                return;
            }

            LogClose();

            // ログディレクトリの検索
            // ログディレクトリの検索
            var di = new DirectoryInfo(_saveDirectory);
            // 一定
            var allLog = di.GetFiles("BlackJumboDog.Log").Union(di.GetFiles("secure.Log"));
            foreach (var f in allLog)
            {
                Tail(f.FullName, _saveDays, DateTime.Now); //saveDays日分以外を削除
            }
            // 日ごと
            var dayLog = di.GetFiles("bjd.????.??.??.Log").Union(di.GetFiles("secure.????.??.??.Log"));
            foreach (var f in dayLog)
            {
                var tmp = f.Name.Split('.');
                if (tmp.Length == 5)
                {
                    try
                    {
                        int year = Convert.ToInt32(tmp[1]);
                        int month = Convert.ToInt32(tmp[2]);
                        int day = Convert.ToInt32(tmp[3]);

                        DeleteLog(year, month, day, _saveDays, f.FullName);
                    }
                    catch (Exception) { }
                }
            }

            // 月ごと
            var monLog = di.GetFiles("bjd.????.??.Log").Union(di.GetFiles("secure.????.??.Log"));
            foreach (var f in monLog)
            {
                var tmp = f.Name.Split('.');
                if (tmp.Length == 4)
                {
                    try
                    {
                        var year = Convert.ToInt32(tmp[1]);
                        var month = Convert.ToInt32(tmp[2]);
                        const int day = 30;

                        DeleteLog(year, month, day, _saveDays, f.FullName);
                    }
                    catch (Exception) { }
                }
            }
        }


        private void Tail(String fileName, int saveDays, DateTime now)
        {

            var lines = new List<string>();
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536))
            {
                //using (var sr = new StreamReader(fs, Encoding.GetEncoding(932)))
                using (var sr = new StreamReader(fs, CodePagesEncodingProvider.Instance.GetEncoding(932)))
                {
                    var isNeed = false;
                    while (true)
                    {
                        string str = sr.ReadLine();
                        if (str == null)
                            break;
                        if (isNeed)
                        {
                            lines.Add(str);
                        }
                        else
                        {
                            var tmp = str.Split('\t');
                            if (tmp.Length > 1)
                            {
                                //Ver5.9.8 日付Parseの例外排除
                                try
                                {
                                    var targetDt = Convert.ToDateTime(tmp[0]);
                                    if (now.Ticks < targetDt.AddDays(saveDays).Ticks)
                                    {
                                        isNeed = true;
                                        lines.Add(str);
                                    }
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                }
            }
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 65536))
            {
                //using (var sw = new StreamWriter(fs, Encoding.GetEncoding(932)))
                using (var sw = new StreamWriter(fs, CodePagesEncodingProvider.Instance.GetEncoding(932)))
                {
                    foreach (string str in lines)
                    {
                        sw.WriteLine(str);
                    }
                    sw.Flush();
                }
            }

        }

        // 指定日以前のログファイルを削除する
        private void DeleteLog(int year, int month, int day, int saveDays, String fullName)
        {
            var targetDt = new DateTime(year, month, day);
            if (_dt.Ticks > targetDt.AddDays(saveDays).Ticks)
            {
                File.Delete(fullName);
            }
        }

        //インターバルタイマ
        private void TimerElapsed(object state)
        {
            DateTime now = DateTime.Now;

            //日付が変わっている場合は、ファイルを初期化する
            if (_lastDelete.Ticks != 0 && _lastDelete.Day == now.Day)
                return;

            if (isDisposed) return;

            tailSignal.Reset();
            try
            {
                emptySignal.Wait();
                LogClose(); //クローズ
                LogDelete(); //過去ログの自動削除
                LogOpen(); //オープン
                _lastDelete = now;
            }
            finally
            {
                tailSignal.Set();
            }
        }


        //終了処理
        //過去ログの自動削除が行われる
        public void Dispose()
        {
            var log = new LogMessage(DateTime.Now, LogKind.Secure, "Log", 0, "local", 0, "LogFile.Dispose()", "last mesasage");
            using (var chars = log.GetChars())
            {
                Append(chars, log);
            }

            emptySignal.Wait();
            isDisposed = true;

            if (_timer != null)
            {
                _timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                _timer.Dispose();
                //_timer.Stop();
                //_timer.Close();
                _timer = null;
            }
            LogClose();
            LogDelete(); // 過去ログの自動削除
            emptySignal.Dispose();
            tailSignal.Dispose();
        }

        public void WriteLine(CharsData message)
        {
        }

        public void TraceInformation(CharsData message)
        {
        }

        public void TraceWarning(CharsData message)
        {
        }

        public void TraceError(CharsData message)
        {
        }
    }
}

