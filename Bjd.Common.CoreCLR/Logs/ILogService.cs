using System;
using System.Threading.Tasks;

namespace Bjd.Logs
{
    public interface ILogService : IDisposable
    {
        void Append(LogMessage oneLog);

        void WriteLine(string message);

        void TraceInformation(string message);

        void TraceWarning(string message);

        void TraceError(string message);

    }
}
