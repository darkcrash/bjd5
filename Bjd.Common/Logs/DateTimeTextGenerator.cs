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
        private static Dictionary<int, string> int4ToString = new Dictionary<int, string>();

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
            for (var i = 1900; i < 2100; i++)
            {
                int4ToString.Add(i, i.ToString("0000"));
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

            if (tick == now.Ticks)
            {
                tick = now.Ticks;
                buffer.Append(CachedText);
                return;
            }

            if (hour != now.Hour)
            {
                hour = now.Hour;
                var hourTxt = int2ToString[now.Hour];
                hourTxt.CopyTo(0, CachedText, 0, 2);
            }
            if (min != now.Minute)
            {
                min = now.Minute;
                var minTxt = int2ToString[now.Minute];
                minTxt.CopyTo(0, CachedText, 3, 2);
            }
            if (sec != now.Second)
            {
                sec = now.Second;
                var secTxt = int2ToString[now.Second];
                secTxt.CopyTo(0, CachedText, 6, 2);
            }
            if (mill != now.Millisecond)
            {
                mill = now.Millisecond;
                var milTxt = int3ToString[now.Millisecond];
                milTxt.CopyTo(0, CachedText, 9, 3);
            }

            buffer.Append(CachedText);

        }

        [ThreadStatic]
        private static Char[] CachedTextYmd;
        [ThreadStatic]
        private static int year = -1;
        [ThreadStatic]
        private static int month = -1;
        [ThreadStatic]
        private static int day = -1;


        public static void AppendFormatStringYMD(CharsData buffer, ref DateTime now)
        {
            if (CachedTextYmd == null)
            {
                CachedTextYmd = new char[19];
                CachedTextYmd[4] = '/';
                CachedTextYmd[6] = '/';
                CachedTextYmd[9] = ' ';
                CachedTextYmd[12] = ':';
                CachedTextYmd[15] = ':';
                year = -1;
                month = -1;
                day = -1;
                hour = -1;
                min = -1;
                sec = -1;
            }

            if (tick == now.Ticks)
            {
                tick = now.Ticks;
                buffer.Append(CachedTextYmd);
                return;
            }

            if (year != now.Year)
            {
                year = now.Year;
                var yearTxt = int4ToString[now.Year];
                yearTxt.CopyTo(0, CachedTextYmd, 0, 4);
            }
            if (month != now.Month)
            {
                month = now.Month;
                var monthTxt = int2ToString[now.Month];
                monthTxt.CopyTo(0, CachedTextYmd, 5, 2);
            }
            if (day != now.Day)
            {
                day = now.Day;
                var dayTxt = int2ToString[now.Day];
                dayTxt.CopyTo(0, CachedTextYmd, 8, 2);
            }
            if (hour != now.Hour)
            {
                hour = now.Hour;
                var hourTxt = int2ToString[now.Hour];
                hourTxt.CopyTo(0, CachedTextYmd, 11, 2);
            }
            if (min != now.Minute)
            {
                min = now.Minute;
                var minTxt = int2ToString[now.Minute];
                minTxt.CopyTo(0, CachedTextYmd, 14, 2);
            }
            if (sec != now.Second)
            {
                sec = now.Second;
                var secTxt = int2ToString[now.Second];
                secTxt.CopyTo(0, CachedTextYmd, 17, 2);
            }


        }

    }
}
