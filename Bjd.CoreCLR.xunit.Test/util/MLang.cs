using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bjd.util;
using Xunit;

namespace BjdTest.util
{

    public class MLangTest
    {

        [Fact]
        public void getEncoding及びgetstringの確認()
        {
            //setUp
            string str = "あいうえお";
            string[] charsetList = new[] { "utf-8", "euc-jp", "iso-2022-jp", "shift_jis" };

            //verify
            foreach (string charset in charsetList)
            {
                byte[] bytes = Encoding.GetEncoding(charset).GetBytes(str);
                Assert.Equal(MLang.GetEncoding(bytes).WebName, charset);
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
            Assert.Equal(actual, expected);
            //TearDown
            File.Delete(tempFile);
        }
    }
}