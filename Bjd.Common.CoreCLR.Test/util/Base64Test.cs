using Bjd.Utils;
using Xunit;

namespace Bjd.Common.Test.util
{

    public class Base64Test
    {

        [Theory]
        [InlineData("本日は晴天なり", "本日は晴天なり")]
        [InlineData("123", "123")]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("1\r\n2", "1\r\n2")]
        public void Base64のエンコード及びデコード(string str, string expected)
        {
            //exercise
            string actual = Base64.Decode(Base64.Encode(str));
            //verify
            Assert.Equal(expected, actual);
        }
    }
}

