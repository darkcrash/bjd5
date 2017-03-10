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
    public class SmtpAuthRangeTest
    {

        //enableEsmtp 0:適用しない　1:適用する
        [Theory]
        [InlineData(0, "192.168.0.1", false)]
        [InlineData(1, "192.168.0.1", true)]
        [InlineData(1, "8012::1", false)]
        [InlineData(1, "::1", false)]
        public void enableEsmtpによる適用有無の確認_IpV4のみ設定(int enableEsmtp, String ipStr, bool expected)
        {
            //setUp
            var range = new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox });
            range.Add(true, "name\t192.168.0.0/24");
            var sut = new SmtpAuthRange(range, enableEsmtp, null);
            //exercise
            var actual = sut.IsHit(new Ip(ipStr));
            //verify
            Assert.Equal(expected, actual);
        }

        //enableEsmtp 0:適用しない　1:適用する
        [Theory]
        [InlineData(0, "192.168.0.1", false)]
        [InlineData(1, "192.168.0.1", true)]
        [InlineData(1, "8012::1", true)]
        [InlineData(1, "::1", false)]
        [InlineData(0, "8012::1", false)]
        [InlineData(0, "::1", true)]
        public void enableEsmtpによる適用有無の確認_IpV4及びV6を設定(int enableEsmtp, String ipStr, bool expected)
        {
            //setUp
            var range = new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox });
            range.Add(true, "name\t192.168.0.0/24");
            range.Add(true, "name\t8012::1/64");
            var sut = new SmtpAuthRange(range, enableEsmtp, null);
            //exercise
            var actual = sut.IsHit(new Ip(ipStr));
            //verify
            Assert.Equal(expected, actual);
        }
    }
}
