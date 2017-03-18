using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd.Logs;
using Bjd.Mailbox;
using Xunit;
using Bjd.Initialization;
using Xunit.Abstractions;

namespace Bjd.Test.Mails
{

    public class MailTest : IDisposable
    {
        private Mail sut = null;
        private TestService _service;
        private Kernel _kernel;

        public MailTest(ITestOutputHelper output)
        {
            _service = TestService.CreateTestService();
            _service.AddOutput(output);
            _kernel = _service.Kernel;

            sut = new Mail(_kernel);

        }

        public void Dispose()
        {
            sut.Dispose();
            _service.Dispose();
        }

        [Fact]
        public void AddHeaderによるヘッダの追加()
        {
            //setUp
            const string val = "value1";
            const string tag = "tag";

            var expected = val;

            //exerceise
            sut.AddHeader(tag, val);
            var actual = sut.GetHeader(tag);

            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConvertHeaderによるヘッダの変換()
        {
            //setUp
            const string val = "value1";
            const string tag = "tag";
            var expected = "val2";

            sut.AddHeader(tag, val);

            //exerceise
            sut.ConvertHeader(tag, expected);
            var actual = sut.GetHeader(tag);

            //verify
            Assert.Equal(expected, actual);
        }

        //TODO まだ、全部のテストを実装できていない

    }
}
