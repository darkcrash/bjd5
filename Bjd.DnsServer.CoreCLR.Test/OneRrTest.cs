﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd.net;
using Bjd.DnsServer;
using Xunit;

namespace DnsServerTest
{


    public class OneRrTest
    {

        //テスト用にOneRrを継承したクラスを定義する
        private class RrTest : OneRr
        {
            public RrTest(string name, DnsType dnsType, uint ttl, string data) : base(name, dnsType, ttl, Encoding.ASCII.GetBytes(data))
            {

            }
        }

        [Fact]
        public void isEffective_ttlが0の場合_どんな時間で確認してもtrueが返る()
        {
            //setUp
            const int ttl = 0;
            var sut = new RrTest("name", DnsType.A, ttl, "123");
            var expected = true;
            var now = 1; //nowはいくつであっても結果は変わらない
            //exercise
            var actual = sut.IsEffective(now);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void isEffective_ttlが10秒の場合_10秒後で確認するとtrueが返る()
        {
            //setUp
            //long now = Calendar.getInstance().getTimeInMillis(); //現在時間
            var now = DateTime.Now.Ticks / 10000000; //現在時間(秒単位)
            const int ttl = 10; //生存時間は10秒
            var sut = new RrTest("name", DnsType.A, ttl, "123");
            var expected = true;
            //exercise
            var actual = sut.IsEffective(now + 10); //10秒後
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void isEffective_ttlが10秒の場合_11秒後で確認するとfalseが返る()
        {
            //setUp
            //long now = Calendar.getInstance().getTimeInMillis(); //現在時間
            var now = DateTime.Now.Ticks / 10000000; //現在時間(秒単位)
            const int ttl = 10; //生存時間は10秒
            var sut = new RrTest("name", DnsType.A, ttl, "123");
            var expected = false;
            //exercise
            var actual = sut.IsEffective(now + 11); //11秒後
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void cloneでAレコードの複製を作成()
        {
            //setUp
            var expected = DnsType.A;
            var sut = new RrTest("name", expected, 10, "123");
            //exercise
            var o = sut.Clone(100);
            //verify
            Assert.Equal(o.Ttl, 100U); //TTLは100に変化している
            Assert.Equal(o.Name, "name"); //その他は同じ
            Assert.Equal(o.DnsType, expected); //その他は同じ
            Assert.Equal(o.Data, Encoding.ASCII.GetBytes("123")); //その他は同じ
        }

        [Fact]
        public void cloneでAAAAレコードの複製を作成()
        {
            //setUp
            var expected = DnsType.AAAA;
            var sut = new RrTest("name", expected, 10, "123");
            //exercise
            var o = sut.Clone(100);
            //verify
            Assert.Equal(o.Ttl, 100u); //TTLは100に変化している
            Assert.Equal(o.Name, "name"); //その他は同じ
            Assert.Equal(o.DnsType, expected); //その他は同じ
            Assert.Equal(o.Data, Encoding.ASCII.GetBytes("123")); //その他は同じ
        }

        [Fact]
        public void cloneでNSレコードの複製を作成()
        {
            //setUp
            var expected = DnsType.Ns;
            var sut = new RrTest("name", expected, 10, "123");
            //exercise
            var o = sut.Clone(100);
            //verify
            Assert.Equal(o.Ttl, 100U); //TTLは100に変化している
            Assert.Equal(o.Name, "name"); //その他は同じ
            Assert.Equal(o.DnsType, expected); //その他は同じ
            Assert.Equal(o.Data, Encoding.ASCII.GetBytes("123")); //その他は同じ
        }

        [Fact]
        public void cloneでMxレコードの複製を作成()
        {
            //setUp
            var expected = DnsType.Mx;
            var sut = new RrTest("name", expected, 10, "123");
            //exercise
            var o = sut.Clone(100);
            //verify
            Assert.Equal(o.Ttl, 100U); //TTLは100に変化している
            Assert.Equal(o.Name, "name"); //その他は同じ
            Assert.Equal(o.DnsType, expected); //その他は同じ
            Assert.Equal(o.Data, Encoding.ASCII.GetBytes("123")); //その他は同じ
        }

        [Fact]
        public void cloneでCnameレコードの複製を作成()
        {
            //setUp
            var expected = DnsType.Cname;
            var sut = new RrTest("name", expected, 10, "123");
            //exercise
            var o = sut.Clone(100);
            //verify
            Assert.Equal(o.Ttl, 100U); //TTLは100に変化している
            Assert.Equal(o.Name, "name"); //その他は同じ
            Assert.Equal(o.DnsType, expected); //その他は同じ
            Assert.Equal(o.Data, Encoding.ASCII.GetBytes("123")); //その他は同じ
        }

        [Fact]
        public void cloneでSoaレコードの複製を作成()
        {
            //setUp
            var expected = DnsType.Soa;
            var sut = new RrTest("name", expected, 10, "123");
            //exercise
            OneRr o = sut.Clone(100);
            //verify
            Assert.Equal(o.Ttl, 100U); //TTLは100に変化している
            Assert.Equal(o.Name, "name"); //その他は同じ
            Assert.Equal(o.DnsType, expected); //その他は同じ
            Assert.Equal(o.Data, Encoding.ASCII.GetBytes("123")); //その他は同じ
        }

        [Fact]
        public void equalsで同一のオブジェクトを比較するとtrueが返る()
        {
            //setUp
            var sut = new RrA("name", 10, new Ip("192.168.0.1"));
            var expected = true;
            //exercise
            var actual = sut.Equals(sut);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void equalsでnullトを比較するとfalseが返る()
        {
            //setUp
            var sut = new RrA("name", 10, new Ip("192.168.0.1"));
            var expected = false;
            //exercise
            var actual = sut.Equals(null);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void equalsでデータが異なるオブジェクトを比較するとfalseが返る()
        {
            //setUp
            var sut = new RrA("name", 10, new Ip("192.168.0.1"));
            var expected = false;
            //exercise
            var actual = sut.Equals(new RrA("name", 10, new Ip("192.168.0.2")));
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void equalsで名前が異なるオブジェクトを比較するとfalseが返る()
        {
            //setUp
            var sut = new RrA("name", 10, new Ip("192.168.0.1"));
            var expected = false;
            //exercise
            var actual = sut.Equals(new RrA("other", 10, new Ip("192.168.0.1")));
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void equalsでDnsTYpeが異なるオブジェクトを比較するとfalseが返る()
        {
            //setUp
            var sut = new RrA("name", 10, new Ip("0.0.0.1"));
            var expected = false;
            //exercise
            var actual = sut.Equals(new RrAaaa("name", 10, new Ip("::1")));
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void equalsでTTLが異なるオブジェクトを比較するとfalseが返る()
        {
            //setUp
            var sut = new RrA("name", 10, new Ip("0.0.0.1"));
            var expected = false;
            //exercise
            var actual = sut.Equals(new RrA("name", 20, new Ip("0.0.0.1")));
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void equalsでDataが異なるオブジェクトを比較するとfalseが返る()
        {
            //setUp
            var sut = new RrTest("name", DnsType.A, 10, "123");
            var expected = false;
            //exercise
            var actual = sut.Equals(new RrTest("name", DnsType.A, 10, "12"));
            //verify
            Assert.Equal(expected, actual);
        }
    }
}