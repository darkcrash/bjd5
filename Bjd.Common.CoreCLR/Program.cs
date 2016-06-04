﻿using System;
using Bjd.Services;
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

                DefaultService.ServiceMain(_serviceProvider);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                System.Diagnostics.Trace.TraceError(ex.StackTrace);
            }
        }
    }

}