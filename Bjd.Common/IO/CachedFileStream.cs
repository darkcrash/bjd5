using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bjd.Common.IO
{
    public class CachedFileStream
    {
        internal static int CacheInterval = 5000;
        static ConcurrentDictionary<string, Cache> streamDic = new ConcurrentDictionary<string, Cache>(Environment.ProcessorCount, 1024);


        private CachedFileStream() { }


        static Func<string, Cache> CreateCache => _ => new Cache();
        public static CachedReadonlyStream GetFileStream(string fullPath)
        {
            //Cache value = streamDic.GetOrAdd(fullPath, _ => new Cache());
            Cache value = streamDic.GetOrAdd(fullPath, CreateCache);
            FileStream stream;
            if (value.Queue.TryDequeue(out stream))
            {
                return new CachedReadonlyStream(fullPath, stream);
            }
            return new CachedReadonlyStream(fullPath, Create(fullPath));

        }

        private static FileStream Create(string fullPath)
        {
            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 65536, FileOptions.SequentialScan);
            return stream;
        }

        internal static void Poll(string fullPath, FileStream stream)
        {
            //Cache value = streamDic.GetOrAdd(fullPath, _ => new Cache());
            Cache value = streamDic.GetOrAdd(fullPath, CreateCache);
            value.Ticks = DateTime.Now.AddMilliseconds(CacheInterval).Ticks;
            value.Queue.Enqueue(stream);
            return;


        }

        internal static void StartCleanup(CancellationToken token)
        {
            Task.Delay(CacheInterval).
                ContinueWith(_ => Cleanup(token), token);
        }
        internal static void Cleanup(CancellationToken token)
        {
            try
            {
                foreach (var item in streamDic.Keys)
                {
                    Cache value;
                    if (streamDic.TryGetValue(item, out value))
                    {
                        if (value.Ticks < DateTime.Now.Ticks)
                        {
                            FileStream stream;
                            if (value.Queue.TryDequeue(out stream))
                            {
                                stream.Dispose();
                            }
                        }
                    }
                }
            }
            finally
            {
                StartCleanup(token);
            }
        }

        private class Cache
        {
            public long Ticks;
            public ConcurrentQueue<FileStream> Queue = new ConcurrentQueue<FileStream>();
        }

    }

}
