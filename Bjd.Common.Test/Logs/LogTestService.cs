using Bjd.Logs;
using Bjd.Memory;
using Bjd.Threading;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Bjd.Test.Logs
{
    //生成時に１つのファイルをオープンしてset()で１行ずつ格納するクラス
    public class LogTestService : IDisposable,  ILogService
    {
        private ITestOutputHelper _helper;
        private readonly SequentialTaskScheduler sts = new SequentialTaskScheduler();

        public LogTestService(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        public void Append(LogMessage oneLog)
        {
            _helper.WriteLine(oneLog.ToTraceString());
        }


        public void Dispose()
        {
        }

        public void WriteLine(CharsData message)
        {
            _helper.WriteLine(message.ToString());
        }

        public void TraceInformation(CharsData message)
        {
            _helper.WriteLine(message.ToString());
        }

        public void TraceWarning(CharsData message)
        {
            _helper.WriteLine(message.ToString());
        }

        public void TraceError(CharsData message)
        {
            _helper.WriteLine(message.ToString());
        }
    }
}

