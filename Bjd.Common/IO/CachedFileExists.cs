using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bjd.Common.IO
{
    public class CachedFileExists
    {

        static ConcurrentDictionary<string, Tuple<bool, long>> existsFileDic = new ConcurrentDictionary<string, Tuple<bool, long>>();

        public static bool ExistsFile(string fullPath)
        {
            Tuple<bool, long> value;
            var hit = existsFileDic.TryGetValue(fullPath, out value);
            if (hit && value.Item2 > DateTime.Now.Ticks)
            {
                return value.Item1;
            }
            var exists = File.Exists(fullPath);
            var now = DateTime.Now.AddSeconds(5).Ticks;
            var newValue = Tuple.Create<bool, long>(exists, now);
            existsFileDic.AddOrUpdate(fullPath, newValue, (a, b) => newValue);
            return exists;
        }

        private CachedFileExists() { }

    }
}
