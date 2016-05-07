using System;
using Bjd.net;
using Xunit;
using Bjd.SipServer;
using Bjd;

namespace SipServerTest
{
    public class JobRegisterTest : IDisposable
    {

        readonly User _user = new User();

        public JobRegisterTest()
        {
            _user.Add(new OneUser("3000", "3000xxx", new Ip("0.0.0.0")));
            _user.Add(new OneUser("3001", "3001xxx", new Ip("0.0.0.0")));
            _user.Add(new OneUser("3002", "3002xxx", new Ip("0.0.0.0")));
            _user.Add(new OneUser("3003", "3003xxx", new Ip("0.0.0.0")));

        }

        public void Dispose()
        {
        }

        [Theory]
        [InlineData()]
        public void Test()
        {


        }
    }

}
