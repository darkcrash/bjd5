using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bjd.Utils;
using Xunit;

namespace Bjd.Common.Test.util
{

    public class MLangTest
    {

        [Fact]
        public void getEncoding及びgetstringの確認()
        {
            //setUp
            string str = "あいうえお";
            string[] charsetList = new[] { "utf-8", "euc-jp", "iso-2022-jp", "shift_jis" };
            //var charsetList = new[] { 65001, 51932, 50220, 932 };

            //verify
            foreach (var charset in charsetList)
            {
                // fix coreclr
                //byte[] bytes = Encoding.GetEncoding(charset).GetBytes(str);
                Encoding enc;
                if ( charset == "utf-8")
                    enc = Encoding.UTF8;
                else
                    enc = CodePagesEncodingProvider.Instance.GetEncoding(charset);
                byte[] bytes = enc.GetBytes(str);
                Assert.Equal(MLang.GetEncoding(bytes).WebName, charset.ToString());
                Assert.Equal(MLang.GetString(bytes), str);
            }
        }

        [Fact]
        public void getEncoding_fileName_の確認()
        {

            //setUp
            string tempFile = Path.GetTempFileName();
            //File tempFile = File.createTempFile("tmp", ".txt");
            List<string> lines = new List<string>();
            lines.Add("あいうえお");
            File.WriteAllLines(tempFile, lines);

            Encoding sut = MLang.GetEncoding(tempFile);
            string expected = "utf-8";
            //exercise
            string actual = sut.WebName;
            //verify
            Assert.Equal(expected, actual);
            //TearDown
            File.Delete(tempFile);
        }
    }
}