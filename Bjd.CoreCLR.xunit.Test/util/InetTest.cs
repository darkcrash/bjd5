using Bjd.util;
using Xunit;
using System;
using System.Text;

namespace BjdTest.util
{
    public class InetTest
    {

        //バイナリ-文字列変換
        [Theory]
        [InlineData("本日は晴天なり", "2c67e5656f30746629596a308a30")]
        [InlineData("12345", "31003200330034003500")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void GetBytesTest(string str, string byteStr)
        {
            var bytes = Inet.ToBytes(str);

            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                if (b < 16) sb.Append('0'); // 二桁になるよう0を追加
                sb.Append(Convert.ToString(b, 16));
            }
            Assert.Equal(sb.ToString(), byteStr);
        }

        //バイナリ-文字列変換
        [Theory]
        [InlineData("本日は晴天なり", "2c67e5656f30746629596a308a30")]
        [InlineData("12345", "31003200330034003500")]
        [InlineData("", "")]
        [InlineData("", null)]
        public void GetStringTest(string str, string byteStr)
        {
            if (byteStr == null)
            {
                Assert.Equal(Inet.FromBytes(null), str);
            }
            else
            {
                var length = byteStr.Length / 2;
                var bytes = new byte[length];
                int j = 0;
                for (int i = 0; i < length; i++)
                {
                    bytes[i] = Convert.ToByte(byteStr.Substring(j, 2), 16);
                    j += 2;
                }
                Assert.Equal(Inet.FromBytes(bytes), str);
            }
        }

        [Theory]
        [InlineData("1\r\n2\r\n3", 3)]
        [InlineData("1\r\n2\r\n3\r\n", 4)]
        [InlineData("1\n2\n3", 1)]
        [InlineData("", 1)]
        [InlineData("\r\n", 2)]
        public void GetLinesTest(string str, int count)
        {
            var lines = Inet.GetLines(str);
            Assert.Equal(lines.Count, count);
        }

        [Theory]
        [InlineData(new byte[] { 0x62, 0x0d, 0x0a, 0x62, 0x0d, 0x0a, 0x62 }, 3)]
        [InlineData(new byte[] { 0x62, 0x0d, 0x0a, 0x62, 0x0d, 0x0a, 0x62, 0x0d, 0x0a }, 3)]
        [InlineData(new byte[] { 0x62, 0x0d, 0x0a }, 1)]
        [InlineData(new byte[] { 0x0d, 0x0a }, 1)]
        [InlineData(new byte[] { }, 0)]
        [InlineData(null, 0)]
        public void GetLinesTest(byte[] buf, int count)
        {
            var lines = Inet.GetLines(buf);
            Assert.Equal(lines.Count, count);
        }

        [Theory]
        [InlineData("1", "1")]
        [InlineData("1\r\n", "1")]
        [InlineData("1\r", "1\r")]
        [InlineData("1\n", "1")]
        [InlineData("1\n2\n", "1\n2")]
        public void TrimCrlfTest(String str, String expanded)
        {
            Assert.Equal(Inet.TrimCrlf(str), expanded);
        }

        [Theory]
        [InlineData(new byte[] { 0x64 }, new byte[] { 0x64 })]
        [InlineData(new byte[] { 0x64, 0x0d, 0x0a }, new byte[] { 0x64 })]
        [InlineData(new byte[] { 0x64, 0x0d }, new byte[] { 0x64, 0x0d })]
        [InlineData(new byte[] { 0x64, 0x0a }, new byte[] { 0x64 })]
        [InlineData(new byte[] { 0x64, 0x0a, 0x65, 0x0a }, new byte[] { 0x64, 0x0a, 0x65 })]
        public void trimCrlf_byte配列(byte[] buf, byte[] expended)
        {
            var actual = Inet.TrimCrlf(buf);
            Assert.Equal(actual.Length, expended.Length);
            for (int i = 0; i < actual.Length; i++)
            {
                Assert.Equal(actual[i], expended[i]);
            }
        }

        [Theory]
        [InlineData("<HTML>", "&lt;HTML&gt;")]
        [InlineData("R&B", "R&amp;B")]
        [InlineData("123~", "123%7E")]
        public void サニタイズ処理(String str, String expended)
        {
            var actual = Inet.Sanitize(str);
            Assert.Equal(actual, expended);
        }

        [Theory]
        [InlineData("<HTML>", "BE-90-72-8C-11-BF-70-8F-52-50-28-A6-78-0F-8E-17")]
        [InlineData("abc", "90-01-50-98-3C-D2-4F-B0-D6-96-3F-7D-28-E1-7F-72")]
        [InlineData("", "D4-1D-8C-D9-8F-00-B2-04-E9-80-09-98-EC-F8-42-7E")]
        [InlineData(null, "")]
        public void MD5ハッシュ文字列(String str, String expended)
        {
            var actual = Inet.Md5Str(str);
            Assert.Equal(actual, expended);
        }
    }
}