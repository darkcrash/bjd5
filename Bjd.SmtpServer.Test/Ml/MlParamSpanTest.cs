using Xunit;
using Bjd.SmtpServer;

namespace Bjd.SmtpServer.Test
{
    public class MlParamSpanTest
    {


        [Theory]
        [InlineData("1-10", 30, 1, 10)]
        [InlineData("1-10", 5, 1, 5)]
        [InlineData("10-5", 30, 5, 10)]
        [InlineData("-1-5", 30, -1, -1)]//無効値
        [InlineData("last:20", 0, -1, -1)]//無効値
        [InlineData("20", 30, 20, 20)]
        [InlineData("last:5", 30, 26, 30)]
        [InlineData("lAST:5", 30, 26, 30)]
        [InlineData("first:5", 30, 1, 5)]
        [InlineData("45-60", 30, -1, -1)]
        public void CtorTest(string paramStr, int current, int start, int end)
        {
            var mlParamSpan = new MlParamSpan(paramStr, current);
            Assert.Equal(mlParamSpan.Start, start);
            Assert.Equal(mlParamSpan.End, end);

        }
    }
}
