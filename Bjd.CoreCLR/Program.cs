using System;
using Bjd.Services;
using System.Diagnostics;

namespace Bjd.Common
{
    public class Program
    {

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        public static void Main(string[] args)
        {
            try
            {

                DefaultConsoleService.ServiceMain();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                System.Diagnostics.Trace.TraceError(ex.StackTrace);
            }
        }
    }

}