using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Bjd.Common.IO
{
    public class CachedFileStream
    {
        const int CacheInterval = 5;
        static ConcurrentDictionary<string, Cache> streamDic = new ConcurrentDictionary<string, Cache>(Environment.ProcessorCount, 1024);

        private CachedFileStream() { }

        public static CachedReadonlyStream GetFileStream(string fullPath)
        {
            Cache value = streamDic.GetOrAdd(fullPath, _ => new Cache());
            FileStream stream;
            if (value.Queue.TryDequeue(out stream))
            {
                return new CachedReadonlyStream(fullPath, stream);
            }
            return new CachedReadonlyStream(fullPath, Create(fullPath));

        }

        private static FileStream Create(string fullPath)
        {
            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 4096, FileOptions.SequentialScan);
            return stream;
        }

        internal static void Poll(string fullPath, FileStream stream)
        {
            Cache value = streamDic.GetOrAdd(fullPath, _ => new Cache());
            value.Ticks = DateTime.Now.AddSeconds(CacheInterval).Ticks;
            value.Queue.Enqueue(stream);
            return;


        }

        private static void Cleanup()
        {

        }

        private class Cache
        {
            public long Ticks;
            public ConcurrentQueue<FileStream> Queue = new ConcurrentQueue<FileStream>();
        }

    }

}
