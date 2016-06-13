using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bjd;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;
using Bjd.Utils;
using Xunit;
using Bjd.Services;

namespace Bjd.Test.Utils
{

    public class IniDbTest
    {

        [Theory]
        [InlineData(CtrlType.Int, 123, "INT=Basic\bname=123")]
        [InlineData(CtrlType.TextBox, "123", "STRING=Basic\bname=123")]
        [InlineData(CtrlType.ComboBox, 1, "LIST=Basic\bname=1")]
        [InlineData(CtrlType.File, "c:\\1.txt", "FILE=Basic\bname=c:\\1.txt")]
        [InlineData(CtrlType.Folder, "c:\\tmp", "FOLDER=Basic\bname=c:\\tmp")]
        [InlineData(CtrlType.CheckBox, true, "BOOL=Basic\bname=true")]
        [InlineData(CtrlType.Hidden, "123", "HIDE_STRING=Basic\bname=qmw+Wuj6Y3f3WlWdncmLEQ==")]
        [InlineData(CtrlType.Memo, "123", "MEMO=Basic\bname=123")]
        [InlineData(CtrlType.Radio, 1, "RADIO=Basic\bname=1")]
        //***[TestCase(CtrlType.AddressV4, new Ip(IpKind.V4_0), "ADDRESS_V4=Basic\bname=0.0.0.0")]
        //***[TestCase(CtrlType.ADDRESSV4,	new Ip("192.168.0.1"), "ADDRESS_V4=Basic\bname=192.168.0.1")]
        public void listVal_add_OneVal_で初期化後saveして当該設定が保存されているかどうか(CtrlType ctrlType, Object value, string expected)
        {
            using (TestService _service = TestService.CreateTestService())
            {
                //setUp
                string fileName = "iniDbTestTmp"; //テンポラリファイル名
                                                  //string progDir = new File(".").getAbsoluteFile().getParent(); //カレントディレクトリ
                string progDir = Directory.GetCurrentDirectory();
                //string path = string.Format("{0}\\{1}.ini", progDir, fileName);
                string path = Path.Combine(progDir, fileName + ".ini");
                IniDb sut = new IniDb(progDir, fileName);

                ListVal listVal = new ListVal();
                listVal.Add(Assistance.createOneVal(_service.Kernel, ctrlType, value));
                sut.Save("Basic", listVal); // nameTagは"Basic"で決め打ちされている

                //exercise
                var lines = File.ReadAllLines(path);
                string actual = lines[0];
                //verify
                Assert.Equal(expected, actual);
                //tearDown
                sut.Delete();

            }

        }

        [Theory]
        [InlineData(CtrlType.Int, "123", "INT=Basic\bname=123")]
        [InlineData(CtrlType.TextBox, "123", "STRING=Basic\bname=123")]
        [InlineData(CtrlType.ComboBox, "1", "LIST=Basic\bname=1")]
        [InlineData(CtrlType.File, "c:\\1.txt", "FILE=Basic\bname=c:\\1.txt")]
        [InlineData(CtrlType.Folder, "c:\\tmp", "FOLDER=Basic\bname=c:\\tmp")]
        [InlineData(CtrlType.CheckBox, "true", "BOOL=Basic\bname=true")]
        [InlineData(CtrlType.Hidden, "qmw+Wuj6Y3f3WlWdncmLEQ==", "HIDE_STRING=Basic\bname=qmw+Wuj6Y3f3WlWdncmLEQ==")]
        [InlineData(CtrlType.Memo, "123", "MEMO=Basic\bname=123")]
        [InlineData(CtrlType.Radio, "1", "RADIO=Basic\bname=1")]
        [InlineData(CtrlType.AddressV4, "192.168.0.1", "ADDRESS_V4=Basic\bname=192.168.0.1")]
        public void 設定ファイルにテキストでセットしてreadして当該設定が読み込めるかどうか(CtrlType ctrlType, string value, string regStr)
        {
            using (TestService _service = TestService.CreateTestService())
            {

                //setUp
                string fileName = "iniDbTestTmp"; //テンポラリファイル名
                                                  //string progDir = new File(".").getAbsoluteFile().getParent();
                string progDir = Directory.GetCurrentDirectory();
                //string path = string.Format("{0}\\{1}.ini", progDir, fileName);
                string path = Path.Combine(progDir, fileName + ".ini");


                IniDb sut = new IniDb(progDir, fileName);
                sut.Delete();

                String expected = value;

                //exercise
                List<string> lines = new List<string>();
                lines.Add(regStr);
                File.WriteAllLines(path, lines);

                ListVal listVal = new ListVal();
                listVal.Add(Assistance.createOneVal(_service.Kernel, ctrlType, null));
                sut.Read("Basic", listVal); // nameTagは"Basic"で決め打ちされている
                OneVal oneVal = listVal.Search("name");

                string actual = oneVal.ToReg(false);

                //verify
                Assert.Equal(expected, actual);

                //TearDown
                sut.Delete();
            }
        }

        [Fact]
        public void データの無いDATの保存()
        {
            using (TestService _service = TestService.CreateTestService())
            {
                //setUp
                string fileName = "iniDbTestTmp"; //テンポラリファイル名
                string progDir = Directory.GetCurrentDirectory();
                //string path = string.Format("{0}\\{1}.ini", progDir, fileName);
                string path = Path.Combine(progDir, fileName + ".ini");
                IniDb sut = new IniDb(progDir, fileName);

                ListVal listVal = new ListVal();
                var l = new ListVal();
                //l.Add(new OneVal("mimeExtension", "", Crlf.Nextline, new CtrlTextBox("Extension", 10)));
                //l.Add(new OneVal("mimeType", "", Crlf.Nextline, new CtrlTextBox("MIME Type", 50)));
                //var oneVal = new OneVal("mime", null, Crlf.Nextline, new CtrlDat("comment", l, 350, LangKind.Jp));
                l.Add(new OneVal("mimeExtension", "", Crlf.Nextline));
                l.Add(new OneVal("mimeType", "", Crlf.Nextline));
                //var oneVal = new OneVal("mime", null, Crlf.Nextline, new CtrlDat("comment", l, 350, LangKind.Jp));
                var oneVal = new OneVal("mime", new Dat(l), Crlf.Nextline);
                listVal.Add(oneVal);

                sut.Save("Basic", listVal); // nameTagは"Basic"で決め打ちされている

                //exercise
                var lines = File.ReadAllLines(path);
                string actual = lines[0];
                //verify
                Assert.Equal("DAT=Basic\bmime=", actual);
                //tearDown
                sut.Delete();
            }
        }


        //共通的に利用されるメソッド
        private static class Assistance
        {
            //OneValの生成
            //デフォルト値(nullを設定した場合、適切な値を自動でセットする)
            public static OneVal createOneVal(Kernel kernel, CtrlType ctrlType, Object val)
            {

                //Kernel kernel = new Kernel();
                //string help = "help";
                //OneCtrl oneCtrl = null;
                switch (ctrlType)
                {
                    case CtrlType.CheckBox:
                        if (val == null)
                        {
                            val = true;
                        }
                        //oneCtrl = new CtrlCheckBox(help);
                        break;
                    case CtrlType.Int:
                        if (val == null)
                        {
                            val = 1;
                        }
                        //oneCtrl = new CtrlInt(help, 3); // ３桁で決め打ち
                        break;
                    case CtrlType.File:
                        if (val == null)
                        {
                            val = "1.txt";
                        }
                        //oneCtrl = new CtrlFile(help, 200, kernel);
                        break;
                    case CtrlType.Folder:
                        if (val == null)
                        {
                            val = "c:\temp";
                        }
                        //oneCtrl = new CtrlFolder(help, 200, kernel);
                        break;
                    case CtrlType.TextBox:
                        if (val == null)
                        {
                            val = "abc";
                        }
                        //oneCtrl = new CtrlTextBox(help, 20);
                        break;
                    case CtrlType.Radio:
                        if (val == null)
                        {
                            val = 0;
                        }
                        //oneCtrl = new CtrlRadio(help, new string[]{"1", "2", "3"}, 30, 3);
                        break;
                    case CtrlType.Font:
                        if (val == null)
                        {
                            //val = new Font("MS UI Gothic", 9);
                        }
                        //oneCtrl = new CtrlFont(help, LangKind.Jp);
                        break;
                    case CtrlType.Memo:
                        if (val == null)
                        {
                            val = "1";
                        }
                        //oneCtrl = new CtrlMemo(help, 10, 10);
                        break;
                    case CtrlType.Hidden:
                        if (val == null)
                        {
                            val = "";
                        }
                        //oneCtrl = new CtrlHidden(help, 30);
                        break;
                    case CtrlType.AddressV4:
                        if (val == null)
                        {
                            val = "";
                        }
                        //oneCtrl = new CtrlAddress(help);
                        break;
                    case CtrlType.BindAddr:
                        if (val == null)
                        {
                            val = "V4ONLY,INADDR_ANY,IN6ADDR_ANY_INIT";
                        }
                        var list = new List<Ip>();
                        try
                        {
                            list.Add(new Ip(IpKind.InAddrAny));
                            list.Add(new Ip("192.168.0.1"));
                        }
                        catch (ValidObjException e)
                        {
                            Assert.True(false, e.Message);
                        }
                        //oneCtrl = new CtrlBindAddr(help, list.ToArray(), list.ToArray());
                        break;
                    case CtrlType.ComboBox:
                        //listを{"1","2"}で決め打ち

                        if (val == null)
                        {
                            val = 0;
                        }
                        //oneCtrl = new CtrlComboBox(help, new string[]{"1", "2"}, 10);
                        break;
                    case CtrlType.Dat:
                        //カラムはTEXTBOX×2で決め打ち
                        ListVal listVal = new ListVal();
                        //listVal.Add(new OneVal("name1", true, Crlf.Nextline, new CtrlCheckBox("help")));
                        //listVal.Add(new OneVal("name2", true, Crlf.Nextline, new CtrlCheckBox("help")));
                        listVal.Add(new OneVal("name1", true, Crlf.Nextline));
                        listVal.Add(new OneVal("name2", true, Crlf.Nextline));

                        if (val == null)
                        {
                            val = (Dat)new Dat(new CtrlType[] { CtrlType.CheckBox, CtrlType.CheckBox });
                        }

                        //oneCtrl = new CtrlDat(help, listVal, 300, LangKind.Jp);
                        break;
                    default:
                        // not implement.
                        throw new Exception(ctrlType.ToString());
                }
                //return new OneVal("name", val, Crlf.Nextline, oneCtrl);
                return new OneVal("name", val, Crlf.Nextline);
            }


        }
    }
}

