﻿using System;
using System.Linq;
using Bjd.Logs;
using Bjd.Net;
using Xunit;
using Bjd.Initialization;

namespace Bjd.Test.Net
{
    public class DnsCacheTest : IDisposable
    {
        TestService _service;
        Kernel _kernel;

        public DnsCacheTest()
        {
            _service = TestService.CreateTestService();
            _kernel = _service.Kernel;
        }

        public void Dispose()
        {
            _service.Dispose();
        }

        [Fact]
        public void アドレスからホスト名を取得する()
        {

            //setUp
            var sut = new DnsCache();
            var ip = new Ip("59.106.27.208");
            const string expected = "www1968.sakura.ne.jp";

            //exercise
            String actual = sut.GetHostName(ip.IPAddress, new Logger(_kernel));

            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ホスト名からアドレスを取得する()
        {

            //setUp
            var sut = new DnsCache();
            const string expected = "59.106.27.208";

            //exercise
            var ipList = sut.GetAddress("www1968.sakura.ne.jp");
            var actual = ipList[0].ToString();

            //verify
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void 一度検索するとキャッシュ件数は１となる()
        {
            //setUp
            var sut = new DnsCache();

            const int expected = 1;

            //exercise
            sut.GetAddress("www.sapporoworks.ne.jp");
            var actual = sut.Size();

            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 同じ内容を複数回検索してもキャッシュ件数は１となる()
        {
            //setUp
            var sut = new DnsCache();

            const int expected = 1;

            //exercise
            sut.GetAddress("www.sapporoworks.ne.jp");
            sut.GetAddress("www.sapporoworks.ne.jp");
            sut.GetAddress("www.sapporoworks.ne.jp");
            var actual = sut.Size();

            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 違う内容を検索するとキャッシュ件数は２となる()
        {

            //setUp
            var sut = new DnsCache();

            const int expected = 2;

            //exercise
            sut.GetAddress("www.sapporoworks.ne.jp");
            sut.GetAddress("www.google.com");
            var actual = sut.Size();

            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 無効なホスト名を検索すると0件の配列が返される_タイムアウトに時間を要する()
        {

            //setUp
            var sut = new DnsCache();

            const int expected = 0;

            //exercise
            TestUtil.WaitDisp("無効ホスト名の検索　タイムアウトまで待機");
            var ipList = sut.GetAddress("xxx");
            var actual = ipList.Count();

            //verify
            Assert.Equal(expected, actual);
            //TearDown
            TestUtil.WaitDisp(null);

        }

        [Fact]
        public void 無効なアドレスを検索するとアドレス表記がそのまま返される_タイムアウトに時間を要する()
        {

            //setUp
            var sut = new DnsCache();
            //InetAddress inetAddress = InetAddress.getByName("1.1.1.1");
            var ip = new Ip("1.1.1.1");

            const string expected = "1.1.1.1";

            //exercise
            TestUtil.WaitDisp("無効アドレスの検索　タイムアウトまで待機");
            var actual = sut.GetHostName(ip.IPAddress, new Logger(_kernel));

            //verify
            Assert.Equal(expected, actual);
            //TearDown
            TestUtil.WaitDisp(null);
        }
    }
}