﻿using System;
using System.IO;
using System.Linq;
using Bjd.Logs;
using Xunit;
using Bjd.Initialization;

namespace Bjd.Test.Logs
{
    public class OneLogFileTest : IDisposable
    {

        //テンポラリディレクトリ名
        private const String TmpDir = "OneLogFileTest";
        TestService service;

        public OneLogFileTest()
        {
            service = TestService.CreateTestService();
        }

        public void Dispose()
        {
            service.Dispose();
        }

        ////テンポラリのフォルダの削除
        ////このクラスの最後に１度だけ実行される
        ////個々のテストでは、例外終了等で完全に削除出来ないので、ここで最後にディレクトリごと削除する
        ////[TestFixtureTearDown]
        ////[OneTimeTearDown]
        //public static void AfterClass()
        //{
        //    var dir = TestUtil.GetTmpDir(TmpDir);
        //    Directory.Delete(dir, true);
        //}

        //public void Dispose()
        //{
        //    var dir = TestUtil.GetTmpDir(TmpDir);
        //    Directory.Delete(dir, true);
        //}

        [Fact]
        public void 一度disposeしたファイルに正常に追加できるかどうか()
        {

            //setUp
            //var fileName = TestUtil.GetTmpPath(TmpDir);
            var fileName = service.GetTmpPath(TmpDir);
            var sut = new LogFileWriter(fileName);
            sut.WriteLine("1");
            sut.WriteLine("2");
            sut.WriteLine("3");
            //いったんクローズする
            sut.Dispose();

            //同一のファイルを再度開いてさらに３行追加
            sut = new LogFileWriter(fileName);
            sut.WriteLine("4");
            sut.WriteLine("5");
            sut.WriteLine("6");
            sut.Dispose();

            const int expected = 6;

            //exercise
            var actual = File.ReadAllLines(fileName).Count();

            //verify
            Assert.Equal(expected, actual);
        }
    }
}

