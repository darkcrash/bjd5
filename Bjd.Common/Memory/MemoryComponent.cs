using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Components;
using Bjd.Utils;
using Bjd.Threading;
using Bjd.Net.Sockets;

namespace Bjd.Memory
{

    public class MemoryComponent : ComponentBase
    {
        private Logger _log;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private BufferData emptyBuffer = BufferData.Empty;
        private CharsData emptyChars = CharsData.Empty;

        public MemoryComponent(Kernel kernel, Conf conf)
            : base(kernel, conf)
        {

            // タスクのキャンセルにサーバー停止イベントを登録
            kernel.Events.Cancel += Events_Cancel;

            var intCacheInterval = (bool)conf.Get("useCleanup");

            //_log = logger;
            _log = kernel.CreateLogger("IO", (bool)conf.Get("useDetailsLog"), null);

            BufferPool._log = _log;
            CharsPool._log = _log;
            SockQueuePool._log = _log;
            SimpleResetPool._log = _log;
            SimpleAsyncAwaiterPool._log = _log;

            BufferPool.GetMaximum(0).Dispose();
            CharsPool.GetMaximum(0).Dispose();
            SockQueuePool.Instance.Get().Dispose();
            SimpleResetPool.GetResetEvent().Dispose();
            SimpleAsyncAwaiterPool.GetResetEvent().Dispose();

        }

        private void Events_Cancel(object sender, EventArgs e)
        {
            tokenSource.Cancel();
        }

        public override void Dispose()
        {
            BufferPool._log = null;
            CharsPool._log = null;
            SockQueuePool._log = null;
            SimpleResetPool._log = null;
            SimpleAsyncAwaiterPool._log = null;

            base.Dispose();
        }

    }
}
