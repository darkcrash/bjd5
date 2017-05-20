using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Bjd.Memory
{
    public static class MemoryExtensions
    {
        private const byte CrByte = 0x0D;
        private const byte LfByte = 0x0A;
        private static char Cr = (char)0x0D;
        private static char Lf = (char)0x0A;

        public static BufferData ToAsciiBufferData(this string text)
        {
            var ascii = System.Text.Encoding.ASCII;
            var len = ascii.GetByteCount( text);
            var buffer = BufferPool.GetMaximum(len);
            buffer.DataSize = ascii.GetBytes(text, 0, text.Length, buffer.Data, 0);
            return buffer;
        }

        public static BufferData ToUtf8BufferData(this string text)
        {
            var ascii = System.Text.Encoding.UTF8;
            var len = ascii.GetByteCount(text);
            var buffer = BufferPool.GetMaximum(len);
            buffer.DataSize = ascii.GetBytes(text, 0, text.Length, buffer.Data, 0);
            return buffer;
        }

        public static BufferData ToBufferData(this byte[] data)
        {
            var buffer = BufferPool.GetMaximum(data.Length);
            Buffer.BlockCopy(data, 0, buffer.Data, 0, data.Length);
            return buffer;
        }

        public static BufferData ToAsciiBufferData(this CharsData data)
        {
            var ascii = System.Text.Encoding.ASCII;
            var size = ascii.GetByteCount(data.Data, 0, data.DataSize);
            var buffer = BufferPool.GetMaximum(size);
            buffer.DataSize = ascii.GetBytes(data.Data, 0, data.DataSize, buffer.Data, 0);
            return buffer;
        }

        public static BufferData ToAsciiLineBufferData(this CharsData data)
        {
            var ascii = System.Text.Encoding.ASCII;
            var size = ascii.GetByteCount(data.Data, 0, data.DataSize) + 2;
            var buffer = BufferPool.GetMaximum(size);
            buffer.DataSize = ascii.GetBytes(data.Data, 0, data.DataSize, buffer.Data, 0);
            buffer.Data[buffer.DataSize++] = CrByte;
            buffer.Data[buffer.DataSize++] = LfByte;
            return buffer;
        }

        public static CharsData Copy(this CharsData source)
        {
            var len = source.Length;
            var chars = CharsPool.GetMaximum(len);
            Buffer.BlockCopy(source.Data, 0, chars.Data, chars.DataSize * 2, len * 2);
            chars.DataSize = len;
            return chars;
        }

        public static IEnumerable<CharsData> Split(this CharsData source, char separator)
        {
            var pos = 0;
            for (var i = 0; i < source.DataSize; i++)
            {
                if (source[i] == separator)
                {
                    var len = i - pos;
                    var chars = CharsPool.GetMaximum(len);
                    //if (len > 0) Buffer.BlockCopy(source.Data, pos * 2, chars.Data, chars.DataSize * 2, len * 2);
                    if (len > 0) source.CopyTo(chars, pos, len);
                    chars.DataSize = len;
                    yield return chars;
                    pos = i + 1;
                }
            }
            if (pos < source.DataSize)
            {
                var len = source.DataSize - pos;
                var chars = CharsPool.GetMaximum(len);
                //if (len > 0) Buffer.BlockCopy(source.Data, pos * 2, chars.Data, chars.DataSize * 2, len * 2);
                if (len > 0) source.CopyTo(chars, pos, len);
                chars.DataSize = len;
                yield return chars;
            }
        }

        //public static IEnumerable<ArraySegment<char>> Split(this CharsData source, char separator)
        //{
        //    var pos = 0;
        //    for (var i = 0; i < source.DataSize; i++)
        //    {
        //        if (source.Data[i] == separator)
        //        {
        //            var len = i - pos;
        //            yield return new ArraySegment<char>(source.Data, pos, len);
        //            pos = i + 1;
        //        }
        //    }
        //    if (pos < source.DataSize)
        //    {
        //        var len = source.DataSize - pos;
        //        yield return new ArraySegment<char>(source.Data, pos, len);
        //    }
        //}


        public static bool ExistsLf(this BufferData data)
        {
            for (var i = data.DataSize; i >= 0; i--)
            {
                if (data.Data[i] == Lf) return true;
            }
            return false;
        }

        public static int CountLf(this BufferData data)
        {
            var result = 0;
            var arr = data.Data;
            for (var i = 0; i < data.DataSize; i++)
            {
                if (arr[i] != LfByte) continue;
                result++;
            }
            return result;
        }

        public static void Append(this BufferData data, byte append)
        {
            data[data.DataSize++] = append;
        }


        public static CharsData ToCharsData(this BufferData data, Encoding enc)
        {
            var size = enc.GetCharCount(data.Data, 0, data.DataSize);
            var chars = CharsPool.GetMaximum(size);
            chars.DataSize = enc.GetChars(data.Data, 0, data.DataSize, chars.Data, 0);
            return chars;
        }

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

        public static string ToAsciiString(this BufferData data)
        {
            var ascii = System.Text.Encoding.ASCII;
            return ascii.GetString(data.Data, 0, data.DataSize);
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

        public static void Append(this CharsData chars, CharsData appendText)
        {
            var len = appendText.DataSize;
            Buffer.BlockCopy(appendText.Data, 0, chars.Data, chars.DataSize * 2, len * 2);
            chars.DataSize += len;
        }

        public static void Append(this CharsData chars, string appendText)
        {
            if (appendText == null) return;
            var len = appendText.Length;
            appendText.CopyTo(0, chars.Data, chars.DataSize, len);
            chars.DataSize += len;
        }

        public static void Append(this CharsData chars, char[] appendText)
        {
            var len = appendText.Length;
            //appendText.CopyTo(0, chars.Data, chars.DataSize, len);
            Buffer.BlockCopy(appendText, 0, chars.Data, chars.DataSize * 2, len * 2);
            chars.DataSize += len;
        }

        public static void Append(this CharsData chars, char appendChar)
        {
            chars.Data[chars.DataSize] = appendChar;
            chars.DataSize++;
        }

        public static void AppendLine(this CharsData chars)
        {
            var newLine = System.Environment.NewLine;
            newLine.CopyTo(0, chars.Data, chars.DataSize, newLine.Length);
            chars.DataSize += newLine.Length;
        }

        public static void AppendCrLf(this CharsData chars)
        {
            chars.Data[chars.DataSize++] = Cr;
            chars.Data[chars.DataSize++] = Lf;
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
