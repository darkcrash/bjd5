using Bjd.Threading;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bjd.Logs
{
    internal class LogStaticMembers
    {
        public static SequentialTaskScheduler TaskScheduler = new SequentialTaskScheduler();

        [ThreadStatic]
        private static StringBuilder sb;
        public static StringBuilder GetStringBuilder()
        {
            if (sb == null) sb = new StringBuilder(1024);
            sb.Clear();
            return sb;
        }
    }
}
