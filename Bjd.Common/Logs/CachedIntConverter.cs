using Bjd.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bjd.Logs
{
    public class CachedIntConverter
    {
        [ThreadStatic]
        private static Dictionary<int, string> cached;

        [ThreadStatic]
        private static Char[] CachedText;

        public static void AppendFormatString(CharsData buffer, int length, int value)
        {
            if (cached == null)
            {
                cached = new Dictionary<int, string>();
                CachedText = new char[12];
            }
            if (!cached.ContainsKey(value))
            {
                cached.Add(value, value.ToString());
            }
            var valueString = cached[value];
            var white = length - valueString.Length;
            while(white > 0)
            {
                buffer.Data[buffer.DataSize++] = ' ';
                white--;
            }
            valueString.CopyTo(0, buffer.Data, buffer.DataSize, valueString.Length);
            buffer.DataSize += valueString.Length;
        }


    }
}
