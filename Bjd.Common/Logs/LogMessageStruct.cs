using System;


namespace Bjd.Logs
{


    // ログ１行を表現するクラス
    public struct LogMessageStruct
    {

        internal DateTime _dt;
        internal LogKind _logKind;
        internal string _nameTag;
        internal long _threadId;
        internal string _remoteHostname;
        internal int _messageNo;
        internal string _message;
        internal string _detailInfomation;
        internal static int _pid = System.Diagnostics.Process.GetCurrentProcess().Id;

        public LogMessageStruct(DateTime dt, LogKind logKind, String nameTag, long threadId, String remoteHostname, int messageNo, string message, string detailInfomation)
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

    }
}