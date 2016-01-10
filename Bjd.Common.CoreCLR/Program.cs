using System;
using Bjd.service;
using System.Diagnostics;

namespace Bjd.Common
{
    public class Program
    {

        private static IServiceProvider _serviceProvider;
        public Program(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        public static void Main(string[] args)
        {
            try
            {

                Service.ServiceMain(_serviceProvider);

            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                Trace.TraceError(ex.StackTrace);
            }
        }
    }

}