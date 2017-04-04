using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Bjd.Memory
{
    public static class MemoryExtensions
    {

        public static CharsData ToAsciiCharsData(this BufferData data)
        {
            var ascii = System.Text.Encoding.ASCII;
            var size = ascii.GetCharCount(data.Data, 0, data.DataSize);
            var chars = CharsPool.GetMaximum(size);
            chars.DataSize = ascii.GetChars(data.Data, 0, data.DataSize, chars.Data, 0);
            return chars;
        }

        public static CharsData ToCharsData(this StringBuilder sb)
        {
            var len = sb.Length;
            var chars = CharsPool.GetMaximum(len);
            sb.CopyTo(0, chars.Data, 0, sb.Length);
            chars.DataSize = len;
            return chars;
        }

        public static CharsData ToCharsData(this string text)
        {
            var len = text.Length;
            var chars = CharsPool.GetMaximum(len);
            text.CopyTo(0, chars.Data, chars.DataSize, len);
            chars.DataSize = len;
            return chars;
        }

        public static CharsData ToCharsData(this string[] array)
        {
            var len = array.Sum(_ => _?.Length ?? 0);
            var chars = CharsPool.GetMaximum(len);
            foreach (var msg in array)
            {
                if (msg == null) continue;
                var l = msg.Length;
                msg.CopyTo(0, chars.Data, chars.DataSize, l);
                chars.DataSize += l;
            }
            return chars;
        }

        public static void Append(this CharsData chars, string appendText)
        {
            var len = appendText.Length;
            appendText.CopyTo(0, chars.Data, chars.DataSize, len);
            chars.DataSize += len;
        }

        public static void Append(this CharsData chars, char[] appendText)
        {
            var len = appendText.Length;
            //appendText.CopyTo(0, chars.Data, chars.DataSize, len);
            Buffer.BlockCopy(appendText, 0, chars.Data, chars.DataSize, len);
            chars.DataSize += len;
        }

        public static void Append(this CharsData chars, char appendChar)
        {
            chars.Data[chars.DataSize] = appendChar;
            chars.DataSize++;
        }

        public static void AppendFormat(this CharsData chars, string appendFormatText, object param)
        {
            var appendText = string.Format(appendFormatText, param);
            var len = appendText.Length;
            appendText.CopyTo(0, chars.Data, chars.DataSize, len);
            chars.DataSize += len;
        }

    }
}
