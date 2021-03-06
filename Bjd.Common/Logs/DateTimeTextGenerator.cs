﻿using Bjd.Memory;
using Bjd.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bjd.Logs
{
    public class DateTimeTextGenerator
    {
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
                buffer.Append(CachedText);
                return;
            }
            tick = now.Ticks;

            if (hour != now.Hour)
            {
                hour = now.Hour;
                var hourTxt = IntTextGenerator.ToString00(now.Hour);
                hourTxt.CopyTo(0, CachedText, 0, 2);
            }
            if (min != now.Minute)
            {
                min = now.Minute;
                var minTxt = IntTextGenerator.ToString00(now.Minute);
                minTxt.CopyTo(0, CachedText, 3, 2);
            }
            if (sec != now.Second)
            {
                sec = now.Second;
                var secTxt = IntTextGenerator.ToString00(now.Second);
                secTxt.CopyTo(0, CachedText, 6, 2);
            }
            if (mill != now.Millisecond)
            {
                mill = now.Millisecond;
                var milTxt = IntTextGenerator.ToString000(now.Millisecond);
                milTxt.CopyTo(0, CachedText, 9, 3);
            }

            buffer.Append(CachedText);

        }

        [ThreadStatic]
        private static Char[] CachedTextYmd;
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


        public static void AppendFormatStringYMD(CharsData buffer, ref DateTime now)
        {
            if (CachedTextYmd == null)
            {
                CachedTextYmd = new char[19];
                CachedTextYmd[4] = '/';
                CachedTextYmd[7] = '/';
                CachedTextYmd[10] = ' ';
                CachedTextYmd[13] = ':';
                CachedTextYmd[16] = ':';
                year2 = -1;
                month2 = -1;
                day2 = -1;
                hour2 = -1;
                min2 = -1;
                sec2 = -1;
            }

            if (tick2 == now.Ticks)
            {
                buffer.Append(CachedTextYmd);
                return;
            }
            tick2 = now.Ticks;

            if (year2 != now.Year)
            {
                year2 = now.Year;
                var yearTxt = IntTextGenerator.ToString0000(now.Year);
                yearTxt.CopyTo(0, CachedTextYmd, 0, 4);
            }

            if (month2 != now.Month)
            {
                month2 = now.Month;
                var monthTxt = IntTextGenerator.ToString00(now.Month);
                monthTxt.CopyTo(0, CachedTextYmd, 5, 2);
            }
            if (day2 != now.Day)
            {
                day2 = now.Day;
                var dayTxt = IntTextGenerator.ToString00(now.Day);
                dayTxt.CopyTo(0, CachedTextYmd, 8, 2);
            }
            if (hour2 != now.Hour)
            {
                hour2 = now.Hour;
                var hourTxt = IntTextGenerator.ToString00(now.Hour);
                hourTxt.CopyTo(0, CachedTextYmd, 11, 2);
            }
            if (min2 != now.Minute)
            {
                min2 = now.Minute;
                var minTxt = IntTextGenerator.ToString00(now.Minute);
                minTxt.CopyTo(0, CachedTextYmd, 14, 2);
            }
            if (sec2 != now.Second)
            {
                sec2 = now.Second;
                var secTxt = IntTextGenerator.ToString00(now.Second);
                secTxt.CopyTo(0, CachedTextYmd, 17, 2);
            }

            buffer.Append(CachedTextYmd);

        }

    }
}
