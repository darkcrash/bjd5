using Xunit;
using Bjd.SmtpServer;


namespace Bjd.SmtpServer.Test {

    public class MlSubjectTest {
        

  //      readonly List<string> _domainList = new List<string>() { "example.com" };


        [Theory]
        [InlineData(100, "本日は晴天なり", "1ban", 0, "(1ban) 本日は晴天なり")]
        [InlineData(100, "本日は晴天なり", "1ban", 1, "[1ban] 本日は晴天なり")]
        [InlineData(100, "本日は晴天なり", "1ban", 2, "(00100) 本日は晴天なり")]
        [InlineData(100, "本日は晴天なり", "1ban", 3, "[00100] 本日は晴天なり")]
        [InlineData(100, "本日は晴天なり", "1ban", 4, "(1ban:00100) 本日は晴天なり")]
        [InlineData(100, "本日は晴天なり", "1ban", 5, "[1ban:00100] 本日は晴天なり")]
        [InlineData(100, "本日は晴天なり", "1ban", 6, " 本日は晴天なり")]
        public void Get2Test(int no, string subject, string mlName, int kind, string ansStr) {
            var mlSubject = new MlSubject(kind,mlName);
            //連番を付加したSubjectの生成
            Assert.Equal(ansStr, mlSubject.Get(subject, no));
        }


        [Theory]
        [InlineData(100)]
        [InlineData(100000)]
        [InlineData(1000000000)]
        [InlineData(0)]
        public void GetTest(int no) {
            const string mlName = "1ban";   
 
            for(var kind =0 ; kind<7 ; kind++){
                var mlSubject = new MlSubject(kind,mlName);
                var s = mlSubject.Get(no);
                switch(kind){
                    case 0: Assert.Equal(s,string.Format("({0})",mlName));
                            break;
                    case 1: Assert.Equal(s,string.Format("[{0}]",mlName));
                            break;
                    case 2: Assert.Equal(s,string.Format("({0:D5})",no));
                            break;
                    case 3: Assert.Equal(s,string.Format("[{0:D5}]",no));
                            break;
                    case 4: Assert.Equal(s,string.Format("({0}:{1:D5})",mlName,no));
                            break;
                    case 5: Assert.Equal(s,string.Format("[{0}:{1:D5}]",mlName,no));
                            break;
                    case 6: Assert.Equal(s,string.Format(""));
                            break;
                }
            }
        }
        
    }
}
