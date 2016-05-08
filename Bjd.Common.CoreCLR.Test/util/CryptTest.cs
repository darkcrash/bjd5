using System;
using Bjd.Utils;
using Xunit;

namespace Bjd.Common.Test.util
{

    public class CryptTest
    {


        [Theory]
        [InlineData("本日は晴天なり")]
        [InlineData("123")]
        [InlineData("xxxx")]
        [InlineData("1\r\n2")]
        public void Encrypt及びDecrypt(string str)
        {
            //setUp
            var expected = str;
            //exercise
            var actual = Crypt.Decrypt(Crypt.Encrypt(str));
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        public void Encryptの例外テスト(string str)
        {
            try
            {
                Crypt.Encrypt(str);
                Assert.True(false, "この行が実行されたらエラー");
            }
            catch (Exception)
            {
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("123")]
        [InlineData("本日は晴天なり")]
        public void Decryptの例外テスト(string str)
        {
            try
            {
                Crypt.Decrypt(str);
                Assert.True(false, "この行が実行されたらエラー");
            }
            catch (Exception)
            {
            }
        }
    }
}