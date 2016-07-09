using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Bjd.Utils;
using Bjd.Threading;
using System.Threading.Tasks;

namespace Bjd.Logs
{
    public class LogFile : IDisposable
    {

        private readonly SequentialTaskScheduler sts = new SequentialTaskScheduler();
        private readonly String _saveDirectory;
        private readonly int _normalLogKind;
        private readonly int _secureLogKind;
        private readonly int _saveDays;
        //Ver6.0.7
        private readonly bool _useLogFile;
        private bool isDisposed = false;
        private object _lock = new object();
        private object _lockCount = new object();
        private System.Threading.CountdownEvent count = new System.Threading.CountdownEvent(0);


        private OneLogFile _normalLog; // 通常ログ
        private OneLogFile _secureLog; // セキュアログ

        private DateTime _dt; //インスタンス生成時に初期化し、日付が変化したかどうかの確認に使用する
        private DateTime _lastDelete = new DateTime(0);
        //private readonly System.Timers.Timer _timer;
        private System.Threading.Timer _timer;

        //保存ディレクトリとファイル名の種類を指定する。例外が発生した場合は、事後のappend()等は、すべて失敗する
        //saveDirectory 保存ディレクトリ
        //normalFileKind　通常ログのファイルル名の種類
        //secureFileKind　セキュリティログのファイルル名の種類
        //saveDays ログの自動削除で残す日数　0を指定した場合、自動削除は行わない
        public LogFile(String saveDirectory, int normalLogKind, int secureLogKind, int saveDays, bool useLogFile)
        {
            _saveDirectory = saveDirectory;
            _normalLogKind = normalLogKind;
            _secureLogKind = secureLogKind;
            _saveDays = saveDays;
            //Ver6.0.7
            _useLogFile = useLogFile;

            if (!Directory.Exists(saveDirectory))
            {
                throw new IOException(string.Format("directory not found. \"{0}\"", saveDirectory));
            }

            //logOpenで例外が発生した場合も、タイマーは起動しない
            LogOpen();

            // 5分に１回のインターバルタイマ
            //_timer = new System.Timers.Timer { Interval = 1000 * 60 * 5 };
            _timer = new System.Threading.Timer(this.TimerElapsed, null, 0, 1000 * 60 * 5);
            //_timer.Elapsed += TimerElapsed;
            //_timer.Enabled = true;
        }

        //ログファイルへの追加
        //oneLog 保存するログ（１行）
        //return 失敗した場合はfalseが返される
        public void Append(OneLog oneLog)
        {
            //コンストラクタで初期化に失敗している場合、falseを返す
            if (_timer == null)
            {
                return;
            }

            lock (_lockCount)
            {
                if (count.IsSet)
                {
                    count.Reset(1);
                }
                else
                {
                    count.AddCount();
                }
            }
            Action a = () =>
            {
                try
                {
                    // セキュリティログは、表示制限に関係なく書き込む
                    if (_secureLog != null && oneLog.IsSecure())
                    {
                        _secureLog.Set(oneLog.ToString());
                    }
                    // 通常ログの場合
                    if (_normalLog != null)
                    {
                        // ルール適用除外　もしくは　表示対象になっている場合
                        _normalLog.Set(oneLog.ToString());
                    }
                }
                catch (IOException)
                {
                }
            };

            var t = new Task(a, TaskCreationOptions.PreferFairness);
            t.ContinueWith(_ => { lock (_lockCount) count.Signal(); });
            t.Start(sts);

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
                    _normalLog = new OneLogFile(fileName);
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
                    _secureLog = new OneLogFile(fileName);
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
            count.Wait();

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
                    catch (Exception)
                    {

                    }
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
                    catch (Exception)
                    {

                    }
                }
            }
        }


        private void Tail(String fileName, int saveDays, DateTime now)
        {
            var lines = new List<string>();
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                    //sr.Close();
                }
                //fs.Close();
            }
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                //using (var sw = new StreamWriter(fs, Encoding.GetEncoding(932)))
                using (var sw = new StreamWriter(fs, CodePagesEncodingProvider.Instance.GetEncoding(932)))
                {
                    foreach (string str in lines)
                    {
                        sw.WriteLine(str);
                    }
                    sw.Flush();
                    //sw.Close();
                }
                //fs.Close();
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

            lock (_lock)
            {
                if (isDisposed) return;
                LogClose(); //クローズ
                LogDelete(); //過去ログの自動削除
                LogOpen(); //オープン
                _lastDelete = now;
            }
        }


        //終了処理
        //過去ログの自動削除が行われる
        public void Dispose()
        {
            lock (_lock)
            {
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
                count.Dispose();
            }
        }

    }
}

