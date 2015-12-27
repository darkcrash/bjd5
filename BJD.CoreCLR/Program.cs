using System;
using Bjd.service;

namespace Bjd {
    static class Program {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {

                //起動ユーザがSYSTEMの場合、サービス起動であると判断する
                if (args != null && args.Length > 0 && args[0].ToLower().Contains("-s"))
                {
                    Service.ServiceMain();
                    return;
                }

                Console.WriteLine("option -s service");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}