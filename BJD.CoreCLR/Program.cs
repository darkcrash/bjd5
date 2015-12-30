using System;
using Bjd.service;
using System.Diagnostics;

namespace Bjd {
    public class Program {

        private readonly IServiceProvider _serviceProvider;
        public Program(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        public void Main(string[] args)
        {
            try
            {

                //起動ユーザがSYSTEMの場合、サービス起動であると判断する
                if (args != null && args.Length > 0 && args[0].ToLower().Contains("-s"))
                {
                    Service.ServiceMain(_serviceProvider);
                }
                else
                {
                    Console.WriteLine("option -s service");
#if DEBUG
                    Console.ReadLine();
#endif
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
            }
        }
    }

}