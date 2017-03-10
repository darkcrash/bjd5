using Bjd.Controls;
using Bjd.Configurations;
using Xunit;
using Bjd.ProxyHttpServer;

namespace ProxyHttpServerTest {
    
    public class LimitUrlTest {
        public enum LimitKind {
            Front = 0,//前方一致
            Rear = 1,//後方一致
            Part = 2,//部分一致
            Regular = 3//正規表現
        }
        
        Dat AddDat(Dat dat, LimitKind limitKind, string url) {
            dat.Add(true, string.Format("{0}\t{1}",url,(int)limitKind));
            return dat;
        }

        [Theory]
        [InlineData("[[]/?*", LimitKind.Regular, "http://www.yahoo.com/", false)]//正規表現が無効で、初期化に失敗している
        [InlineData(".*", LimitKind.Regular, "http://www.yahoo.com/", true)]
        [InlineData(".*", LimitKind.Regular, "http://smtp.yahoo.com/", true)]
        [InlineData(".*", LimitKind.Regular, "http://www.goo.co.jp/", true)]
        [InlineData(".*", LimitKind.Regular, "http://www.yahoo.co.jp", true)]
        [InlineData(".com/", LimitKind.Rear, "http://www.yahoo.com/", true)]
        [InlineData(".com/", LimitKind.Rear, "http://smtp.yahoo.com/", true)]
        [InlineData(".com/", LimitKind.Rear, "http://www.goo.co.jp/", false)]
        [InlineData(".com/", LimitKind.Rear, "http://www.yahoo.co.jp", false)]
        [InlineData("yahoo.com", LimitKind.Part, "http://www.yahoo.com/", true)]
        [InlineData("yahoo.com", LimitKind.Part, "http://smtp.yahoo.com/", true)]
        [InlineData("yahoo.com", LimitKind.Part, "http://www.goo.co.jp/", false)]
        [InlineData("yahoo.com", LimitKind.Part, "http://www.yahoo.co.jp", false)]
        [InlineData("http://www.goo.com/", LimitKind.Front, "http://www.goo.com/", true)]
        [InlineData("http://www.goo.com/", LimitKind.Front, "http://www.goo.com/test", true)]
        [InlineData("http://www.goo.com/", LimitKind.Front, "http://www.go.co.jp/", false)]
        [InlineData("http://www.goo.com/", LimitKind.Front, "http://www.go.co", false)]
        public void AllowTest(string str, LimitKind limitKind, string target, bool isAllow) {
            var allow = new Dat(new[]{CtrlType.TextBox, CtrlType.Int });
            var deny = new Dat(new[] { CtrlType.TextBox, CtrlType.Int });
            allow = AddDat(allow, limitKind, str);
            var limitUrl = new LimitUrl(allow, deny);
            var errorStr = "";
            Assert.Equal(limitUrl.IsAllow(target, ref errorStr), isAllow);
        }

        [Theory]
        [InlineData("[[]/?*", LimitKind.Regular, "http://www.yahoo.com/", true)]//正規表現が無効で、初期化に失敗している
        [InlineData(".*", LimitKind.Regular, "http://www.yahoo.com/", false)]
        [InlineData(".*", LimitKind.Regular, "http://smtp.yahoo.com/", false)]
        [InlineData(".*", LimitKind.Regular, "http://www.goo.co.jp/", false)]
        [InlineData(".*", LimitKind.Regular, "http://www.yahoo.co.jp", false)]
        [InlineData(".com/", LimitKind.Rear, "http://www.yahoo.com/", false)]
        [InlineData(".com/", LimitKind.Rear, "http://smtp.yahoo.com/", false)]
        [InlineData(".com/", LimitKind.Rear, "http://www.goo.co.jp/", true)]
        [InlineData(".com/", LimitKind.Rear, "http://www.yahoo.co.jp", true)]
        [InlineData("yahoo.com", LimitKind.Part, "http://www.yahoo.com/", false)]
        [InlineData("yahoo.com", LimitKind.Part, "http://smtp.yahoo.com/", false)]
        [InlineData("yahoo.com", LimitKind.Part, "http://www.goo.co.jp/", true)]
        [InlineData("yahoo.com", LimitKind.Part, "http://www.yahoo.co.jp", true)]
        [InlineData("http://www.goo.com/", LimitKind.Front, "http://www.goo.com/", false)]
        [InlineData("http://www.goo.com/", LimitKind.Front, "http://www.goo.com/test", false)]
        [InlineData("http://www.goo.com/", LimitKind.Front, "http://www.go.co.jp/", true)]
        [InlineData("http://www.goo.com/", LimitKind.Front, "http://www.go.co", true)]
        public void DenyTest(string str, LimitKind limitKind, string target, bool isAllow) {
            var allow = new Dat(new[] { CtrlType.TextBox, CtrlType.Int });
            var deny = new Dat(new[] { CtrlType.TextBox, CtrlType.Int });
            deny = AddDat(deny, limitKind, str);
            LimitUrl limitUrl = new LimitUrl(allow, deny);
            var errorStr = "";
            Assert.Equal(limitUrl.IsAllow(target, ref errorStr), isAllow);
        }

        [Theory]
        [InlineData("go.com", LimitKind.Part,".*",LimitKind.Regular,"http://www.go.com/", true)]
        [InlineData("go.com", LimitKind.Part, ".*", LimitKind.Regular, "http://www.go.co", false)]
        public void AllowDenyTest(string allowStr, LimitKind allowKind, string denyStr, LimitKind denyKind, string target, bool isAllow) {
            var allow = new Dat(new[] { CtrlType.TextBox, CtrlType.Int });
            var deny = new Dat(new[] { CtrlType.TextBox, CtrlType.Int });
            allow = AddDat(allow, allowKind, allowStr);
            deny = AddDat(deny, denyKind, denyStr);
            var limitUrl = new LimitUrl(allow, deny);
            var errorStr = "";
            Assert.Equal(limitUrl.IsAllow(target, ref errorStr), isAllow);
        }


    }
}
