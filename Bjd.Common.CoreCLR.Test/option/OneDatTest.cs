using System;
using Bjd.Option;
using Xunit;



namespace Bjd.Common.Test.option
{

    public class OneDatTest
    {

        private static readonly String[] StrList = new[] { "user1", "pass" };
        private static readonly bool[] IsSecretlList = new[] { true, false };

        [Theory]
        [InlineData(false, "\tuser1\tpass")]
        [InlineData(true, "\t***\tpass")]
        public void IsSecretの違いによるToRegの確認Enableがtrueの場合(bool isSecret, string expected)
        {
            //setUp
            var enable = true; //Enable=TRUE
            var sut = new OneDat(enable, StrList, IsSecretlList);
            //exercise
            var actual = sut.ToReg(isSecret);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(false, "#\tuser1\tpass")]
        [InlineData(true, "#\t***\tpass")]
        public void IsSecretの違いによるToRegの確認Enableがfalseの場合(bool isSecret, string expected)
        {
            //setUp
            var enable = false; //Enable=FALSE
            var sut = new OneDat(enable, StrList, IsSecretlList);
            //exercise
            var actual = sut.ToReg(isSecret);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(2, "\tuser1\tpass")]
        [InlineData(2, "#\tuser1\tpass")]
        [InlineData(3, "\tn1\tn2\tn3")]
        public void FromRegで初期化してToRegで出力する(int max, String str)
        {
            //setUp
            var sut = new OneDat(true, new String[max], new bool[max]);
            sut.FromReg(str);
            var expected = str;
            //exercise
            var actual = sut.ToReg(false);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(3, "\tuser1\tpass")] //カラム数宇一致
        [InlineData(2, null)]
        [InlineData(3, "_\tn1\tn2\tn3")] //無効文字列
        [InlineData(3, "")] //無効文字列
        [InlineData(3, "\t")] //無効文字列
        public void FromRegに無効な入力があった時falseが帰る(int max, String str)
        {
            //setUp
            var sut = new OneDat(true, new String[max], new bool[max]);
            var expected = false;
            //exercise
            var actual = sut.FromReg(str);
            //verify
            Assert.Equal(expected, actual);
        }

    }
}
