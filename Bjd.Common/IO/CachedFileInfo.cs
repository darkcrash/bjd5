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
        static ConcurrentDictionary<string, Cache> existsFileDic = new ConcurrentDictionary<string, Cache>();

        public static FileInfo GetFileInfo(string fullPath)
        {
            Cache value;
            var hit = existsFileDic.TryGetValue(fullPath, out value);
            if (hit)
            {
                if (value.Ticks < DateTime.Now.Ticks)
                {
                    value.Ticks = DateTime.Now.AddSeconds(CacheInterval).Ticks;
                    value.Info.Refresh();
                }
                return value.Info;
            }
            var info = new FileInfo(fullPath);
            var now = DateTime.Now.AddSeconds(CacheInterval).Ticks;
            var newValue = new Cache();
            newValue.Info = info;
            newValue.Ticks = now;
            existsFileDic.AddOrUpdate(fullPath, newValue, (a, b) => newValue);
            return info;
        }

        private CachedFileInfo() { }


        private class Cache
        {
            public FileInfo Info;
            public long Ticks;
        }

    }
}
