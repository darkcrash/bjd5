using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Bjd.Logs;
using Xunit;
using Bjd.Services;
using Xunit.Abstractions;

namespace Bjd.Test.Logs
{
    public class LogFileTest : IDisposable
    {
        //テンポラリディレクトリ名
        const string TmpDir = "LogFileTest";
        TestService service;
        TestOutputService _output;

        public LogFileTest(ITestOutputHelper output)
        {
            service = TestService.CreateTestService();
            _output = new TestOutputService(output);
        }

        public void Dispose()
        {
            _output.Dispose();
            service.Dispose();
        }

        ////テンポラリのフォルダの削除
        ////このクラスの最後に１度だけ実行される
        //// 個々のテストでは、例外終了等で完全に削除出来ないので、ここで最後にディレクトリごと削除する
        ////[TearDown]
        ////[TestFixtureTearDown]
        //public void AfterClass()
        //{
        //    var dir = TestUtil.GetTmpDir(TmpDir);
        //    Directory.Delete(dir, true);
        //}

        [Fact]
        public void ログの種類日別で予想されたパターンのファイルが２つ生成される()
        {

            const int logKind = 0; //通常ログの種類
            const string pattern = "*.????.??.??.log";

            //setUp
            //var dir = TestUtil.GetTmpPath(TmpDir);
            var dir = service.GetTmpPath(TmpDir);
            Directory.CreateDirectory(dir);
            using (var sut = new LogFile(dir, logKind, logKind, 0, true))
            {
                const int expected = 2;

                //exercise
                var actual = Directory.GetFiles(dir, pattern).Count();

                //verify
                Assert.Equal(expected, actual);

                //tearDown
                //sut.Dispose();
            }
        }

        [Fact]
        public void ログの種類月別で予想されたパターンのファイルが２つ生成される()
        {

            const int logKind = 1; //通常ログの種類
            const string pattern = "*.????.??.log";

            //setUp
            //var dir = TestUtil.GetTmpPath(TmpDir);
            var dir = service.GetTmpPath(TmpDir);
            Directory.CreateDirectory(dir);
            using (var sut = new LogFile(dir, logKind, logKind, 0, true))
            {
                const int expected = 2;

                //exercise
                var actual = Directory.GetFiles(dir, pattern).Count();

                //verify
                Assert.Equal(expected, actual);

                //tearDown
                //sut.Dispose();
            }

        }

        [Fact]
        public void ログの種類固定で予想されたパターンのファイルが２つ生成される()
        {

            const int logKind = 2; //固定ログの種類
            const string pattern = "*.Log";

            //setUp
            //var dir = TestUtil.GetTmpPath(TmpDir);
            var dir = service.GetTmpPath(TmpDir);
            Directory.CreateDirectory(dir);
            using (var sut = new LogFile(dir, logKind, logKind, 0, true))
            {
                const int expected = 2;

                //exercise
                var actual = Directory.GetFiles(dir, pattern).Count();

                //verify
                Assert.Equal(expected, actual);

                //tearDown
                //sut.Dispose();
            }

        }

        [Fact]
        public void Appendで３行ログを追加すると通常ログが4行になる()
        {

            const int logKind = 2; //固定ログの種類
            const string fileName = "BlackJumboDog.Log";

            //setUp
            //var dir = TestUtil.GetTmpPath(TmpDir);
            var dir = service.GetTmpPath(TmpDir);
            Directory.CreateDirectory(dir);
            using (var sut = new LogFile(dir, logKind, logKind, 0, true))
            {
                sut.AppendAsync(
                    new OneLog("2012/06/01 00:00:00\tDetail\t3208\tWeb-localhost:88\t127.0.0.1\t0000018\texecute\tramapater"));
                sut.AppendAsync(
                    new OneLog("2012/06/02 00:00:00\tError\t3208\tWeb-localhost:88\t127.0.0.1\t0000018\texecute\tramapater"));
                var r = sut.AppendAsync(
                    new OneLog("2012/06/03 00:00:00\tSecure\t3208\tWeb-localhost:88\t127.0.0.1\t0000018\texecute\tramapater"));
                r.Wait();
                //sut.Dispose();
            }

            const int expected = 4;

            //exercise
            var lines = File.ReadAllLines(Path.Combine(dir, fileName));
            var actual = lines.Length;

            //verify
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void Appendで３行ログを追加するとセキュアログが2行になる()
        {

            const int logKind = 2; //固定ログの種類
            const string fileName = "secure.Log";

            //setUp
            //var dir = TestUtil.GetTmpPath(TmpDir);
            var dir = service.GetTmpPath(TmpDir);
            Directory.CreateDirectory(dir);
            using (var sut = new LogFile(dir, logKind, logKind, 0, true))
            {
                sut.AppendAsync(
                    new OneLog("2012/06/01 00:00:00\tDetail\t3208\tWeb-localhost:88\t127.0.0.1\t0000018\texecute\tramapater"));
                sut.AppendAsync(
                    new OneLog("2012/06/02 00:00:00\tError\t3208\tWeb-localhost:88\t127.0.0.1\t0000018\texecute\tramapater"));
                var r = sut.AppendAsync(
                     new OneLog("2012/06/03 00:00:00\tSecure\t3208\tWeb-localhost:88\t127.0.0.1\t0000018\texecute\tramapater"));
                r.Wait();
                //sut.Dispose();
            }



            const int expected = 2;

            //exercise
            var lines = File.ReadAllLines(Path.Combine(dir, fileName));

            var actual = lines.Length;

            //verify
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void 過去7日分のログを準備して本日からsaveDaysでtailする()
        {
            //// コードページ932を先に読み込むことで、エラーを回避する
            //var encService = System.Text.CodePagesEncodingProvider.Instance;
            //encService.GetEncoding(932);

            //setUp
            //var dir = TestUtil.GetTmpPath(TmpDir);
            var dir = service.GetTmpPath(TmpDir);
            Directory.CreateDirectory(dir);
            //var path = string.Format("{0}\\BlackJumboDog.Log", dir);
            var path = Path.Combine(dir, "BlackJumboDog.Log");

            //2012/09/01~7日分のログを準備
            //最初は、保存期間指定なしで起動する
            using (var logFile = new LogFile(dir, 2, 2, 0, true))
            {
                logFile.AppendAsync(
                    new OneLog("2012/09/01 00:00:00\tDetail\t3208\tWeb-localhost:88\t127.0.0.1\t0000018\texecute\tramapater"));
                logFile.AppendAsync(
                    new OneLog("2012/09/02 00:00:00\tError\t3208\tWeb-localhost:88\t127.0.0.1\t0000018\texecute\tramapater"));
                logFile.AppendAsync(
                    new OneLog("2012/09/03 00:00:00\tSecure\t3208\tWeb-localhost:88\t127.0.0.1\t0000018\texecute\tramapater"));
                logFile.AppendAsync(
                    new OneLog("2012/09/04 00:00:00\tSecure\t3208\tWeb-localhost:88\t127.0.0.1\t0000018\texecute\tramapater"));
                logFile.AppendAsync(
                    new OneLog("2012/09/05 00:00:00\tSecure\t3208\tWeb-localhost:88\t127.0.0.1\t0000018\texecute\tramapater"));
                logFile.AppendAsync(
                    new OneLog("2012/09/06 00:00:00\tSecure\t3208\tWeb-localhost:88\t127.0.0.1\t0000018\texecute\tramapater"));
                var r = logFile.AppendAsync(
                    new OneLog("2012/09/07 00:00:00\tSecure\t3208\tWeb-localhost:88\t127.0.0.1\t0000018\texecute\tramapater"));

                r.Wait();

                //exercise
                //リフレクションを使用してprivateメソッドにアクセスする
                var cls = logFile;
                var type = cls.GetType();
                var methodInfo = type.GetMethod("Tail", BindingFlags.NonPublic | BindingFlags.Instance);

                var dt = new DateTime(2012, 9, 7);//2012.9.7に設定する
                const int saveDays = 2; //保存期間２日
                methodInfo.Invoke(cls, new object[] { path, saveDays, dt });

                //logFile.Dispose();
            }
            const int expected = 3;
            var actual = File.ReadAllLines(path).Length;

            //verify
            Assert.Equal(expected, actual);


        }
    }
}
