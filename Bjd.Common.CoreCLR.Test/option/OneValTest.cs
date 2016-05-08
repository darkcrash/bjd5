using System;
using System.Collections.Generic;
using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Controls;
using Bjd.Utils;
using Xunit;


namespace Bjd.Common.Test.option
{

    public class OneValTest
    {

        [Theory]
        [InlineData(CtrlType.CheckBox, true, "true")]
        [InlineData(CtrlType.CheckBox, false, "false")]
        [InlineData(CtrlType.Int, 100, "100")]
        [InlineData(CtrlType.Int, 0, "0")]
        [InlineData(CtrlType.Int, -100, "-100")]
        [InlineData(CtrlType.File, "c:\\test.txt", "c:\\test.txt")]
        [InlineData(CtrlType.Folder, "c:\\test", "c:\\test")]
        [InlineData(CtrlType.TextBox, "abcdefg１２３", "abcdefg１２３")]
        [InlineData(CtrlType.Radio, 1, "1")]
        [InlineData(CtrlType.Radio, 5, "5")]
        //[InlineData(CtrlType.Font, null, "Microsoft Sans Serif,10,Regular")]
        [InlineData(CtrlType.Memo, "1\r\n2\r\n3\r\n", "1\t2\t3\t")]
        [InlineData(CtrlType.Memo, "123", "123")]
        [InlineData(CtrlType.Hidden, null, "0t9GC1bkpWNzg1uea3drbQ==")] //その他はA004でテストする
        [InlineData(CtrlType.AddressV4, "192.168.0.1", "192.168.0.1")]
        //[InlineData(CtrlType.Dat, new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox }), "")] // CtrlDatはTESTBOX×2で初期化されている
        [InlineData(CtrlType.Dat, null, "")] // CtrlDatはTESTBOX×2で初期化されている
        //[InlineData(CtrlType.BindAddr, new BindAddr(), "V4ONLY,INADDR_ANY,IN6ADDR_ANY_INIT")]
        [InlineData(CtrlType.BindAddr, null, "V4ONLY,INADDR_ANY,IN6ADDR_ANY_INIT")]
        //[InlineData(CtrlType.BindAddr, new BindAddr(BindStyle.V4ONLY, new Ip(InetKind.V4), new Ip(InetKind.V6)), "V4ONLY,0.0.0.0,::0")]
        [InlineData(CtrlType.ComboBox, 0, "0")]
        [InlineData(CtrlType.ComboBox, 1, "1")]
        public void デフォルト値をtoRegで取り出す(CtrlType ctrlType, Object val, String expected)
        {
            //setUp
            const bool isSecret = false;
            var sut = Assistance.CreateOneVal(ctrlType, val);
            //exercise
            var actual = sut.ToReg(isSecret);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(CtrlType.CheckBox, "True")]
        [InlineData(CtrlType.CheckBox, "False")]
        [InlineData(CtrlType.Int, "100")]
        [InlineData(CtrlType.Int, "0")]
        [InlineData(CtrlType.File, "c:\\test.txt")]
        [InlineData(CtrlType.Folder, "c:\\test")]
        [InlineData(CtrlType.TextBox, "abcdefg１２３")]
        [InlineData(CtrlType.Radio, "1")]
        [InlineData(CtrlType.Radio, "0")]
        //[InlineData(CtrlType.Font, "Times New Roman,2,Bold")]
        //[InlineData(CtrlType.Font, "ＭＳ ゴシック,1,Strikeout")]
        //[InlineData(CtrlType.Font, "Arial,1,Bold")]
        //[InlineData(CtrlType.Font, "Arial,1,Italic")]
        //[InlineData(CtrlType.Font, "Arial,1,Underline")]
        [InlineData(CtrlType.Memo, "1\t2\t3\t")]
        [InlineData(CtrlType.Hidden, "qmw+Wuj6Y3f3WlWdncmLEQ==")]
        [InlineData(CtrlType.Hidden, "Htt+6zREaQU3sc7UrnAWHQ==")]
        [InlineData(CtrlType.AddressV4, "192.168.0.1")]
        [InlineData(CtrlType.Dat, "\tn1\tn2")]
        [InlineData(CtrlType.Dat, "\tn1\tn2\b\tn1#\tn2")]
        [InlineData(CtrlType.BindAddr, "V4Only,INADDR_ANY,IN6ADDR_ANY_INIT")]
        [InlineData(CtrlType.BindAddr, "V6Only,198.168.0.1,ffe0::1")]
        [InlineData(CtrlType.ComboBox, "1")]
        public void FromRegで設定した値をtoRegで取り出す(CtrlType ctrlType, String str)
        {
            //setUp
            const bool isSecret = false;
            OneVal sut = Assistance.CreateOneVal(ctrlType, null);
            sut.FromReg(str);
            var expected = str;
            //exercise
            String actual = sut.ToReg(isSecret);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(CtrlType.CheckBox, "true", true)]
        [InlineData(CtrlType.CheckBox, "TRUE", true)]
        [InlineData(CtrlType.CheckBox, "false", true)]
        [InlineData(CtrlType.CheckBox, "FALSE", true)]
        [InlineData(CtrlType.CheckBox, "t", false)] // 不正入力
        [InlineData(CtrlType.CheckBox, "", false)] // 不正入力
        [InlineData(CtrlType.Int, "-100", true)]
        [InlineData(CtrlType.Int, "0", true)]
        [InlineData(CtrlType.Int, "aaa", false)] // 不正入力
        [InlineData(CtrlType.File, "c:\\test.txt", true)]
        [InlineData(CtrlType.Folder, "c:\\test", true)]
        [InlineData(CtrlType.TextBox, "abcdefg１２３", true)]
        [InlineData(CtrlType.Radio, "0", true)]
        [InlineData(CtrlType.Radio, "5", true)]
        [InlineData(CtrlType.Radio, "-1", false)] //不正入力 Radioは0以上
        [InlineData(CtrlType.Font, "Default,-1,1", false)] //不正入力(styleが無効値)
        [InlineData(CtrlType.Font, "Default,2,-1", false)] //不正入力(sizeが0以下)
        [InlineData(CtrlType.Font, "XXX,1,8", false)] //　C#:エラー Java:(Font名ではエラーが発生しない)
        [InlineData(CtrlType.Font, "Serif,1,-1", false)] //不正入力
        [InlineData(CtrlType.Memo, null, false)] //不正入力
        [InlineData(CtrlType.Hidden, null, false)] //不正入力
        [InlineData(CtrlType.AddressV4, null, false)] //不正入力
        [InlineData(CtrlType.AddressV4, "xxx", false)] //不正入力
        [InlineData(CtrlType.AddressV4, "1", false)] //不正入力
        [InlineData(CtrlType.Dat, "", false)] //不正入力
        [InlineData(CtrlType.Dat, null, false)] //不正入力
        [InlineData(CtrlType.Dat, "\tn1", false)] //不正入力(カラム不一致)
        [InlineData(CtrlType.BindAddr, null, false)] //不正入力
        [InlineData(CtrlType.BindAddr, "XXX", false)] //不正入力
        [InlineData(CtrlType.ComboBox, "XXX", false)] //不正入力
        [InlineData(CtrlType.ComboBox, null, false)] //不正入力
        [InlineData(CtrlType.ComboBox, "2", false)] //不正入力 list.size()オーバー
        public void FromRegの不正パラメータ判定(CtrlType ctrlType, String str, bool expected)
        {
            //setUp
            var sut = Assistance.CreateOneVal(ctrlType, null);
            //exercise
            var actual = sut.FromReg(str);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(CtrlType.Hidden, true, "123", "***")]
        [InlineData(CtrlType.Hidden, false, "123", "qmw+Wuj6Y3f3WlWdncmLEQ==")]
        [InlineData(CtrlType.Hidden, false, "", "0t9GC1bkpWNzg1uea3drbQ==")]
        [InlineData(CtrlType.Hidden, false, null, "0t9GC1bkpWNzg1uea3drbQ==")]
        [InlineData(CtrlType.Hidden, false, "本日は晴天なり", "Htt+6zREaQU3sc7UrnAWHQ==")]
        public void IsDebugTrueの時のToReg出力(CtrlType ctrlType, bool isDebug, String str, String expected)
        {
            //setUp
            OneVal sut = Assistance.CreateOneVal(ctrlType, str);
            //exercise
            String actual = sut.ToReg(isDebug);
            //verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(CtrlType.CheckBox, true)]
        [InlineData(CtrlType.Hidden, "123")]
        [InlineData(CtrlType.TextBox, "123")]
        [InlineData(CtrlType.Memo, "123\n123")]
        [InlineData(CtrlType.CheckBox, true)]
        [InlineData(CtrlType.Int, 0)]
        [InlineData(CtrlType.Folder, "c:\\test")]
        [InlineData(CtrlType.TextBox, "abcdefg１２３")]
        [InlineData(CtrlType.Radio, 1)]
        //[InlineData(CtrlType.Font, new Font("Times New Roman", Font.ITALIC, 15))]
        [InlineData(CtrlType.Memo, "1\r\n2\r\n3\r\n")]
        //[InlineData(CtrlType.AddressV4, new Ip(IpKind.V4Localhost))]
        //[InlineData(CtrlType.AddressV4, new Ip(IpKind.V6Localhost))] //追加
        //×[InlineData(CtrlType.Dat, new Dat(new CtrlType[] { CtrlType.TextBox, CtrlType.TextBox }))]
        //[InlineData(CtrlType.BindAddr, new BindAddr())]
        [InlineData(CtrlType.ComboBox, 0)]
        public void ReadCtrlFalseでデフォルトの値に戻るかどうかのテスト(CtrlType ctrlType, Object value)
        {
            //setUp
            var sut = Assistance.CreateOneVal(ctrlType, value);
            //var tabindex = 0;
            //sut.CreateCtrl(null, 0, 0, ref tabindex);
            //var b = sut.ReadCtrl(false); //isConfirm = false; 確認のみではなく、実際に読み込む
            //Assert.IsTrue(b); // readCtrl()の戻り値がfalseの場合、読み込みに失敗している
            var expected = value;
            //exercise
            var actual = sut.Value;
            //verify
            Assert.Equal(expected, actual);
        }
    }


    internal class Assistance
    {
        //OneValの生成
        //デフォルト値(nullを設定した場合、適切な値を自動でセットする)
        public static OneVal CreateOneVal(CtrlType ctrlType, Object val)
        {
            //Kernel kernel = new Kernel();
            //const string help = "help";
            //OneCtrl oneCtrl;
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
                    //oneCtrl = new CtrlFile(help, 200, new Kernel());
                    break;
                case CtrlType.Folder:
                    if (val == null)
                    {
                        val = "c:\temp";
                    }
                    //oneCtrl = new CtrlFolder(help, 200,  new Kernel());
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
                    //oneCtrl = new CtrlRadio(help, new[] { "1", "2", "3" }, 30, 3);
                    break;
                case CtrlType.Font:
                    if (val == null)
                    {
                        //val = new Font("MS ゴシック", 10f);
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
                    catch (ValidObjException ex)
                    {
                        Assert.False(true, ex.Message);

                    }
                    //oneCtrl = new CtrlBindAddr(help, list.ToArray(), list.ToArray());
                    break;
                case CtrlType.ComboBox:
                    //listを{"1","2"}で決め打ち

                    if (val == null)
                    {
                        //val =  new[] { "1", "2" };
                        val = "1";
                    }
                    //oneCtrl = new CtrlComboBox(help, new[] { "1", "2" }, 10);
                    break;
                case CtrlType.Dat:
                    //カラムはTEXTBOX×2で決め打ち
                    var listVal = new ListVal{
					    //new OneVal("name1", true, Crlf.Nextline, new CtrlCheckBox("help")),
					    //new OneVal("name2", true, Crlf.Nextline, new CtrlCheckBox("help"))
					    new OneVal("name1", true, Crlf.Nextline),
                        new OneVal("name2", true, Crlf.Nextline)
                    };

                    if (val == null)
                    {
                        //var v = new Dat(new[] { CtrlType.CheckBox, CtrlType.CheckBox });
                        var v = new Dat(listVal);
                        val = v;
                    }

                    //oneCtrl = new CtrlDat(help, listVal, 300, LangKind.Jp);
                    break;
                default:
                    throw new Exception(ctrlType.ToString());
            }
            //return new OneVal("name", val, Crlf.Nextline, oneCtrl);
            return new OneVal("name", val, Crlf.Nextline);
        }
    }
}
