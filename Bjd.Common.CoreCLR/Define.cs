using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;

namespace Bjd
{
    internal class Define
    {

        public static readonly string ApplicationName = "BlackJumboDog";
        public static readonly string Copyright = "Copyright(c) 1998/05.. by SIN/SapporoWorks";
        public static readonly string WebHome = "http://www.sapporoworks.ne.jp/spw/";
        public static readonly string WebDocument = "http://www.sapporoworks.ne.jp/spw/?page_id=517";
        public static readonly string WebSupport = "http://www.sapporoworks.ne.jp/sbbs/sbbs.cgi?book=bjd";

        public static RuntimeLibrary[] Libraries { get; private set; }
        public static string ExecutablePath { get; private set; }
        public static string ExecutableDirectory { get; private set; }
        public static string ProductVersion { get; private set; }
        public static string OperatingSystem { get; private set; }
        public static bool IsWindows { get; private set; }
        public static List<string> ServerAddressList { get; private set; }
        public static string HostName { get; private set; }

        public static event EventHandler ChangeOperationSystem;

        public static bool IsInitialize { get; private set; } = false;

        private static Define Instance = new Define();

        public static void Initialize()
        {
            IsInitialize = true;
            Trace.TraceInformation("Define.Initialize Start");

            // get service
            //var runtimeServices = GetService<Microsoft.Extensions.PlatformAbstractions.PlatformServices>(sb);
            var runtimeServices = PlatformServices.Default;
            //var compilerOptions = GetService<ICompilerOptions>(sb);
            var compilerOptions = CompilationOptions.Default;
            var runtimeEnvironment = runtimeServices.Runtime;
            var applicationEnvironment = runtimeServices.Application;

            Define.Libraries = DependencyContext.Default.RuntimeLibraries.ToArray();

            if (applicationEnvironment == null)
                applicationEnvironment = PlatformServices.Default.Application;
            if (runtimeEnvironment == null)
                runtimeEnvironment = PlatformServices.Default.Runtime;

            if (applicationEnvironment != null)
            {
                Trace.TraceError($"----------------------------------------------------------------");
                Trace.TraceError($"- {applicationEnvironment.ApplicationName} - {applicationEnvironment.ApplicationVersion}");
                Trace.TraceError($"- {applicationEnvironment.RuntimeFramework.FullName} ");
                Trace.TraceError($"----------------------------------------------------------------");
            }

            //// RuntimeServices
            //if (runtimeServices != null)
            //{
            //    Trace.TraceInformation($"RuntimeServices");
            //    Trace.Indent();
            //    foreach (var sv in runtimeServices.Services)
            //    {
            //        Trace.TraceInformation($"{(sv.FullName)}");
            //    }
            //    Trace.Unindent();
            //}

            // CompilerOptions
            if (compilerOptions != null)
            {
                Trace.TraceInformation($"CompilerOptions");
                Trace.Indent();
                if (compilerOptions.AllowUnsafe.HasValue)
                    Trace.TraceInformation($"AllowUnsafe:{(compilerOptions.AllowUnsafe)}");
                if (compilerOptions.DebugType != null)
                    Trace.TraceInformation($"DebugType:{compilerOptions.DebugType}");
                if (compilerOptions.DelaySign.HasValue)
                    Trace.TraceInformation($"DelaySign:{compilerOptions.DelaySign}");
                if (compilerOptions.Defines != null)
                {
                    Trace.Indent();
                    Trace.TraceInformation($"Defines:");
                    foreach (var def in compilerOptions.Defines.ToArray())
                        Trace.TraceInformation($"{def}");
                    Trace.Unindent();
                }
                if (compilerOptions.EmitEntryPoint.HasValue)
                    Trace.TraceInformation($"EmitEntryPoint:{compilerOptions.EmitEntryPoint}");
                if (compilerOptions.KeyFile != null)
                    Trace.TraceInformation($"KeyFile:{compilerOptions.KeyFile}");
                Trace.TraceInformation($"LanguageVersion:{compilerOptions.LanguageVersion}");
                if (compilerOptions.Optimize.HasValue)
                    Trace.TraceInformation($"Optimize:{compilerOptions.Optimize}");
                if (compilerOptions.Platform != null)
                    Trace.TraceInformation($"Platform:{compilerOptions.Platform}");
                //if (compilerOptions.UseOssSigning.HasValue)
                //    Trace.TraceInformation($"UseOssSigning:{compilerOptions.UseOssSigning}");
                if (compilerOptions.WarningsAsErrors.HasValue)
                    Trace.TraceInformation($"WarningsAsErrors:{compilerOptions.WarningsAsErrors}");
                Trace.Unindent();
            }

            // RuntimeEnvironment
            if (runtimeEnvironment != null)
            {
                Trace.TraceInformation($"RuntimeEnvironment");
                Trace.Indent();
                Trace.TraceInformation($"OperatingSystem:{(runtimeEnvironment.OperatingSystem)}");
                Trace.TraceInformation($"OperatingSystemVersion:{(runtimeEnvironment.OperatingSystemVersion)}");
                Trace.TraceInformation($"RuntimeArchitecture:{(runtimeEnvironment.RuntimeArchitecture)}");
                //Trace.TraceInformation($"RuntimePath:{(runtimeEnvironment.RuntimePath)}");
                Trace.TraceInformation($"RuntimeType:{(runtimeEnvironment.RuntimeType)}");
                Trace.TraceInformation($"RuntimeVersion:{(runtimeEnvironment.RuntimeVersion)}");
                Trace.Unindent();
            }

            // ApplicationEnvironment
            if (applicationEnvironment != null)
            {
                Trace.TraceInformation($"ApplicationEnvironment");
                Trace.Indent();
                Trace.TraceInformation($"ApplicationBasePath:{(applicationEnvironment.ApplicationBasePath)}");
                Trace.TraceInformation($"ApplicationName:{(applicationEnvironment.ApplicationName)}");
                Trace.TraceInformation($"ApplicationVersion:{(applicationEnvironment.ApplicationVersion)}");
                //Trace.TraceInformation($"Configuration:{(applicationEnvironment.Configuration)}");
                Trace.TraceInformation($"RuntimeFramework:{(applicationEnvironment.RuntimeFramework.FullName)}");
                Trace.TraceInformation($"RuntimeFrameworkIdentifier:{(applicationEnvironment.RuntimeFramework.Identifier)}");
                Trace.TraceInformation($"RuntimeFrameworkVersion:{applicationEnvironment.RuntimeFramework.Version}");
                Trace.Unindent();
            }

            // Define.Libraries
            if (Define.Libraries != null)
            {
                Trace.TraceInformation($"LibraryManager");
                Trace.Indent();
                foreach (var lib in Define.Libraries)
                {
                    Trace.TraceInformation($"({lib.Type}) {lib.Name.PadRight(61)} {lib.Version.PadRight(17)} Assemblies({lib.Assemblies.Count().ToString().PadLeft(2)}) Dependencies({ lib.Dependencies.Count().ToString().PadLeft(2)})");
                    //Trace.Indent();
                    //Trace.WriteLine($"{lib.Path}");
                    //Trace.Unindent();
                }
                Trace.Unindent();
            }

            // current directory
            var dir = System.IO.Directory.GetCurrentDirectory();
            var asm = typeof(Define).GetTypeInfo().Assembly;
            var asmName = asm.GetName();
            Trace.TraceInformation($"CurrentDirectory:{dir}");
            Trace.TraceInformation($"AppContext.BaseDirectory:{AppContext.BaseDirectory}");
            Trace.TraceInformation($"Assembly.Location:{asm.Location}");

            //プログラム起動から初めて呼び出されたとき、１度だけ実行される
            if (ServerAddressList == null)
            {
                ServerAddressList = new List<string>();
                try
                {
                    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface nic in nics)
                    {
                        if (nic.OperationalStatus != OperationalStatus.Up)
                            continue;
                        IPInterfaceProperties props = nic.GetIPProperties();
                        foreach (UnicastIPAddressInformation info in props.UnicastAddresses)
                        {
                            if (info.Address.AddressFamily == AddressFamily.InterNetwork)
                                ServerAddressList.Add(info.Address.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"ServerAddressList:{ex.Message}");
                    Trace.TraceError(ex.StackTrace);
                }

                Define.HostName = Dns.GetHostName();
            }


            // set define
            Define.ExecutableDirectory = AppContext.BaseDirectory;
            Define.ExecutablePath = AppContext.BaseDirectory;
            Define.ProductVersion = asmName.Version.ToString();

            if (runtimeEnvironment != null)
            {
                Define.OperatingSystem = $"{runtimeEnvironment.OperatingSystem} {runtimeEnvironment.OperatingSystemVersion}";
                Define.IsWindows = (runtimeEnvironment.OperatingSystem == "Windows");
            }
            else
            {
                Define.OperatingSystem = "Unknow";
                Define.IsWindows = false;
            }
            OnChangeOperationSystem();

            Trace.TraceInformation("Define.Initialize End");
        }

        public static void TestInitalize()
        {
            Initialize();
            var parent = System.IO.Path.GetDirectoryName(AppContext.BaseDirectory);
            parent = System.IO.Path.GetDirectoryName(parent);
            Define.ExecutableDirectory = parent;
        }

        protected static void OnChangeOperationSystem()
        {
            if (ChangeOperationSystem != null)
                ChangeOperationSystem(Instance, EventArgs.Empty);
        }

        public static string ServerAddress()
        {
            if (ServerAddressList.Count > 0)
                return ServerAddressList[0];
            return "127.0.0.1";
        }

        private static T GetService<T>(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) return default(T);
            return (T)serviceProvider.GetService(typeof(T));
        }

        private Define() { }


    }
}
