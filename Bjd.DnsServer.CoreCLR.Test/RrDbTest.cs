﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Options;
using Bjd.Utils;
using Bjd.Test;
using Bjd.DnsServer;
using Xunit;

namespace DnsServerTest
{


    public class RrDbTest
    {


        //リフレクションを使用してプライベートメソッドにアクセスする RrDb.size()
        public static int Size(RrDb sut)
        {
            var type = sut.GetType();
            var func = type.GetMethod("Size", BindingFlags.NonPublic | BindingFlags.Instance);
            return (int)func.Invoke(sut, new object[] { });
        }

        //リフレクションを使用してプライベートメソッドにアクセスする RrDb.addNamedCaLine(string tmpName, string str)
        public static string AddNamedCaLine(RrDb sut, string tmpName, string str)
        {
            var type = sut.GetType();
            var func = type.GetMethod("AddNamedCaLine", BindingFlags.NonPublic | BindingFlags.Instance);
            try
            {
                return (string)func.Invoke(sut, new object[] { tmpName, str });
            }
            catch (Exception e)
            {
                //リフレクションで呼び出したメソッドで例外が発生すると、System.Reflection.TargetInvocationException 
                //でラップされて、InnerException プロパティに発生した例外が設定される
                throw e.InnerException;
            }
        }

        //リフレクションを使用してプライベートメソッドにアクセスする RrDb.get(int)
        public static OneRr Get(RrDb sut, int index)
        {
            var type = sut.GetType();
            var func = type.GetMethod("Get", BindingFlags.NonPublic | BindingFlags.Instance);
            return (OneRr)func.Invoke(sut, new object[] { index });
        }

        //リフレクションを使用してプライベートメソッドにアクセスする RrDb.addOneDat(string,OneDat)
        public static void AddOneDat(RrDb sut, string domainName, OneDat oneDat)
        {
            var type = sut.GetType();
            var func = type.GetMethod("AddOneDat", BindingFlags.NonPublic | BindingFlags.Instance);
            try
            {
                func.Invoke(sut, new object[] { domainName, oneDat });
            }
            catch (Exception e)
            {
                //リフレクションで呼び出したメソッドで例外が発生すると、System.Reflection.TargetInvocationException 
                //でラップされて、InnerException プロパティに発生した例外が設定される
                throw e.InnerException;
            }
        }

        //リフレクションを使用してプライベートメソッドにアクセスする RrDb.initLocalHost()
        public static void InitLocalHost(RrDb sut)
        {
            var type = sut.GetType();
            var func = type.GetMethod("InitLocalHost", BindingFlags.NonPublic | BindingFlags.Instance);
            func.Invoke(sut, new object[] { });
        }

        //リフレクションを使用してプライベートメソッドにアクセスする RrDb.addOneDat(string,OneDat)
        public static bool InitSoa(RrDb sut, string domainName, string mail, uint serial, uint refresh, uint retry, uint expire, uint minimum)
        {
            var type = sut.GetType();
            var func = type.GetMethod("InitSoa", BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool)func.Invoke(sut, new object[] { domainName, mail, serial, refresh, retry, expire, minimum });
        }

        [Fact]
        //[ExpectedException(typeof(IOException))]
        //例外テスト
        public void コンストラクタの例外処理_指定したファイルが存在しない()
        {
            //exercise
            var namedCaPath = "dmy";
            Assert.Throws<IOException>(() =>
              new RrDb(namedCaPath, 2400)
                );
        }

        [Fact]
        public void getDomainNameの確認_namedcaで初期化された場合ルートになる()
        {
            //setUp
            var namedCaPath = Path.GetTempFileName();
            var sut = new RrDb(namedCaPath, 2400);
            var expected = ".";
            //exercise
            var actual = sut.GetDomainName();
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void getDomainNameの確認_Datで初期化された場合指定されたドメインになる()
        {
            //setUp
            var sut = new RrDb(null, null, null, "example.com", true);
            var expected = "example.com.";
            //exercise
            var actual = sut.GetDomainName();
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void getListによる検索_ヒットするデータが存在する場合()
        {
            //setUp
            var sut = new RrDb(null, null, null, "example.com", true);
            sut.Add(new RrA("www.example.com.", 100, new Ip("192.168.0.1")));
            var expected = 1;
            //exercise
            int actual = sut.GetList("www.example.com.", DnsType.A).Count;
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetListによる検索_ヒットするデータが存在しない場合()
        {
            //setUp
            var sut = new RrDb(null, null, null, "example.com", true);
            sut.Add(new RrA("www1.example.com.", 100, new Ip("192.168.0.1")));
            var expected = 0;
            //exercise
            var actual = sut.GetList("www.example.com.", DnsType.A).Count;
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetListによる検索_名前が同じでタイプのデータが存在する場合()
        {
            //setUp
            var sut = new RrDb(null, null, null, "example.com", true);
            sut.Add(new RrAaaa("www.example.com.", 100, new Ip("::1")));
            var expected = 0;
            //exercise
            var actual = sut.GetList("www.example.com.", DnsType.A).Count;
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetListを使用すると期限の切れたリソースが削除される()
        {
            //setUp
            var ttl = 1u; //TTL=1秒
            var sut = new RrDb(null, null, null, "example.com", true);
            sut.Add(new RrA("www.example.com.", ttl, new Ip("1.1.1.1")));
            sut.Add(new RrA("www.example.com.", ttl, new Ip("2.2.2.2")));
            var expected = 0;

            TestUtil.WaitDisp("RrDb.getList()で期限切れリソースの削除を確認するため、TTL指定時間が経過するまで待機");
            Thread.Sleep(2000); //２秒経過
            //exercise
            sut.GetList("www.example.com.", DnsType.A);
            var actual = RrDbTest.Size(sut); //DBのサイズは0になっている
            //verify
            Assert.Equal(expected, actual);
            //TearDown
            TestUtil.WaitDisp(null);
        }

        [Fact]
        public void Findによる検索_ヒットするデータが存在しない場合()
        {
            //setUp
            var sut = new RrDb(null, null, null, "example.com", true);
            sut.Add(new RrA("www1.example.com.", 100, new Ip("192.168.0.1")));
            var expected = false;
            //exercise
            var actual = sut.Find("www.example.com.", DnsType.A);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void findによる検索_ヒットするデータが存在する場合()
        {
            //setUp
            var sut = new RrDb(null, null, null, "example.com", true);
            sut.Add(new RrA("www1.example.com.", 100, new Ip("192.168.0.1")));
            sut.Add(new RrA("www.example.com.", 100, new Ip("192.168.0.1")));
            var expected = true;
            //exercise
            var actual = sut.Find("www.example.com.", DnsType.A);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 重複しない２つのリソースの追加()
        {
            //setUp
            var dat = new Dat(new CtrlType[5]);
            dat.Add(true, "0\twww\talias\t192.168.0.1\t10");
            dat.Add(true, "1\tns\talias\t192.168.0.1\t10");
            var sut = new RrDb(null, null, dat, "example.com", true);
            //(1)a   www.example.com. 192.168.0.1
            //(2)ptr 1.0.168.192.in.addr.ptr  www.example.com.
            //(3)ns  example.com. ns.example.com. 
            //(4)a   ns.example.com. 192.168.0.1
            //(5)ptr 1.0.168.192.in.addr.ptr  ns.example.com.
            var expected = 5;
            //exercise
            var actual = RrDbTest.Size(sut);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 重複する２つのリソースの追加()
        {
            //setUp
            var dat = new Dat(new CtrlType[5]);
            dat.Add(true, "0\twww\talias\t192.168.0.1\t10");
            dat.Add(true, "0\twww\talias\t192.168.0.1\t10");
            var sut = new RrDb(null, null, dat, "example.com", true);
            //(1)a   www.example.com. 192.168.0.1
            //(2)ptr 1.0.168.192.in.addr.ptr  www.example.com.
            var expected = 2;
            //exercise
            var actual = RrDbTest.Size(sut);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 一部が重複する２つのリソースの追加()
        {
            //setUp
            var dat = new Dat(new CtrlType[5]);
            dat.Add(true, "0\tns\talias\t192.168.0.1\t10");
            dat.Add(true, "1\tns\talias\t192.168.0.1\t10");
            var sut = new RrDb(null, null, dat, "example.com", true);
            //(1)a   ns.example.com. 192.168.0.1
            //(2)ptr 1.0.168.192.in.addr.ptr  ns.example.com.
            //(3)ns  example.com. ns.example.com. 
            var expected = 3;
            //exercise
            int actual = Size(sut);
            //verify
            Assert.Equal(expected, actual);
        }
    }
}