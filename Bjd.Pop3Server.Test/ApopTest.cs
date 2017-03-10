using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using Bjd.Pop3Server;

namespace Pop3ServerTest{
    public class ApopTest{

        [Theory]
        [InlineData("user1", "user1", true)]
        [InlineData("user1", "xxx", false)] //パスワードが間違えた場合失敗する
        [InlineData("user4", "", false)] //登録されていないユーザは失敗する
        [InlineData("user3", "", false)] //パスワードが無効のユーザは、失敗する
        public void APopAuthによる認証_チャレンジ文字列対応(string user, string pass, bool expected){
            //setUp
            const string challengeStr = "solt";
            byte[] data = Encoding.ASCII.GetBytes(challengeStr + pass);
           
            //MD5 md5 = new MD5CryptoServiceProvider();
            var md5 = System.Security.Cryptography.MD5.Create();
            md5.Initialize();


            byte[] result = md5.ComputeHash(data);
            var sb = new StringBuilder();
            for (int i = 0; i < 16; i++){
                sb.Append(string.Format("{0:x2}", result[i]));
            }

            //exercise
            //MailBoxの設定がuser=passだった場合のテスト
            //パラメータのpassはクライアントからの入力と仮定する
            var actual = APop.Auth(user, user,challengeStr, sb.ToString());
            //verify
            Assert.Equal(expected, actual);
        }
    }
}
