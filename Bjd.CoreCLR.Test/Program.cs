using NUnitLite;
using System;
using System.Reflection;

namespace Bjd.CoreCLR.Test
{
    public class Program
    {
        public static int Main(string[] args)
        {
#if DNXCORE50
            var result = new AutoRun().Execute(typeof(Program).GetTypeInfo().Assembly, Console.Out, Console.In, args);
            return result;
#else
            return new AutoRun().Execute(args);
#endif
        }
        }
    }
