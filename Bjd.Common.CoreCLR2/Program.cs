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

                Service.ServiceMain(_serviceProvider);

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
            }
        }
    }

}