using System;
using Xunit;
using Bjd.SipServer;
using System.Text;

namespace SipServerTest
{
    public class StartLineTest : IDisposable
    {
        public  StartLineTest()
        {

        }

        public void Dispose()
        {
        }

        //SIPメソッドの解釈
        [Theory]
        [InlineData("SIP", SipMethod.Unknown)]//異常系
        [InlineData("xxx sip:1@1 SIP/1.0\r\n", SipMethod.Unknown)]//異常系
        [InlineData("invite sip:1@1 SIP/1.5\r\n", SipMethod.Invite)]
        [InlineData("invite sip:1@1 SIP/1.5", SipMethod.Unknown)]//異常系(改行なし)
        [InlineData("INVITE sip:UserB@there.com SIP/2.0\r\n", SipMethod.Invite)]
        public void SipMethodの解釈(string str, SipMethod sipMethod)
        {
            //setup
            var sut = new StartLine(Encoding.ASCII.GetBytes(str));
            var expected = sipMethod;
            //exercise
            var actual = sut.SipMethod;
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("SIP", "")]//異常系
        [InlineData("xxx sip:1@1 SIP/1.0\r\n", "")]//異常系
        [InlineData("invite sip:1@1 SIP/1.5\r\n", "1@1")]
        [InlineData("invite sip:1@1 SIP/1.5", "")]//異常系(改行なし)
        [InlineData("INVITE sip:UserB@there.com SIP/2.0\r\n", "UserB@there.com")]
        public void RequestUriの解釈(string str, string requestUri)
        {
            //setup
            var sut = new StartLine(Encoding.ASCII.GetBytes(str));
            var expected = requestUri;
            //exercise
            var actual = sut.RequestUri;
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("SIP", (float)0)]//異常系
        [InlineData("xxx sip:1@1 SIP/1.0\r\n", (float)0)]//異常系
        [InlineData("invite sip:1@1 SIP/1.5\r\n", (float)1.5)]
        [InlineData("invite sip:1@1 SIP/1.5", (float)0)]//異常系(改行なし)
        [InlineData("INVITE sip:UserB@there.com SIP/2.0\r\n", (float)2.0)]
        public void SipVerの解釈(string str, float no)
        {
            //setup
            var sut = new StartLine(Encoding.ASCII.GetBytes(str));
            var expected = no;
            //exercise
            var actual = sut.SipVer.No;
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("SIP/2.0 180 Ringing\r\n", 180)]
        [InlineData("SIP/2.0 200 OK\r\n", 200)]
        [InlineData("SIP/2.0 200 \r\n", 0)]//異常系
        [InlineData("SIP/2.0 200", 0)]//異常系
        public void Statusコードの解釈(string str, int statusCode)
        {
            //setup
            var sut = new StartLine(Encoding.ASCII.GetBytes(str));
            var expected = statusCode;
            //exercise
            var actual = sut.StatusCode;
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("SIP/2.0 180 Ringing\r\n", (float)2.0)]
        [InlineData("SIP/2.0 200 OK\r\n", (float)2.0)]
        [InlineData("SIP/2.0 200 \r\n", (float)0)]//異常系
        public void Sipバージョンの解釈(string str, float verNo)
        {
            //setup
            var sut = new StartLine(Encoding.ASCII.GetBytes(str));
            var expected = verNo;
            //exercise
            var actual = sut.SipVer.No;
            //verify
            Assert.Equal(expected, actual);
        }


        //ReceptionKindの判定
        [Theory]
        [InlineData("SIP/2.0 180 Ringing\r\n", ReceptionKind.Status)]
        [InlineData("SIP/2.0 200 OK\r\n", ReceptionKind.Status)]
        [InlineData("SIP/2.0 200 OK", ReceptionKind.Unknown)]//改行なし
        [InlineData("SIP/2.0 200\r\n", ReceptionKind.Unknown)]//項目不足
        [InlineData("INVITE sip SIP/1.0\r\n", ReceptionKind.Unknown)] //無効項目
        [InlineData("xxx sip:1@1 SIP/1.0\r\n", ReceptionKind.Unknown)] //無効メソッド
        [InlineData("INVITE sip:UserB@there.com \r\n", ReceptionKind.Unknown)]//項目不足
        [InlineData("invite sip:1@1 SIP/1.5\r\n", ReceptionKind.Request)]
        [InlineData("INVITE sip:UserB@there.com SIP/2.0\r\n", ReceptionKind.Request)]
        public void ReceptionKindの解釈(string str, ReceptionKind receptionKind)
        {
            //setup
            var sut = new StartLine(Encoding.ASCII.GetBytes(str));
            var expected = receptionKind;
            //exercise
            var actual = sut.ReceptionKind;
            //verify
            Assert.Equal(expected, actual);
        }


    }
}
