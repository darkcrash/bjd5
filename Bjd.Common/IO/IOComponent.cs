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

namespace Bjd.Common.IO
{

    public class IOComponent : ComponentBase
    {
        private Logger _log;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        public IOComponent(Kernel kernel, Conf conf)
            : base(kernel, conf)
        {
            CachedFileStream.Cleanup(tokenSource.Token);

            // タスクのキャンセルにサーバー停止イベントを登録
            kernel.Events.Cancel += Events_Cancel;

            var intCacheInterval = (int)conf.Get("cacheInterval");
            CachedFileStream.CacheInterval = intCacheInterval;

            //_log = logger;
            _log = kernel.CreateLogger("IO", (bool)conf.Get("useDetailsLog"), null);

        }

        private void Events_Cancel(object sender, EventArgs e)
        {
            tokenSource.Cancel();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

    }
}
