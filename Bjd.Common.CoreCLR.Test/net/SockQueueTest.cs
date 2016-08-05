using Bjd.Net.Sockets;
using System;
using System.Threading.Tasks;
using Xunit;


namespace Bjd.Test.Net
{


    public class SockQueueTest
    {
        const int waitTimeout = 10;

        [Fact]
        public void 生成時のlengthは0になる()
        {
            //setUp
            using (var sut = new SockQueue())
            {
                const int expected = 0;
                //exercise
                var actual = sut.Length;
                //verify
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Lengthが0の時Dequeueで100バイト取得しても0バイトしか返らない()
        {
            //setUp
            using (var sut = new SockQueue())
            {
                const int expected = 0;
                //exercise
                var actual = sut.Dequeue(100).Length;
                //verify
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Lengthが50の時Dequeueで100バイト取得しても50バイトしか返らない()
        {
            //setUp
            using (var sut = new SockQueue())
            {
                sut.Enqueue(new byte[50], 50);
                const int expected = 50;
                //exercise
                var actual = sut.Dequeue(100).Length;
                //verify
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Lengthが200の時Dequeueで100バイト取得すると100バイト返る()
        {
            //setUp
            using (var sut = new SockQueue())
            {
                sut.Enqueue(new byte[200], 200);
                const int expected = 100;
                //exercise
                var actual = sut.Dequeue(100).Length;
                //verify
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Lengthが200の時Dequeueで100バイト取得すると残りは100バイトになる()
        {
            //setUp
            using (var sut = new SockQueue())
            {
                sut.Enqueue(new byte[200], 200);
                sut.Dequeue(100);
                const int expected = 100;
                //exercise
                var actual = sut.Length;
                //verify
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(0, 50, 0)]
        [InlineData(0, 0, 50)]
        [InlineData(50, 50, 50)]
        [InlineData(50, 50, 100)]
        [InlineData(50, 100, 50)]
        [InlineData(2000000, 2000000, 2000000)]
        [InlineData(0, 2000001, 2000001)]
        public void EnqueueDequeueWait(int expected, int writeLength, int dequeueLength)
        {
            //setUp
            using (var sut = new SockQueue())
            {
                var cancel = new System.Threading.CancellationTokenSource();

                sut.Enqueue(new byte[writeLength], writeLength);

                //exercise
                var actual = sut.DequeueWait(dequeueLength, 10, cancel.Token).Length;

                //verify
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(0, 50, 0)]
        [InlineData(0, 0, 50)]
        [InlineData(50, 50, 50)]
        [InlineData(50, 50, 100)]
        [InlineData(50, 100, 50)]
        [InlineData(2000000, 2000000, 2000000)]
        [InlineData(0, 2000001, 2000001)]
        public void NotifyWriteDequeueWait(int expected, int writeLength, int dequeueLength)
        {
            //setUp
            using (var sut = new SockQueue())
            {
                var cancel = new System.Threading.CancellationTokenSource();
                var seg = sut.GetWriteSegment();

                var tWait = Task.Delay(5);
                tWait.ContinueWith(_ => sut.NotifyWrite(writeLength));

                //exercise
                var actual = sut.DequeueWait(dequeueLength, waitTimeout + 2000, cancel.Token).Length;

                //verify
                Assert.Equal(expected, actual);
            }
        }


        [Fact]
        public void EnqueueしたデータとDequeueしたデータの整合性を確認する()
        {
            //setUp
            using (var sut = new SockQueue())
            {
                var expected = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                sut.Enqueue(expected, 10);
                //exercise
                var actual = sut.Dequeue(10);
                //verify
                Assert.Equal(expected, actual);
            }
        }


        [Fact]
        public void Enqueueしたデータの一部をDequeueしたデータの整合性を確認する()
        {
            //setUp
            using (var sut = new SockQueue())
            {
                var buf = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                sut.Enqueue(buf, 10);
                sut.Dequeue(5); //最初に5バイト取得
                var expected = new byte[] { 5, 6, 7, 8, 9 };
                //exercise
                var actual = sut.Dequeue(5);
                //verify
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Enqueueしたデータの一部をDequeueしたデータの整合性を確認する分割()
        {
            //setUp
            using (var sut = new SockQueue())
            {
                var buf = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                sut.Enqueue(buf, 10);
                sut.Enqueue(buf, 10);

                var actual1 = sut.Dequeue(5); //最初に5バイト取得
                var actual2 = sut.Dequeue(5);
                var actual3 = sut.Dequeue(5);
                var actual4 = sut.Dequeue(5);
                var expected1 = new byte[] { 0, 1, 2, 3, 4 };
                var expected2 = new byte[] { 5, 6, 7, 8, 9 };


                Assert.Equal(expected1, actual1);
                Assert.Equal(expected2, actual2);
                Assert.Equal(expected1, actual3);
                Assert.Equal(expected2, actual4);
            }
        }

        [Theory]
        [InlineData(-1, -1)]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        public void Enqueueしたデータの一部をDequeueしたデータの整合性を確認する境界値(int val, int excepted)
        {
            //setUp
            using (var sut = new SockQueue())
            {
                var max = sut.Max;
                var buf = new byte[max];
                buf[0] = byte.MaxValue;
                buf[1] = byte.MinValue;
                buf[max - 1] = byte.MinValue;

                sut.Enqueue(buf, buf.Length);
                Assert.Equal(0, sut.Space);
                Assert.Equal(max, sut.Length);
                var dequeuebuf1 = sut.Dequeue(max + val);
                Assert.Equal(max + excepted, dequeuebuf1.Length);
                Assert.Equal(byte.MaxValue, dequeuebuf1[0]);
                Assert.Equal(byte.MinValue, dequeuebuf1[1]);

                // free
                sut.Dequeue(sut.Length);

                sut.Enqueue(buf, buf.Length);
                Assert.Equal(0, sut.Space);
                Assert.Equal(max, sut.Length);
                var dequeuebuf2 = sut.Dequeue(max + val);
                Assert.Equal(max + excepted, dequeuebuf2.Length);
                Assert.Equal(byte.MaxValue, dequeuebuf2[0]);
                Assert.Equal(byte.MinValue, dequeuebuf2[1]);

                // free
                sut.Dequeue(sut.Length);

                sut.Enqueue(buf, buf.Length);
                Assert.Equal(0, sut.Space);
                Assert.Equal(max, sut.Length);
                var dequeuebuf3 = sut.Dequeue(max + val);
                Assert.Equal(max + excepted, dequeuebuf3.Length);
                Assert.Equal(byte.MaxValue, dequeuebuf3[0]);
                Assert.Equal(byte.MinValue, dequeuebuf3[1]);
            }
        }


        [Fact]
        public void SockQueueスペース確認()
        {
            const int max = 2000000;

            using (var sockQueu = new SockQueue())
            {

                var space = sockQueu.Space;
                //キューの空きサイズ
                Assert.Equal(space, max);

                var buf = new byte[max - 100];
                sockQueu.Enqueue(buf, buf.Length);

                space = sockQueu.Space;
                //キューの空きサイズ
                Assert.Equal(space, 100);

                var len = sockQueu.Enqueue(buf, 200);
                //空きサイズを超えて格納すると失敗する(※0が返る)
                Assert.Equal(len, 0);
            }
        }

        [Fact]
        public void SockQueueスペース確認複数回()
        {
            const int max = 2000000;

            using (var sockQueu = new SockQueue())
            {

                //キューの空きサイズ
                Assert.Equal(max, sockQueu.Space);

                var buf = new byte[max - 100];
                buf[0] = byte.MaxValue;
                sockQueu.Enqueue(buf, buf.Length);

                //キューの空きサイズ
                Assert.Equal(100, sockQueu.Space);

                var recv1 = sockQueu.Dequeue(max - 100);
                Assert.Equal(byte.MaxValue, recv1[0]);
                sockQueu.Enqueue(buf, buf.Length);

                //キューの空きサイズ
                Assert.Equal(100, sockQueu.Space);

                var len = sockQueu.Enqueue(buf, 200);
                //空きサイズを超えて格納すると失敗する(※0が返る)
                Assert.Equal(0, len);

                var recv2 = sockQueu.Dequeue(max - 100);
                Assert.Equal(byte.MaxValue, recv2[0]);
            }
        }

        [Fact]
        public void SockQueuePerformancePerMax()
        {
            using (var sockQueu = new SockQueue())
            {
                int max = sockQueu.Max;
                var buf = new byte[max];

                for (var i = 0; i < 1000; i++)
                {
                    sockQueu.Enqueue(buf, buf.Length);
                    Assert.Equal(0, sockQueu.Space);
                    Assert.Equal(max, sockQueu.Length);

                    var bufdequeue = sockQueu.Dequeue(max);
                    Assert.Equal(max, sockQueu.Space);
                    Assert.Equal(0, sockQueu.Length);
                }
            }
        }

        [Fact]
        public void SockQueuePerformancePerMiddle()
        {
            using (var sockQueu = new SockQueue())
            {
                int max = 1000;
                var buf = new byte[max];

                for (var i = 0; i < 1000000; i++)
                {
                    sockQueu.Enqueue(buf, buf.Length);
                    Assert.Equal(max, sockQueu.Length);

                    var bufdequeue = sockQueu.Dequeue(max);
                    Assert.Equal(sockQueu.Max, sockQueu.Space);
                    Assert.Equal(0, sockQueu.Length);
                }
            }
        }

        [Fact]
        public void SockQueuePerformancePerSmall()
        {
            using (var sockQueu = new SockQueue())
            {
                int max = 512;
                var buf = new byte[max];

                for (var i = 0; i < 1000000; i++)
                {
                    sockQueu.Enqueue(buf, buf.Length);
                    Assert.Equal(max, sockQueu.Length);

                    var bufdequeue = sockQueu.Dequeue(max);
                    Assert.Equal(sockQueu.Max, sockQueu.Space);
                    Assert.Equal(0, sockQueu.Length);
                }
            }
        }

        [Fact]
        public void SockQueue_DequeueLine()
        {
            //int max = 1048560;

            using (var sockQueu = new SockQueue())
            {

                var lines = new byte[] { 0x61, 0x0d, 0x0a, 0x62, 0x0d, 0x0a, 0x63 };
                sockQueu.Enqueue(lines, lines.Length);
                //2行と改行なしの1行で初期化

                var buf = sockQueu.DequeueLine();
                //sockQueue.dequeuLine()=\"1/r/n\" 1行目取得
                Assert.Equal(buf, new byte[] { 0x61, 0x0d, 0x0a });

                //sockQueue.dequeuLine()=\"2/r/n\" 2行目取得 
                buf = sockQueu.DequeueLine();
                Assert.Equal(buf, new byte[] { 0x62, 0x0d, 0x0a });

                buf = sockQueu.DequeueLine();
                //sockQueue.dequeuLine()=\"\" 3行目の取得は失敗する
                Assert.Equal(buf, new byte[0]);

                lines = new byte[] { 0x0d, 0x0a };
                sockQueu.Enqueue(lines, lines.Length);
                //"sockQueue.enqueu(/r/n) 改行のみ追加

                buf = sockQueu.DequeueLine();
                //sockQueue.dequeuLine()=\"3\" 3行目の取得に成功する"
                Assert.Equal(buf, new byte[] { 0x63, 0x0d, 0x0a });
            }
        }

        [Fact]
        public void SockQueue_DequeueLinelf()
        {
            //int max = 1048560;

            using (var sockQueu = new SockQueue())
            {

                var lines = new byte[] { 0x61, 0x0a, 0x62, 0x0a, 0x63 };
                sockQueu.Enqueue(lines, lines.Length);
                //2行と改行なしの1行で初期化

                var buf = sockQueu.DequeueLine();
                //sockQueue.dequeuLine()=\"1/n\" 1行目取得
                Assert.Equal(buf, new byte[] { 0x61, 0x0a });

                //sockQueue.dequeuLine()=\"2/n\" 2行目取得 
                buf = sockQueu.DequeueLine();
                Assert.Equal(buf, new byte[] { 0x62, 0x0a });

                buf = sockQueu.DequeueLine();
                //sockQueue.dequeuLine()=\"\" 3行目の取得は失敗する
                Assert.Equal(buf, new byte[0]);

                lines = new byte[] { 0x0a };
                sockQueu.Enqueue(lines, lines.Length);
                //"sockQueue.enqueu(/n) 改行のみ追加

                buf = sockQueu.DequeueLine();
                //sockQueue.dequeuLine()=\"3\" 3行目の取得に成功する"
                Assert.Equal(buf, new byte[] { 0x63, 0x0a });
            }
        }

        [Fact]
        public void SockQueueMaxDequeueLine()
        {
            const int max = 2000000;

            using (var sockQueu = new SockQueue())
            {

                //2行と改行なしの1行で初期化
                var lines = new byte[] { 0x61, 0x0d, 0x0a, 0x62, 0x0d, 0x0a };
                sockQueu.Enqueue(lines, lines.Length);
                Assert.Equal(max, sockQueu.Space + sockQueu.Length);
                Assert.Equal(max - lines.Length, sockQueu.Space);
                Assert.Equal(lines.Length, sockQueu.Length);

                var sendbuf = new byte[max - lines.Length];
                sockQueu.Enqueue(sendbuf, sendbuf.Length);
                Assert.Equal(max, sockQueu.Space + sockQueu.Length);
                Assert.Equal(max - lines.Length - sendbuf.Length, sockQueu.Space);
                Assert.Equal(max, sockQueu.Length);

                var recvbuf1 = sockQueu.DequeueLine();
                //sockQueue.dequeuLine()=\"1/r/n\" 1行目取得
                Assert.Equal(new byte[] { 0x61, 0x0d, 0x0a }, recvbuf1);

                //sockQueue.dequeuLine()=\"2/r/n\" 2行目取得 
                var recvbuf2 = sockQueu.DequeueLine();
                Assert.Equal(new byte[] { 0x62, 0x0d, 0x0a }, recvbuf2);

                var recvbuf3 = sockQueu.DequeueLine();
                //sockQueue.dequeuLine()=\"\" 3行目の取得は失敗する
                Assert.Equal(new byte[0], recvbuf3);

                lines = new byte[] { 0x63, 0x0d, 0x0a };
                sockQueu.Enqueue(lines, lines.Length);
                //"sockQueue.enqueu(/r/n) 改行のみ追加

                var recvbuf4 = sockQueu.Dequeue(sendbuf.Length);
                Assert.Equal(sendbuf, recvbuf4);


                var recvbuf5 = sockQueu.DequeueLine();
                //sockQueue.dequeuLine()=\"3\" 3行目の取得に成功する"
                Assert.Equal(new byte[] { 0x63, 0x0d, 0x0a }, recvbuf5);
            }
        }

        [Fact]
        public void SockQueueMaxDequeueLineWait()
        {
            const int max = 2000000;

            using (var sockQueu = new SockQueue())
            using (var cancel = new System.Threading.CancellationTokenSource())
            {



                //2行と改行なしの1行で初期化
                var lines = new byte[] { 0x61, 0x0d, 0x0a, 0x62, 0x0d, 0x0a };
                sockQueu.Enqueue(lines, lines.Length);
                Assert.Equal(max, sockQueu.Space + sockQueu.Length);
                Assert.Equal(max - lines.Length, sockQueu.Space);
                Assert.Equal(lines.Length, sockQueu.Length);

                var sendbuf = new byte[max - lines.Length];
                sockQueu.Enqueue(sendbuf, sendbuf.Length);
                Assert.Equal(max, sockQueu.Space + sockQueu.Length);
                Assert.Equal(max - lines.Length - sendbuf.Length, sockQueu.Space);
                Assert.Equal(max, sockQueu.Length);

                var recvbuf1 = sockQueu.DequeueLineWait(waitTimeout, cancel.Token);
                //sockQueue.dequeuLine()=\"1/r/n\" 1行目取得
                Assert.Equal(new byte[] { 0x61, 0x0d, 0x0a }, recvbuf1);

                //sockQueue.dequeuLine()=\"2/r/n\" 2行目取得 
                var recvbuf2 = sockQueu.DequeueLineWait(waitTimeout, cancel.Token);
                Assert.Equal(new byte[] { 0x62, 0x0d, 0x0a }, recvbuf2);

                var recvbuf3 = sockQueu.DequeueLineWait(waitTimeout, cancel.Token);
                //sockQueue.dequeuLine()=\"\" 3行目の取得は失敗する
                Assert.Equal(new byte[0], recvbuf3);

                lines = new byte[] { 0x63, 0x0d, 0x0a };
                sockQueu.Enqueue(lines, lines.Length);
                //"sockQueue.enqueu(/r/n) 改行のみ追加

                var recvbuf4 = sockQueu.DequeueWait(sendbuf.Length, waitTimeout, cancel.Token);
                Assert.Equal(sendbuf, recvbuf4);


                var recvbuf5 = sockQueu.DequeueLineWait(waitTimeout, cancel.Token);
                //sockQueue.dequeuLine()=\"3\" 3行目の取得に成功する"
                Assert.Equal(new byte[] { 0x63, 0x0d, 0x0a }, recvbuf5);
            }
        }

        [Theory]
        [InlineData(2000000, 0)]
        [InlineData(1999999, 1)]
        [InlineData(2000000, 2000000)]
        public void AfterSpace(int expected, int add)
        {
            //setUp
            using (var sut = new SockQueue())
            {
                sut.NotifyWrite(add);
                //exercise
                var actual = sut.AfterSpace;
                //verify
                Assert.Equal(expected, actual);

            }
        }

        [Theory]
        [InlineData(2000000, 0)]
        [InlineData(2000000, 1)]
        [InlineData(2000000, 2000000)]
        public void AfterLength(int expected, int add)
        {
            //setUp
            using (var sut = new SockQueue())
            {
                sut.NotifyWrite(add);
                //exercise
                var actual = sut.AfterLength;
                //verify
                Assert.Equal(expected, actual);

            }
        }

        [Theory]
        [InlineData(2000000, 0)]
        [InlineData(1999999, 1)]
        [InlineData(1, 1999999)]
        [InlineData(0, 2000000)]
        public void GetWriteSegmentCount(int expected, int len)
        {
            //setUp
            using (var sut = new SockQueue())
            {
                sut.NotifyWrite(len);
                //exercise
                var actual = sut.GetWriteSegment();
                //verify
                Assert.Equal(expected, actual.Count);

            }
        }

        [Theory]
        [InlineData(2000000, 0)]
        [InlineData(1999999, 1)]
        [InlineData(1, 1999999)]
        [InlineData(0, 2000000)]
        public void GetWriteSegmentCountWithLoop(int expected, int len)
        {
            //setUp
            using (var sut = new SockQueue())
            {
                sut.NotifyWrite(sut.Max);
                sut.Dequeue(sut.Max);

                sut.NotifyWrite(len);
                //exercise
                var actual = sut.GetWriteSegment();
                //verify
                Assert.Equal(expected, actual.Count);

            }
        }


        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(1999999, 1999999)]
        [InlineData(0, 2000000)]
        public void GetWriteSegmentOffset(int expected, int len)
        {
            //setUp
            using (var sut = new SockQueue())
            {
                sut.NotifyWrite(len);
                //exercise
                var actual = sut.GetWriteSegment();
                //verify
                Assert.Equal(expected, actual.Offset);

            }
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(1999999, 1999999)]
        [InlineData(0, 2000000)]
        public void GetWriteSegmentOffsetWithLoop(int expected, int len)
        {
            //setUp
            using (var sut = new SockQueue())
            {
                sut.NotifyWrite(sut.Max);
                sut.Dequeue(sut.Max);

                sut.NotifyWrite(len);
                //exercise
                var actual = sut.GetWriteSegment();
                //verify
                Assert.Equal(expected, actual.Offset);

            }
        }


    }
}