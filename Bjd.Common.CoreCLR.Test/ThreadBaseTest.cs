using System.Threading;
using Bjd;
using Xunit;
using Bjd.Services;
using System;
using Bjd.Threading;

namespace Bjd.Test
{

    public class ThreadBaseTest : IDisposable
    {
        private class MyThread : ThreadBase
        {
            public MyThread(Kernel kernel) : base(kernel, null)
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

        TestService _service;

        public ThreadBaseTest()
        {
            _service = TestService.CreateTestService();
        }

        public void Dispose()
        {
            _service.Dispose();
        }

        [Fact]
        public void Startする前はThreadBaseKindはBeforeとなる()
        {
            //setUp
            var sut = new MyThread(_service.Kernel);
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
            var sut = new MyThread(_service.Kernel);
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
            var sut = new MyThread(_service.Kernel);
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
            var sut = new MyThread(_service.Kernel);
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
            var sut = new MyThread(_service.Kernel);
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
                var sut = new MyThread(_service.Kernel);
                sut.Start();
                Assert.Equal(sut.ThreadBaseKind, ThreadBaseKind.Running);
                sut.Stop();
                Assert.Equal(sut.ThreadBaseKind, ThreadBaseKind.After);
                sut.Dispose();
            }
        }

    }

}