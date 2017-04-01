using System;
using System.Collections.Generic;
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

    }
}
