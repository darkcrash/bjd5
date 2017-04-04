using Bjd.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bjd.Logs
{
    public class DateTimeTextGenerator
    {
        private static Dictionary<int, string> int2ToString = new Dictionary<int, string>();
        private static Dictionary<int, string> int3ToString = new Dictionary<int, string>();

        [ThreadStatic]
        private static Char[] CachedText;
        [ThreadStatic]
        private static int hour = -1;
        [ThreadStatic]
        private static int min = -1;
        [ThreadStatic]
        private static int sec = -1;
        [ThreadStatic]
        private static int mill = -1;
        [ThreadStatic]
        private static long tick = -1;

        static DateTimeTextGenerator()
        {
            for (var i = 0; i < 100; i++)
            {
                int2ToString.Add(i, i.ToString("00"));
            }
            for (var i = 0; i < 1000; i++)
            {
                int3ToString.Add(i, i.ToString("000"));
            }
        }

        public static void AppendFormatString(CharsData buffer, ref DateTime now)
        {
            if (CachedText == null)
            {
                CachedText = new char[12];
                CachedText[2] = ':';
                CachedText[5] = ':';
                CachedText[8] = '.';
                hour = -1;
                min = -1;
                sec = -1;
                mill = -1;
                tick = -1;
            }

            if (Interlocked.Exchange(ref tick, now.Ticks) == now.Ticks)
            {
                buffer.Append(CachedText);
                return;
            }

            if (Interlocked.Exchange(ref hour, now.Hour) != now.Hour)
            {
                var hourTxt = int2ToString[now.Hour];
                hourTxt.CopyTo(0, CachedText, 0, 2);
            }
            if (Interlocked.Exchange(ref min, now.Minute) != now.Minute)
            {
                var minTxt = int2ToString[now.Minute];
                minTxt.CopyTo(0, CachedText, 3, 2);
            }
            if (Interlocked.Exchange(ref sec, now.Second) != now.Second)
            {
                var secTxt = int2ToString[now.Second];
                secTxt.CopyTo(0, CachedText, 6, 2);
            }
            if (Interlocked.Exchange(ref mill, now.Millisecond) != now.Millisecond)
            {
                var milTxt = int3ToString[now.Millisecond];
                milTxt.CopyTo(0, CachedText, 9, 3);
            }

            buffer.Append(CachedText);

        }


    }
}
