using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd.Net;
using Bjd.DnsServer;
using Xunit;

namespace DnsServerTest
{


    public class RrDbTest_initSoa
    {

        [Fact]
        public void 予め同一ドメインのNSレコードが有る場合成功する()
        {
            //setUp
            RrDb sut = new RrDb();
            bool expected = true;
            sut.Add(new RrNs("aaa.com.", 0, "ns.aaa.com."));
            //exercise
            bool actual = RrDbTest.InitSoa(sut, "aaa.com.", "mail.", 1, 2, 3, 4, 5);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 予め同一ドメインのNSレコードが無い場合失敗する_レコードが無い()
        {
            //setUp
            RrDb sut = new RrDb();
            bool expected = false;
            //exercise
            bool actual = RrDbTest.InitSoa(sut, "aaa.com.", "mail.", 1, 2, 3, 4, 5);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 予め同一ドメインのNSレコードが無い場合失敗する_NSレコードはあるがドメインが違う()
        {
            //setUp
            RrDb sut = new RrDb();
            bool expected = false;
            sut.Add(new RrNs("bbb.com.", 0, "ns.bbb.com.")); //NSレコードはあるがドメインが違う
            //exercise
            bool actual = RrDbTest.InitSoa(sut, "aaa.com.", "mail.", 1, 2, 3, 4, 5);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 予め同一ドメインのNSレコードが無い場合失敗する_ドメインは同じだがNSレコードではない()
        {
            //setUp
            RrDb sut = new RrDb();
            bool expected = false;
            sut.Add(new RrA("aaa.com.", 0, new Ip("192.168.0.1"))); //ドメインは同じだがNSレコードではない
            //exercise
            bool actual = RrDbTest.InitSoa(sut, "aaa.com.", "mail.", 1, 2, 3, 4, 5);
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void 追加に成功したばあのSOAレコードの検証()
        {
            //setUp
            RrDb sut = new RrDb();
            sut.Add(new RrNs("aaa.com.", 0, "ns.aaa.com."));
            //exercise
            RrDbTest.InitSoa(sut, "aaa.com.", "root@aaa.com", 1, 2, 3, 4, 5);
            //verify
            Assert.Equal(RrDbTest.Size(sut), 2); //NS及びSOAの2件になっている
            RrSoa o = (RrSoa)RrDbTest.Get(sut, 1);
            Assert.Equal(o.NameServer, "ns.aaa.com.");
            Assert.Equal(o.PostMaster, "root.aaa.com."); //変換が完了している(@=>. 最後に.追加）
            Assert.Equal(o.Serial, 1U);
            Assert.Equal(o.Refresh, 2U);
            Assert.Equal(o.Retry, 3U);
            Assert.Equal(o.Expire, 4U);
            Assert.Equal(o.Minimum, 5U);
        }
    }
}