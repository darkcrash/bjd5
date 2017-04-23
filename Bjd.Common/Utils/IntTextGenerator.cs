using Bjd.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bjd.Utils
{
    internal class IntTextGenerator
    {
        //private static Dictionary<int, string> int2ToString = new Dictionary<int, string>();
        //private static Dictionary<int, string> int3ToString = new Dictionary<int, string>();
        //private static Dictionary<int, string> int4ToString = new Dictionary<int, string>();

        private static string[] int2ToString = new string[100];
        private static string[] int3ToString = new string[1000];
        private static string[] int4ToString = new string[10000];

        static IntTextGenerator()
        {
            for (var i = 0; i < 100; i++)
            {
                int2ToString[i] = i.ToString("00");
            }
            for (var i = 0; i < 1000; i++)
            {
                int3ToString[i] = i.ToString("000");
            }
            for (var i = 0; i < 10000; i++)
            {
                int4ToString[i] = i.ToString("0000");
            }
        }

        public static string ToString00(int val)
        {
            return int2ToString[val];
        }

        public static string ToString000(int val)
        {
            return int3ToString[val];
        }

        public static string ToString0000(int val)
        {
            return int4ToString[val];
        }


    }
}
