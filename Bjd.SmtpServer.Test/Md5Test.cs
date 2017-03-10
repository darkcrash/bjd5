using Xunit;
using Bjd.SmtpServer;

namespace Bjd.SmtpServer.Test
{

    public class Md5Test
    {
        [Theory]
        [InlineData("password", "solt", "f6a4e260a28ece018b556fb5336d0e34")]
        [InlineData("", "", "74e6f7298a9c2d168935f58c001bad88")]
        [InlineData("", "###", "59b38243e644af8be7cb910cb8739608")]
        [InlineData("$$$", "", "363408996ea1e1c3fcd88a88ae639f7c")]
        public void HashStrTest(string passStr, string timestampStr, string hashStr)
        {
            var s = Md5.Hash(passStr, timestampStr);
            Assert.Equal(s, hashStr);
        }
    }
}
