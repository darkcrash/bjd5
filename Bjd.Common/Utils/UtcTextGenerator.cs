using Bjd.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bjd.Utils
{
    internal class UtcTextGenerator
    {
        static string[] monthList = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        static string[] weekList = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        static UtcTextGenerator()
        {
        }

        [ThreadStatic]
        private static Char[] CachedTextYmd;
        [ThreadStatic]
        private static int week =  -1;
        [ThreadStatic]
        private static int year2 = -1;
        [ThreadStatic]
        private static int month2 = -1;
        [ThreadStatic]
        private static int day2 = -1;
        [ThreadStatic]
        private static int hour2 = -1;
        [ThreadStatic]
        private static int min2 = -1;
        [ThreadStatic]
        private static int sec2 = -1;
        [ThreadStatic]
        private static long tick2 = -1;


        public static string GetUtcTimeNow()
        {
            var now = DateTime.UtcNow;
            //var result = CharsPool.GetMaximum(29);

            if (CachedTextYmd == null)
            {
                CachedTextYmd = new char[29];
                CachedTextYmd[3] = ',';
                CachedTextYmd[4] = ' ';
                CachedTextYmd[7] = ' ';
                CachedTextYmd[11] = ' ';
                CachedTextYmd[16] = ' ';
                CachedTextYmd[19] = ':';
                CachedTextYmd[22] = ':';
                CachedTextYmd[25] = ' ';
                CachedTextYmd[26] = 'G';
                CachedTextYmd[27] = 'M';
                CachedTextYmd[28] = 'T';

                week = -1;
                year2 = -1;
                month2 = -1;
                day2 = -1;
                hour2 = -1;
                min2 = -1;
                sec2 = -1;
            }

            if (tick2 == now.Ticks)
            {
                //result.Append(CachedTextYmd);
                //return result;
                return new string(CachedTextYmd);
            }
            tick2 = now.Ticks;

            var wk = (int)now.DayOfWeek;
            if (week != wk)
            {
                week = wk;
                var weekTxt = weekList[wk];
                weekTxt.CopyTo(0, CachedTextYmd, 0, 3);
            }

            if (year2 != now.Year)
            {
                year2 = now.Year;
                var yearTxt = IntTextGenerator.ToString0000(now.Year);
                yearTxt.CopyTo(0, CachedTextYmd, 12, 4);
            }

            if (month2 != now.Month)
            {
                month2 = now.Month;
                var monthTxt = monthList[now.Month - 1];
                monthTxt.CopyTo(0, CachedTextYmd, 8, 3);
            }
            if (day2 != now.Day)
            {
                day2 = now.Day;
                var dayTxt = IntTextGenerator.ToString00(now.Day);
                dayTxt.CopyTo(0, CachedTextYmd, 5, 2);
            }
            if (hour2 != now.Hour)
            {
                hour2 = now.Hour;
                var hourTxt = IntTextGenerator.ToString00(now.Hour);
                hourTxt.CopyTo(0, CachedTextYmd, 17, 2);
            }
            if (min2 != now.Minute)
            {
                min2 = now.Minute;
                var minTxt = IntTextGenerator.ToString00(now.Minute);
                minTxt.CopyTo(0, CachedTextYmd, 20, 2);
            }
            if (sec2 != now.Second)
            {
                sec2 = now.Second;
                var secTxt = IntTextGenerator.ToString00(now.Second);
                secTxt.CopyTo(0, CachedTextYmd, 23, 2);
            }

            //result.Append(CachedTextYmd);
            //return result;
            return new string(CachedTextYmd);

        }


    }
}
