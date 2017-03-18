using System.Threading;
using Bjd;
using Xunit;
using Bjd.Initialization;
using System;
using Bjd.Threading;
using Xunit.Abstractions;

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
                    Thread.Sleep(10);
                }
            }

            public override string GetMsg(int no)
            {
                return "";
            }
        }

        TestService _service;

        public ThreadBaseTest(ITestOutputHelper output)
        {
            _service = TestService.CreateTestService();
            _service.AddOutput(output);
        }

        public void Dispose()
        {
            _service.Dispose();
        }

        [Fact]
        public void Startする前はThreadBaseKindはBeforeとなる()
        {
            //setUp
            using (var sut = new MyThread(_service.Kernel))
            {
                //var expected = ThreadBaseKind.Before;
                //exercise
                var actual = sut.ThreadBaseKind;
                //verify
                Assert.Equal(ThreadBaseKind.Before, actual);
                //tearDown
            }
        }

        [Fact]
        public void StartするとThreadBaseKindはRunningとなる()
        {
            //setUp
            using (var sut = new MyThread(_service.Kernel))
            {
                //var expected = ThreadBaseKind.Running;
                //exercise
                sut.Start();
                var actual = sut.ThreadBaseKind;
                //verify
                Assert.Equal(ThreadBaseKind.Running, actual);
                //tearDown
            }
        }

        [Fact]
        public void Startは重複しても問題ない()
        {
            //setUp
            using (var sut = new MyThread(_service.Kernel))
            {
                //var expected = ThreadBaseKind.Running;
                //exercise
                sut.Start();
                sut.Start();
                sut.Start();
                sut.Start(); //重複
                var actual = sut.ThreadBaseKind;
                //verify
                Assert.Equal(ThreadBaseKind.Running, actual);
                //tearDown
            }
        }

        [Fact]
        public void Stopは重複しても問題ない()
        {
            //setUp
            using (var sut = new MyThread(_service.Kernel))
            {
                //var expected = ThreadBaseKind.After;
                //exercise
                sut.Stop(); //重複
                sut.Start();
                sut.Stop();
                sut.Stop();
                sut.Stop();
                sut.Stop();
                sut.Stop(); //重複
                var actual = sut.ThreadBaseKind;
                //verify
                Assert.Equal(ThreadBaseKind.After, actual);
                //tearDown
            }
        }

        [Fact]
        public void start及びstopしてisRunnigの状態を確認する_負荷テスト()
        {

            //setUp
            using (var sut = new MyThread(_service.Kernel))
            {
                //exercise verify 
                for (var i = 0; i < 20; i++)
                {
                    sut.Start();
                    Assert.Equal(ThreadBaseKind.Running, sut.ThreadBaseKind);
                    sut.Stop();
                    Assert.Equal(ThreadBaseKind.After, sut.ThreadBaseKind);
                }
                //tearDown
            }
        }

        [Fact]
        public void new及びstart_stop_disposeしてisRunnigの状態を確認する_負荷テスト()
        {
            //exercise verify 
            for (var i = 0; i < 20; i++)
            {
                var sut = new MyThread(_service.Kernel);
                Assert.Equal(ThreadBaseKind.Before, sut.ThreadBaseKind);
                sut.Start();
                Assert.Equal(ThreadBaseKind.Running, sut.ThreadBaseKind);
                sut.Stop();
                Assert.Equal(ThreadBaseKind.After, sut.ThreadBaseKind);
                sut.Dispose();
            }
        }

        [Fact]
        public void new及びstart_disposeしてisRunnigの状態を確認する_負荷テスト()
        {
            //exercise verify 
            for (var i = 0; i < 20; i++)
            {
                var sut = new MyThread(_service.Kernel);
                Assert.Equal(ThreadBaseKind.Before, sut.ThreadBaseKind);
                sut.Start();
                Assert.Equal(ThreadBaseKind.Running, sut.ThreadBaseKind);
                sut.Dispose();
                Assert.Equal(ThreadBaseKind.After, sut.ThreadBaseKind);
            }
        }


    }

}