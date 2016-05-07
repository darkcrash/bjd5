using System;
using Bjd.ctrl;
using Bjd.option;
using Xunit;

namespace BjdTest.option
{

    public class DatTest
    {

        [Theory]
        [InlineData(2, "#\tn1\tn2\b\tn1\tn2")]
        [InlineData(1, "\tn1\b\tn1\b#\tn1\b#\tn1")]
        public void FromRegで初期化してtoRegで取り出す(int colMax, String str)
        {
            //setUp
            var ctrlTypeList = new CtrlType[colMax];
            for (var i = 0; i < colMax; i++)
            {
                ctrlTypeList[i] = CtrlType.Int;
            }
            var sut = new Dat(new CtrlType[colMax]);
            var expected = str;
            sut.FromReg(str);
            //exercise
            var actual = sut.ToReg(false);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(3, "#\tn1\tn2\b\tn1\tn2")] //カラム数不一致		
        [InlineData(1, "#\tn1\b\tn1\tn2")] //カラム数不一致		
        [InlineData(1, "_\tn1")] //矛盾データ		
        [InlineData(1, "\b")] //矛盾データ		
        [InlineData(1, "")]
        [InlineData(1, null)]
        public void FromRegに無効な文字列を与えるとfalseが返る(int colMax, String str)
        {
            //setUp
            var sut = new Dat(new CtrlType[colMax]);
            const bool expected = false;
            //exercise
            var actual = sut.FromReg(str);
            //verify
            Assert.Equal(expected, actual);
        }

    }
}
