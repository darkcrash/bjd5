using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd.log;
using Bjd.mail;
using Xunit;

namespace BjdTest.mail
{

    public class MailTest : IDisposable
    {
        private Mail sut = null;

        public MailTest()
        {
            sut = new Mail();
        }

        public void Dispose()
        {

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
