using Bjd.Memory;
using System;
using System.Text;

namespace Bjd.Logs
{


    // ログ１行を表現するクラス
    public class LogMessage : ValidObj
    {
        private DateTime _dt;
        private LogKind _logKind;
        private string _nameTag;
        private int _threadId;
        private string _remoteHostname;
        private int _messageNo;
        private string _message;
        private string _detailInfomation;
        private static int _pid = System.Diagnostics.Process.GetCurrentProcess().Id;

        public LogKind LogKind { get { return _logKind; } }

        public LogMessage(DateTime dt, LogKind logKind, string nameTag, int threadId, string remoteHostname, int messageNo, string message, string detailInfomation)
        {
            _dt = dt;
            _logKind = logKind;
            _nameTag = nameTag;
            _threadId = threadId;
            _remoteHostname = remoteHostname;
            _messageNo = messageNo;
            _message = message;
            _detailInfomation = detailInfomation;
        }

        //コンストラクタ
        //行の文字列(\t区切り)で指定される
        public LogMessage(String str)
        {
            var tmp = str.Split('\t');
            if (tmp.Length != 8)
            {
                ThrowException(str); // 初期化失敗
            }
            _nameTag = tmp[3];
            _remoteHostname = tmp[4];
            _messageNo = int.Parse(tmp[5]);
            _message = tmp[6];
            _detailInfomation = tmp[7];
            if (!Enum.TryParse(tmp[1], out _logKind))
            {
                ThrowException(str); // 初期化失敗
            }
            try
            {
                _dt = DateTime.Parse(tmp[0]);
                _threadId = int.Parse(tmp[2]);
                _messageNo = int.Parse(tmp[5]);
            }
            catch (Exception)
            {
                ThrowException(str); // 初期化失敗
            }

        }

        public string Dt()
        {
            CheckInitialise(); // 他のgetterは、これとセットで使用されるため、チェックはここだけにする
            return String.Format("{0:D4}/{1:D2}/{2:D2} {3:D2}:{4:D2}:{5:D2}", _dt.Year, _dt.Month, _dt.Day, _dt.Hour,
                                 _dt.Minute, _dt.Second);
        }

        public string Kind()
        {
            return _logKind.ToString();
        }

        public string NameTag()
        {
            return _nameTag;
        }

        public string ThreadId()
        {
            return _threadId.ToString();
        }

        public string RemoteHostname()
        {
            return _remoteHostname;
        }

        public string MessageNo()
        {
            return String.Format("{0:D7}", _messageNo);
        }

        public string Message()
        {
            return _message;
        }

        public string DetailInfomation()
        {
            return _detailInfomation;
        }


        //文字列化
        //\t区切りで出力される
        public override string ToString()
        {
            CheckInitialise();

            using (var sb = CharsPool.GetMaximum(256))
            {
                DateTimeTextGenerator.AppendFormatStringYMD(sb, ref _dt);
                sb.Append('\t');
                sb.Append(_logKind.ToString());
                sb.Append('\t');
                CachedIntConverter.AppendFormatString(sb, 1, _threadId);
                sb.Append('\t');
                sb.Append(_nameTag);
                sb.Append('\t');
                sb.Append(_remoteHostname);
                sb.Append('\t');
                CachedIntConverter.AppendFormatString(sb, 7, _messageNo, '0');
                sb.Append('\t');
                sb.Append(_message);
                sb.Append('\t');
                sb.Append(_detailInfomation);
                return sb.ToString();
            }

        }

        public CharsData GetChars()
        {
            CheckInitialise();

            var sb = CharsPool.GetMaximum(256 + _detailInfomation.Length);
            DateTimeTextGenerator.AppendFormatStringYMD(sb, ref _dt);
            sb.Append('\t');
            sb.Append(_logKind.ToString());
            sb.Append('\t');
            CachedIntConverter.AppendFormatString(sb, 1, _threadId);
            sb.Append('\t');
            sb.Append(_nameTag);
            sb.Append('\t');
            sb.Append(_remoteHostname);
            sb.Append('\t');
            CachedIntConverter.AppendFormatString(sb, 1, _messageNo);
            sb.Append('\t');
            sb.Append(_message);
            sb.Append('\t');
            sb.Append(_detailInfomation);
            return sb;
        }


        public CharsData ToTraceChars()
        {
            CheckInitialise();

            var sb = CharsPool.GetMaximum(256 + _detailInfomation.Length);
            sb.Append('[');
            DateTimeTextGenerator.AppendFormatString(sb, ref _dt);
            sb.Append("][");
            CachedIntConverter.AppendFormatString(sb, 1, _pid);
            sb.Append("][");
            CachedIntConverter.AppendFormatString(sb, 3, _threadId);
            sb.Append("][");
            sb.Append(Kind());
            sb.Append("] ");
            sb.Append(_nameTag);
            sb.Append(" ");
            sb.Append(_remoteHostname);
            sb.Append(" ");
            CachedIntConverter.AppendFormatString(sb, 7, _messageNo);
            sb.Append(" ");
            sb.Append(_message);
            sb.Append(" ");
            sb.Append(_detailInfomation);
            sb.Append(']');
            return sb;
        }

        //セキュリティログかどうかの確認
        public bool IsSecure()
        {
            CheckInitialise();
            if (_logKind == LogKind.Secure)
            {
                return true;
            }
            return false;
        }

        protected override void Init()
        {
            _dt = new DateTime(0);
            _logKind = LogKind.Normal;
            _threadId = 0;
            _nameTag = "UNKNOWN";
            _remoteHostname = "";
            _messageNo = 0;
            _message = "";
            _detailInfomation = "";
        }
    }
}