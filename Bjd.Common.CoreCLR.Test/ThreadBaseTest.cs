using System.Threading;
using Bjd;
using Xunit;

namespace Bjd.Common.Test
{

    public class ThreadBaseTest
    {
        private class MyThread : ThreadBase
        {
            public MyThread() : base(null)
            {

            }

            protected override bool OnStartThread()
            {
                return true;
            }

            protected override void OnStopThread()
            {
            }

            protected override void OnRunThread()
            {
                //[C#]
                ThreadBaseKind = ThreadBaseKind.Running;

                while (IsLife())
                {
                    Thread.Sleep(100);
                }
            }

            public override string GetMsg(int no)
            {
                return "";
            }
        }

        [Fact]
        public void Startする前はThreadBaseKindはBeforeとなる()
        {
            //setUp
            var sut = new MyThread();
            //var expected = ThreadBaseKind.Before;
            //exercise
            var actual = sut.ThreadBaseKind;
            //verify
            Assert.Equal(actual, ThreadBaseKind.Before);
            //tearDown
            sut.Dispose();
        }

        [Fact]
        public void StartするとThreadBaseKindはRunningとなる()
        {
            //setUp
            var sut = new MyThread();
            //var expected = ThreadBaseKind.Running;
            //exercise
            sut.Start();
            var actual = sut.ThreadBaseKind;
            //verify
            Assert.Equal(actual, ThreadBaseKind.Running);
            //tearDown
            sut.Dispose();
        }

        [Fact]
        public void Startは重複しても問題ない()
        {
            //setUp
            var sut = new MyThread();
            //var expected = ThreadBaseKind.Running;
            //exercise
            sut.Start();
            sut.Start(); //重複
            var actual = sut.ThreadBaseKind;
            //verify
            Assert.Equal(actual, ThreadBaseKind.Running);
            //tearDown
            sut.Dispose();
        }

        [Fact]
        public void Stopは重複しても問題ない()
        {
            //setUp
            var sut = new MyThread();
            //var expected = ThreadBaseKind.After;
            //exercise
            sut.Stop(); //重複
            sut.Start();
            sut.Stop();
            sut.Stop(); //重複
            var actual = sut.ThreadBaseKind;
            //verify
            Assert.Equal(actual, ThreadBaseKind.After);
            //tearDown
            sut.Dispose();
        }

        [Fact]
        public void start及びstopしてisRunnigの状態を確認する_負荷テスト()
        {

            //setUp
            var sut = new MyThread();
            //exercise verify 
            for (var i = 0; i < 5; i++)
            {
                sut.Start();
                Assert.Equal(sut.ThreadBaseKind, ThreadBaseKind.Running);
                sut.Stop();
                Assert.Equal(sut.ThreadBaseKind, ThreadBaseKind.After);
            }
            //tearDown
            sut.Dispose();
        }

        [Fact]
        public void new及びstart_stop_disposeしてisRunnigの状態を確認する_負荷テスト()
        {

            //exercise verify 
            for (var i = 0; i < 3; i++)
            {
                var sut = new MyThread();
                sut.Start();
                Assert.Equal(sut.ThreadBaseKind, ThreadBaseKind.Running);
                sut.Stop();
                Assert.Equal(sut.ThreadBaseKind, ThreadBaseKind.After);
                sut.Dispose();
            }
        }

    }

}