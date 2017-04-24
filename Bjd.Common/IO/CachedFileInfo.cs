using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Bjd.Common.IO
{
    public class CachedFileInfo
    {
        const int CacheInterval = 1;
        const int LOCK = 1;
        const int UNLOCK = 0;
        static ConcurrentDictionary<string, Cache> existsFileDic = new ConcurrentDictionary<string, Cache>();

        static Func<string, Cache> valueFactory = _ =>
        {
            var info = new FileInfo(_);
            info.Refresh();
            var now = DateTime.Now.AddSeconds(CacheInterval).Ticks;
            var newValue = new Cache();
            newValue.Info = info;
            newValue.Ticks = now;
            return newValue;
        };

        public static FileInfo GetFileInfo(string fullPath)
        {
            //Cache value;
            //var hit = existsFileDic.TryGetValue(fullPath, out value);
            //if (hit)
            //{
            //    if (value.Ticks < DateTime.Now.Ticks)
            //    {
            //        if (Interlocked.CompareExchange(ref value.lockState, LOCK, UNLOCK) == UNLOCK)
            //        {
            //            value.Ticks = DateTime.Now.AddSeconds(CacheInterval).Ticks;
            //            value.Info.Refresh();
            //            Interlocked.Exchange(ref value.lockState, UNLOCK);
            //        }
            //    }
            //    return value.Info;
            //}
            //var info = new FileInfo(fullPath);
            //info.Refresh();
            //var now = DateTime.Now.AddSeconds(CacheInterval).Ticks;
            //var newValue = new Cache();
            //newValue.Info = info;
            //newValue.Ticks = now;
            //existsFileDic.AddOrUpdate(fullPath, newValue, (a, b) => newValue);
            //return info;

            Cache value = existsFileDic.GetOrAdd(fullPath, valueFactory);
            if (value.Ticks < DateTime.Now.Ticks)
            {
                if (Interlocked.CompareExchange(ref value.lockState, LOCK, UNLOCK) == UNLOCK)
                {
                    value.Ticks = DateTime.Now.AddSeconds(CacheInterval).Ticks;
                    var info = new FileInfo(fullPath);
                    info.Refresh();
                    value.Info = info;
                    Interlocked.Exchange(ref value.lockState, UNLOCK);
                }
            }
            return value.Info;

        }

        private CachedFileInfo() { }


        private class Cache
        {
            public FileInfo Info;
            public long Ticks;
            public int lockState = 0;
        }

    }
}
