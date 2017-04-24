using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Bjd.Common.IO
{
    public class CachedFileExists
    {
        const int CacheInterval = 1;
        const int LOCK = 1;
        const int UNLOCK = 0;
        static ConcurrentDictionary<string, Cache> existsFileDic = new ConcurrentDictionary<string, Cache>();


        static Func<string, Cache> valueFactory = _ =>
        {
            var exists = File.Exists(_);
            var now = DateTime.Now.AddSeconds(CacheInterval).Ticks;
            var newValue = new Cache();
            newValue.Exists = exists;
            newValue.Ticks = now;
            return newValue;
        };


        public static bool ExistsFile(string fullPath)
        {
            //Tuple<bool, long> value;
            //var hit = existsFileDic.TryGetValue(fullPath, out value);
            //if (hit && value.Item2 > DateTime.Now.Ticks)
            //{
            //    return value.Item1;
            //}
            //var exists = File.Exists(fullPath);
            //var now = DateTime.Now.AddSeconds(5).Ticks;
            //var newValue = Tuple.Create<bool, long>(exists, now);
            //existsFileDic.AddOrUpdate(fullPath, newValue, (a, b) => newValue);
            //return exists;

            Cache value = existsFileDic.GetOrAdd(fullPath, valueFactory);
            if (value.Ticks < DateTime.Now.Ticks)
            {
                if (Interlocked.CompareExchange(ref value.lockState, LOCK, UNLOCK) == UNLOCK)
                {
                    value.Ticks = DateTime.Now.AddSeconds(CacheInterval).Ticks;
                    value.Exists = File.Exists(fullPath);
                    Interlocked.Exchange(ref value.lockState, UNLOCK);
                }
            }
            return value.Exists;

        }

        private CachedFileExists() { }

        private class Cache
        {
            public bool Exists;
            public long Ticks;
            public int lockState = 0;
        }

    }
}
