using System;


namespace Bjd.Logs
{


    // ログ１行を表現するクラス
    public static class LogMessageExtension
    {
        //コンストラクタ
        //行の文字列(\t区切り)で指定される
        public static LogMessageStruct Create(string str)
        {
            var tmp = str.Split('\t');
            if (tmp.Length != 8)
            {
                ThrowException(str); // 初期化失敗
            }
            string _nameTag = tmp[3];
            string _remoteHostname = tmp[4];
            int _messageNo = int.Parse(tmp[5]);
            string _message = tmp[6];
            string _detailInfomation = tmp[7];
            LogKind _logKind;
            DateTime _dt = DateTime.MinValue;
            long _threadId = 0;

            if (!Enum.TryParse(tmp[1], out _logKind))
            {
                ThrowException(str); // 初期化失敗
            }
            try
            {
                _dt = DateTime.Parse(tmp[0]);
                _threadId = long.Parse(tmp[2]);
                _messageNo = int.Parse(tmp[5]);
            }
            catch (Exception)
            {
                ThrowException(str); // 初期化失敗
            }
            var newMsg = new LogMessageStruct(_dt, _logKind, _nameTag, _threadId, _remoteHostname, _messageNo, _message, _detailInfomation);

            return newMsg;

        }
        public static string Dt(this LogMessageStruct msg)
        {
            var _dt = msg._dt;
            return String.Format("{0:D4}/{1:D2}/{2:D2} {3:D2}:{4:D2}:{5:D2}", msg._dt.Year, _dt.Month, _dt.Day, _dt.Hour,
                                 _dt.Minute, _dt.Second);
        }

        public static  string Kind(this LogMessageStruct msg)
        {
            return msg._logKind.ToString();
        }

        public static  String NameTag(this LogMessageStruct msg)
        {
            return msg._nameTag;
        }

        public static  string ThreadId(this LogMessageStruct msg)
        {
            return msg._threadId.ToString();
        }

        public static string RemoteHostname(this LogMessageStruct msg)
        {
            return msg._remoteHostname;
        }

        public static string MessageNo(this LogMessageStruct msg)
        {
            return string.Format("{0:D7}", msg._messageNo);
        }

        public static string Message(this LogMessageStruct msg)
        {
            return msg._message;
        }

        public static string DetailInfomation(this LogMessageStruct msg)
        {
            return msg._detailInfomation;
        }

        //文字列化
        //\t区切りで出力される
        public static string ToLogString(this LogMessageStruct msg)
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", msg.Dt(), msg.Kind(), msg.ThreadId(),
                                 msg.NameTag(), msg.RemoteHostname(), msg.MessageNo(), msg.Message(), msg._detailInfomation);
        }

        public static string ToTraceString(this LogMessageStruct msg)
        {
            return $"[{msg._dt.ToString("HH\\:mm\\:ss\\.fff")}][{LogMessageStruct._pid}][{msg._threadId.ToString().PadLeft(3)}][{msg.Kind()}] {msg.NameTag()} {msg.RemoteHostname()} {msg.MessageNo()} {msg.Message()} {msg._detailInfomation}";
        }

        //セキュリティログかどうかの確認
        public static bool IsSecure(this LogMessageStruct msg)
        {
            if (msg._logKind == LogKind.Secure)
            {
                return true;
            }
            return false;
        }

        private static void Init(this LogMessageStruct msg)
        {
            msg._dt = new DateTime(0);
            msg._logKind = LogKind.Normal;
            msg._threadId = 0;
            msg._nameTag = "UNKNOWN";
            msg._remoteHostname = "";
            msg._messageNo = 0;
            msg._message = "";
            msg._detailInfomation = "";
        }


        private static void ThrowException(String paramStr)
        {
            throw new ValidObjException(String.Format("[ValidObj] 引数が不正です。 \"{0}\"", paramStr));
        }

    }
}