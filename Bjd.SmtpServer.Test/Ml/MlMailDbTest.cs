using Bjd.Logs;
using Bjd.Mailbox;
using Bjd.Test;
using Xunit;
using Bjd.SmtpServer;
using Bjd;
using System.IO;
using System;
using Bjd.Initialization;
using Xunit.Abstractions;

namespace Bjd.SmtpServer.Test {
    public class MlMailDbTest : IDisposable
    {
        TestService service;
        Kernel _kernel;

        public MlMailDbTest(ITestOutputHelper output)
        {
            service = TestService.CreateTestService();
            service.AddOutput(output);
            _kernel = service.Kernel;
        }

        public void Dispose()
        {
            service.Dispose();
        }


        [Fact]
        public void SaveReadTest(){
            //var tmpDir = TestUtil.GetTmpDir("$tmp");
            var tmpDir = service.GetTmpDir("$tmp");
            var logger = new Logger(_kernel);

            var mail = new Mail(_kernel);
            const string mlName = "1ban";
            var mlMailDb = new MlMailDb(_kernel, logger, tmpDir, mlName);
            mlMailDb.Remove();//もし、以前のメールが残っていたらTESTが誤動作するので、ここで消しておく

            Assert.Equal(mlMailDb.Count(), 0);
            
            const int max = 10; //試験件数10件
            //保存と、
            for (int i = 0; i < max; i++) {
                var b = mlMailDb.Save( mail);
                Assert.Equal(b,true);//保存が成功しているか
                Assert.Equal(mlMailDb.Count(), i+1);//連番がインクリメントしているか
            }
            //範囲外のメール取得でnullが返るか
            //no==1..10が取得可能
            var m = mlMailDb.Read(0);//範囲外
            Assert.Null(m);
            //範囲内
            for (int no = 1; no <= max; no++) {
                //m = mlMailDb.Read(no);
                mlMailDb.Read(no);
                Assert.NotNull(mlMailDb.Read(no));
            }
            //範囲外
            m = mlMailDb.Read(11);
            Assert.Null(m);


            //TearDown
            mlMailDb.Remove();
            mlMailDb.Dispose();
            Directory.Delete(tmpDir,true);
        }

        //コンストラクタ
        [Theory]
        [InlineData("TestDir",true,true)]//存在するフォルダを指定すると、Status=trueとなる
        [InlineData("$$$$",false,true)]  //存在しないフォルダを指定すると、フォルダが作成され、Status=trueとなる
        [InlineData("???", false,false)]  //作成できないフォルダを指定すると、Status=falseとなる
        public void CtorTest(string folder, bool exists,bool status) {
            //Testプロジェクトの下に、TEST用フォルダを作成する

            //var dir = string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), folder);
            var dir = Path.Combine(service.Kernel.Enviroment.ExecutableDirectory, folder);

            if (!exists){//存在しないフォルダをTESTする場合は、フォルダをあらかじめ削除してお
                if (Directory.Exists(dir)) {
                    Directory.Delete(dir,true);
                }
            }
            const string mlName = "2ban";
            var mlMailDb = new MlMailDb(_kernel, null, dir,mlName);//コンストラクタ
            Assert.Equal(mlMailDb.Status, status);//初期化成功
            mlMailDb.Remove();
            
            if (!exists) {//存在しないフォルダをTESTする場合は、最後にフォルダを削除しておく
                if (Directory.Exists(dir)) {
                    Directory.Delete(dir, true);
                }
            }
            mlMailDb.Dispose();
        }
    }
}
