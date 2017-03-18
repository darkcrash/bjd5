using System;
using Bjd.Initialization;
using System.Diagnostics;
using System.Collections.Generic;

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
                if (args != null)
                {
                    var argsList = new List<string>(args);
                    if (argsList.Contains("--console"))
                    {
                        DefaultConsole.Start();
                        return;
                    }
                    if (argsList.Contains("--interactive"))
                    {
                        InteractiveConsole.Start();
                        return;
                    }
                    if (argsList.Contains("--service"))
                    {
                        Default.Start();
                        return;
                    }
                }
                InteractiveConsole.Start();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                System.Diagnostics.Trace.TraceError(ex.StackTrace);
            }
        }
    }

}