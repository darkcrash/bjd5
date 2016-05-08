using Bjd.Sockets;

namespace Bjd.Logs
{
    public class Log
    {
        private readonly Logger _logger;
        public Log(Logger logger)
        {
            _logger = logger;
        }
        public void Set(LogKind logKind, SockObj sockBase, int messageNo, string detailInfomation)
        {
            if (_logger != null)
            {
                _logger.Set(logKind, sockBase, messageNo, detailInfomation);
            }
        }
    }
}
