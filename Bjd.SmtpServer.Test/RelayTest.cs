using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Configurations;
using Xunit;
using Bjd.SmtpServer;

namespace Bjd.SmtpServer.Test
{
    public class RelayTest
    {

        [Theory]
        [InlineData(0, true)] //許可リスト優先
        [InlineData(1, false)] //禁止リスト優先
        public void Orderによる制御をテストする(int order, bool isAllow)
        {
            //setUp
            var allowList = new Dat(new CtrlType[] { CtrlType.TextBox });
            allowList.Add(true, "192.168.0.0/24");
            var denyList = new Dat(new CtrlType[] { CtrlType.TextBox });
            denyList.Add(true, "192.0.0.0/8");

            var sut = new Relay(allowList, denyList, order, null);
            var expected = isAllow;
            //exercise
            var actual = sut.IsAllow(new Ip("192.168.0.1"));
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0, false)] //許可リスト優先
        [InlineData(1, false)] //禁止リスト優先
        public void リストが空の場合(int order, bool isAllow)
        {
            //setUp
            var sut = new Relay(null, null, order, null);
            var expected = isAllow;
            //exercise
            var actual = sut.IsAllow(new Ip("192.168.0.1"));
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0, true)] //許可リスト優先
        [InlineData(1, true)] //禁止リスト優先
        public void 許可リストだけの場合(int order, bool isAllow)
        {
            //setUp
            var allowList = new Dat(new CtrlType[] { CtrlType.TextBox });
            allowList.Add(true, "192.168.0.0/24");

            var sut = new Relay(allowList, null, order, null);
            var expected = isAllow;
            //exercise
            var actual = sut.IsAllow(new Ip("192.168.0.1"));
            //verify
            Assert.Equal(expected, actual);
        }

    }
}
