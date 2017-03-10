using System;
using System.Collections.Generic;
using System.Text;
using Bjd.Controls;
using Bjd.Configurations;
using Bjd.Utils;
using Xunit;
using Bjd.Services;

namespace Bjd.Test.Options
{
    //テストでは、リソースの開放（dispose）を省略する
    public class ListValTest : IDisposable
    {
        private TestService _service;
        private Kernel _kernel;

        public ListValTest()
        {
            _service = TestService.CreateTestService();
            _kernel = _service.Kernel;
        }


        public void Dispose()
        {
            _service.Dispose();
        }


        //テスト用のListVal作成(パターン１)
        private ListVal CreateListVal1()
        {

            var listVal = new ListVal();
            //listVal.Add(new OneVal("n1", 1, Crlf.Nextline, new CtrlInt("help", 10)));
            //listVal.Add(new OneVal("n2", 1, Crlf.Nextline, new CtrlInt("help", 10)));
            listVal.Add(new OneVal(_kernel, CtrlType.Int, "n1", 1, Crlf.Nextline));
            listVal.Add(new OneVal(_kernel, CtrlType.Int, "n2", 1, Crlf.Nextline));

            var datList = new ListVal();
            //datList.Add(new OneVal("n3", 1, Crlf.Nextline, new CtrlInt("help", 10)));
            //datList.Add(new OneVal("n4", 1, Crlf.Nextline, new CtrlInt("help", 10)));
            //listVal.Add(new OneVal("n5", 1, Crlf.Nextline, new CtrlDat("help", datList, 10, LangKind.Jp)));
            datList.Add(new OneVal(_kernel, CtrlType.Int, "n3", 1, Crlf.Nextline));
            datList.Add(new OneVal(_kernel, CtrlType.Int, "n4", 1, Crlf.Nextline));
            listVal.Add(new OneVal(_kernel, CtrlType.Dat, "n5", new Dat(datList), Crlf.Nextline));


            datList = new ListVal();
            //datList.Add(new OneVal("n6", 1, Crlf.Nextline, new CtrlInt("help", 10)));
            //datList.Add(new OneVal("n7", 1, Crlf.Nextline, new CtrlInt("help", 10)));
            //listVal.Add(new OneVal("n8", 1, Crlf.Nextline, new CtrlDat("help", datList, 10, LangKind.Jp)));
            datList.Add(new OneVal(_kernel, CtrlType.Int, "n6", 1, Crlf.Nextline));
            datList.Add(new OneVal(_kernel, CtrlType.Int, "n7", 1, Crlf.Nextline));
            listVal.Add(new OneVal(_kernel, CtrlType.Dat, "n8", new Dat(datList), Crlf.Nextline));


            return listVal;
        }

        //テスト用のListVal作成(パターン２)
        private ListVal CreateListVal2()
        {

            var listVal = new ListVal();

            var pageList = new List<OnePage>();

            var onePage = new OnePage("page1", "ページ１");
            //onePage.Add(new OneVal("n0", 1, Crlf.Nextline, new CtrlInt("help", 10)));
            listVal.Add(new OneVal(_kernel, CtrlType.Int, "n0", 1, Crlf.Nextline));
            pageList.Add(onePage);

            onePage = new OnePage("page2", "ページ２");
            //onePage.Add(new OneVal("n1", 1, Crlf.Nextline, new CtrlInt("help", 10)));
            listVal.Add(new OneVal(_kernel, CtrlType.Int, "n1", 1, Crlf.Nextline));
            pageList.Add(onePage);

            //listVal.Add(new OneVal("n2", null, Crlf.Nextline, new CtrlTabPage("help", pageList)));
            listVal.Add(new OneVal(_kernel, CtrlType.TabPage, "n2", null, Crlf.Nextline));
            return listVal;
        }

        //listValを名前一覧（文字列）に変換する
        private String ArrayToString(IEnumerable<OneVal> list)
        {
            var sb = new StringBuilder();
            foreach (var o in list)
            {
                sb.Append(o.Name);
                sb.Append(",");
            }
            return sb.ToString();
        }

        [Fact]
        public void パターン１で作成したListValをgetListで取得する()
        {
            //setUp
            var sut = CreateListVal1();
            const string expected = "n1,n2,n3,n4,n5,n6,n7,n8,";

            //exercise
            var actual = ArrayToString(sut.GetList(null));

            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void パターン２で作成したListValをgetListで取得する()
        {
            //setUp
            var sut = CreateListVal2();
            const string expected = "n0,n1,n2,";

            //exercise
            var actual = ArrayToString(sut.GetList(null));

            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 存在するデータを検査するとnull以外が返る()
        {
            //setUp
            var sut = CreateListVal1();

            //exercise
            var actual = sut.Search("n1");

            //verify
            Assert.NotNull(actual);
        }

        [Fact]
        public void 存在しないデータを検査するとnullが返る()
        {
            //setUp
            var sut = CreateListVal1();

            //exercise
            var actual = sut.Search("xxx");

            //verify
            Assert.Null(actual);
        }

    }
}
