using Bjd.Net;
using Xunit;
using Bjd;

namespace Bjd.Common.Test
{

    public class MacTest
    {

        //[SetUp]
        public void SetUp()
        {
        }

        //[TearDown]
        public void TearDown()
        {

        }

        [Theory]
        [InlineData("00-00-00-00-00-00")]
        [InlineData("FF-FF-FF-FF-FF-FF")]
        [InlineData("00-26-2D-3F-3F-67")]
        [InlineData("00-ff-ff-ff-3F-67")]
        public void ToStringTest(string macStr)
        {
            var target = new Mac(macStr);
            Assert.Equal(target.ToString(), macStr.ToUpper());
        }

        [Theory]
        [InlineData("00-00-00-00-00-00")]
        [InlineData("FF-FF-FF-FF-FF-FF")]
        [InlineData("00-26-2D-3F-3F-67")]
        [InlineData("00-ff-ff-ff-3F-67")]
        public void OperandTest(string macStr)
        {
            const string dmy = "11-11-11-11-11-11";
            Assert.Equal(new Mac(macStr) == new Mac(macStr), true);
            Assert.Equal(new Mac(macStr) != new Mac(macStr), false);
            Assert.Equal(new Mac(dmy) == new Mac(macStr), false);
            Assert.Equal(new Mac(dmy) != new Mac(macStr), true);
            Assert.Equal(new Mac(macStr) == null, false);
            Assert.Equal(new Mac(macStr) != null, true);
        }

    }
}
