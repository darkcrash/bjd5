using System;
using Bjd.Services;
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
                        DefaultConsoleService.Start();
                        return;
                    }
                    if (argsList.Contains("--interactive"))
                    {
                        InteractiveConsoleService.Start();
                        return;
                    }
                    if (argsList.Contains("--service"))
                    {
                        DefaultService.Start();
                        return;
                    }
                }
                InteractiveConsoleService.Start();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                System.Diagnostics.Trace.TraceError(ex.StackTrace);
            }
        }
    }

}