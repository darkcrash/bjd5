using System;
using Xunit;
using Bjd.ProxyHttpServer;

namespace ProxyHttpServerTest
{
    public class BodyBufTest : IDisposable
    {

        //BodyBuf bodyBuf;
        byte[] _dmyData;
        private const int DmyMax = 1000; //1Kbyte

        public BodyBufTest()
        {
            _dmyData = new byte[DmyMax];
            for (int i = 0; i < DmyMax; i++)
            {
                _dmyData[i] = (byte)i;
            }

        }

        public void Dispose()
        {
        }

        [Theory]
        [InlineData(1, 640000)]
        [InlineData(1000, 6400)]
        [InlineData(65535, 6400)]
        public void LengthTest(int count, int max)
        {

            var bodyBuf = new BodyBuf(max);

            for (var i = 0; i < count; i++)
            {
                bodyBuf.Add(_dmyData);
                bodyBuf.SendBuf(i * DmyMax);
            }
            Assert.Equal(bodyBuf.Length, count * DmyMax);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2000)]
        [InlineData(4294967)]//このサイズ(4G)までは正常動作、これ以上は動作保証できない (return null)
        public void SendBufTest(int count)
        {

            var bodyBuf = new BodyBuf(6400);

            for (var i = 0; i < count; i++)
            {
                bodyBuf.Add(_dmyData);
                var b = bodyBuf.SendBuf(i * DmyMax);
                Assert.Equal(b[10], 10);
            }
        }

        [Theory]
        [InlineData(1, 350)]//バッファリング内
        [InlineData(5, 222)]//バッファリングオーバー
        public void SetGetTest(int count, int pos)
        {//posテスト場所（0～999）

            var bodyBuf = new BodyBuf(DmyMax * 3);

            for (var i = 0; i < count; i++)
            {
                bodyBuf.Set(_dmyData);
                var b = bodyBuf.Get();
                Assert.Equal(b[pos], (byte)pos);
            }
        }

        //バッファリングできている場合の試験
        [Theory]
        [InlineData(1, 100)]
        [InlineData(50, 200)]
        [InlineData(100, null)]//バッファリングされているデータの最後を指定する
        public void SendBuf0Test(int count, int? pos)
        {

            var bodyBuf = new BodyBuf(DmyMax * 100);
            for (var i = 0; i < 100; i++)
            {
                bodyBuf.Add(_dmyData);
            }

            var b = bodyBuf.SendBuf(count * DmyMax);
            if (pos == null)
            {
                Assert.Null(b);
            }
            else
            {
                Assert.Equal(b[(int)pos], (byte)pos);
            }
        }

        //バッファリングされていない場合の試験
        [Theory]
        [InlineData(-1, 100)]//オーバした場合のリクエスト
        [InlineData(9000, 200)]//ぎりぎり最後のデータ
        public void SendBuf2Test(int start, int? pos)
        {

            var bodyBuf = new BodyBuf(DmyMax * 3);
            for (int i = 0; i < 10; i++)
            {
                bodyBuf.Add(_dmyData);//バッファを超えて保存
            }
            var b = bodyBuf.SendBuf(start);
            Assert.Equal(b[(int)pos], (byte)pos);
        }

    }

}

