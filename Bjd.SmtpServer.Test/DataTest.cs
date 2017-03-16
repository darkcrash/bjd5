using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Xunit;
using Bjd.SmtpServer;
using Bjd.Services;
using Xunit.Abstractions;

namespace Bjd.SmtpServer.Test
{
    public class DataTest : IDisposable
    {
        private TestService _service;
        private Kernel _kernel;

        public DataTest(ITestOutputHelper output)
        {
            _service = TestService.CreateTestService();
            _service.AddOutput(output);
            _kernel = _service.Kernel;

        }

        public void Dispose()
        {
            _service.Dispose();
        }

        [Fact]
        public void Appendでメール受信の完了時にFinishが返される()
        {
            //setUp
            const int sizeLimit = 1000;
            var sut = new Data(_kernel, sizeLimit);
            var expected = RecvStatus.Finish;
            //exercise
            var actual = sut.Append(Encoding.ASCII.GetBytes("1:1\r\n\r\n.\r\n"));//<CL><CR>.<CL><CR>
            //verify
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void Appendでドットのみの行を受信()
        {
            //setUp
            const int sizeLimit = 1000;
            var sut = new Data(_kernel, sizeLimit);
            var expected = RecvStatus.Continue;
            //exercise
            var actual = sut.Append(Encoding.ASCII.GetBytes("1:1\r\n\r\n..\r\n"));//<CL><CR>..<CL><CR>
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Appendでドットで始まる行の確認()
        {
            //setUp
            const int sizeLimit = 1000;
            var sut = new Data(_kernel, sizeLimit);
            var expected = ".htaccess\r\n";
            //exercise
            sut.Append(Encoding.ASCII.GetBytes("1:1\r\n\r\n..htaccess\r\n.\r\n"));//>.htaccess
            var lines = Inet.GetLines(sut.Mail.GetBody());
            var actual = Encoding.ASCII.GetString(lines[0]);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Appendでドットのみの行の確認()
        {
            //setUp
            const int sizeLimit = 1000;
            var sut = new Data(_kernel, sizeLimit);
            var expected = ".\r\n";
            //exercise
            sut.Append(Encoding.ASCII.GetBytes("1:1\r\n\r\n..\r\n.\r\n"));//>.htaccess
            var lines = Inet.GetLines(sut.Mail.GetBody());
            var actual = Encoding.ASCII.GetString(lines[0]);
            //verify
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void Appendでドットを含む行の受信()
        {
            //setUp
            const int sizeLimit = 1000;
            var sut = new Data(_kernel, sizeLimit);
            var expected = RecvStatus.Continue;
            //exercise
            var actual = sut.Append(Encoding.ASCII.GetBytes("123.\r\n"));//.<CL><CR>
            //verify
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void Appendでメール受信中にContinueが返される()
        {
            //setUp
            const int sizeLimit = 1000;
            var sut = new Data(_kernel, sizeLimit);
            var expected = RecvStatus.Continue;
            //exercise
            var actual = sut.Append(Encoding.ASCII.GetBytes("1:1\r\n\r\n."));
            //verify
            Assert.Equal(expected, actual);
        }
        [Theory]
        [InlineData(1, 1023, RecvStatus.Continue)] //1Kbyte制限
        [InlineData(1, 1024, RecvStatus.Limit)]//1Kbyte制限
        [InlineData(1, 1025, RecvStatus.Limit)]//1Kbyte制限
        [InlineData(0, 2048, RecvStatus.Continue)] //制限なし
        public void Appendでサイズ制限を超えるとContinueが返される(int limit, int size, RecvStatus recvStatus)
        {
            //setUp
            var sut = new Data(_kernel, limit);
            var expected = recvStatus;
            //exercise
            var actual = sut.Append(new byte[size]);
            //verify
            Assert.Equal(expected, actual);
        }
    }
}
