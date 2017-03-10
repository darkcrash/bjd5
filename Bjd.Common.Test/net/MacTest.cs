using Bjd.Net;
using Xunit;

namespace Bjd.Test.Net
{
    public class MacTest
    {

        [Theory]
        [InlineData("00-00-00-00-00-00")]
        [InlineData("00-26-2D-3F-3F-67")]
        [InlineData("00-ff-ff-ff-3F-67")]
        [InlineData("FF-FF-FF-FF-FF-FF")]
        public void Mac_macStr_で初期化してtoStringで確かめる(string macStr)
        {
            //setUp
            var sut = new Mac(macStr);
            var expected = macStr.ToLower();
            //exercise
            var actual = sut.ToString().ToLower();
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("12-34-56-78-9a-bc", true)]
        [InlineData("12-34-56-78-9A-BC", true)]
        [InlineData("00-26-2D-3F-3F-67", false)]
        [InlineData("00-00-00-00-00-00", false)]
        [InlineData("ff-ff-ff-ff-ff-ff", false)]
        [InlineData(null, false)]
        public void Equalのテスト12_34_56_78_9A_BCと比較する(string macStr, bool expected)
        {
            //setUp
            var sut = new Mac("12-34-56-78-9A-BC");
            Mac target = null;
            if (macStr != null)
            {
                target = new Mac(macStr);
            }
            //exercise
            bool actual = sut.Equals(target);
            //verify
            Assert.Equal(expected, actual);
        }

    }
}
