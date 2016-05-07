using System;
using System.Linq;
using Bjd.mail;
using Xunit;
using System.IO;
using System.Reflection;

namespace BjdTest.mail
{

    public class MailInfoTest : IDisposable
    {

        private string _dfFile;

        public MailInfoTest()
        {
            //const string srcDir = "C:\\tmp2\\bjd5\\BJDTest";
            const string srcDir = ".";
            //テンポラリテストデータの準備
            //ファイルの内容が変更されるので、テンポラリファイルで作業する
            var src = "DF_MailInfoTest.dat";
            _dfFile = string.Format("{0}\\$$$", srcDir);
            File.Copy(src, _dfFile);
        }

        public void Dispose()
        {
            //テンポラリテストデータの削除
            File.Delete(_dfFile);//テンポラリ削除
        }

        [Theory]
        [InlineData("Date", "Sat, 28 Apr 2012 14:16:34 +0900")]
        [InlineData("From", "sin@comco.ne.jp")]
        [InlineData("Host", "win7-201108")]
        //[InlineData("Name", "MailInfoTest.dat")]
        [InlineData("RetryCounter", "0")]
        [InlineData("Size", "310")]
        [InlineData("To", "user1@example.com")]
        [InlineData("Uid", "bjd.00634712193942765633.000")]
        [InlineData("Addr", "127.0.0.1")]
        public void プロパティによる値取得(string tag, string expected)
        {
            //setUp
            var sut = new MailInfo(_dfFile);
            //exercise
            var actual = sut.GetType().GetProperty(tag).GetValue(sut, null).ToString();
            //verify
            Assert.Equal(actual, expected);
        }


        [Theory]
        [InlineData(0, true)]
        [InlineData(100, true)]
        public void IsProcessにより処理対象かどうかを判断する(double threadSpan, bool expected)
        {
            //setUp
            var sut = new MailInfo(_dfFile);
            //exercise
            var actual = sut.IsProcess(threadSpan, _dfFile);
            //verify
            Assert.Equal(actual, expected);
        }


        [Fact]
        public void Saveによる保存()
        {

            //setUp
            var tmpFile = Path.GetTempFileName();
            var sut = new MailInfo(_dfFile);
            //exercise
            sut.Save(tmpFile);

            //verify
            var src = File.ReadAllLines(_dfFile);
            var dst = File.ReadAllLines(tmpFile);
            Assert.Equal(src.Count(), dst.Count());
            for (int i = 0; i < src.Count(); i++)
            {
                Assert.Equal(src[i], dst[i]);
            }

            //tearDown
            File.Delete(tmpFile);
        }

        [Fact]
        public void ToStringによる文字列化()
        {
            //setUp
            var sut = new MailInfo(_dfFile);
            var expected = "from:sin@comco.ne.jp to:user1@example.com size:310 uid:bjd.00634712193942765633.000";
            //exercise
            var actual = sut.ToString();
            //verify
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void パラメータ指定によるコンストラクタの動作確認()
        {
            //setUp
            var a = new MailInfo(_dfFile);
            //var sut = new MailInfo(a.Uid, a.Size, a.Host, a.Addr, a.Date,a.From, a.To);
            var sut = new MailInfo(a.Uid, a.Size, a.Host, a.Addr, a.From, a.To);
            var expected = "from:sin@comco.ne.jp to:user1@example.com size:310 uid:bjd.00634712193942765633.000";
            //exercise
            var actual = sut.ToString();
            //verify
            Assert.Equal(actual, expected);
        }

    }

}
