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
using Microsoft.DotNet.InternalAbstractions;

namespace Bjd
{
    internal class Define
    {

        public static readonly string ApplicationName = "BlackJumboDog .NET Core";
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
            var runtimeServices = PlatformServices.Default;
            var compilerOptions = CompilationOptions.Default;
            var applicationEnvironment = runtimeServices.Application;

            Define.Libraries = DependencyContext.Default.RuntimeLibraries.ToArray();

            if (applicationEnvironment == null)
                applicationEnvironment = PlatformServices.Default.Application;

            if (applicationEnvironment != null)
            {
                Trace.TraceError($"----------------------------------------------------------------");
                Trace.TraceError($"- {applicationEnvironment.ApplicationName} - {applicationEnvironment.ApplicationVersion}");
                Trace.TraceError($"- {applicationEnvironment.RuntimeFramework.FullName} ");
                Trace.TraceError($"----------------------------------------------------------------");
            }

            // CompilerOptions
            if (compilerOptions != null)
            {
                Trace.TraceInformation($"[CompilerOptions]");
                if (compilerOptions.AllowUnsafe.HasValue)
                    Trace.TraceInformation($"[CompilerOptions] AllowUnsafe:{(compilerOptions.AllowUnsafe)}");
                if (compilerOptions.DebugType != null)
                    Trace.TraceInformation($"[CompilerOptions] DebugType:{compilerOptions.DebugType}");
                if (compilerOptions.DelaySign.HasValue)
                    Trace.TraceInformation($"[CompilerOptions] DelaySign:{compilerOptions.DelaySign}");
                if (compilerOptions.Defines != null)
                {
                    foreach (var def in compilerOptions.Defines.ToArray())
                        Trace.TraceInformation($"[CompilerOptions][Defines] {def}");
                }
                if (compilerOptions.EmitEntryPoint.HasValue)
                    Trace.TraceInformation($"[CompilerOptions] EmitEntryPoint:{compilerOptions.EmitEntryPoint}");
                if (compilerOptions.KeyFile != null)
                    Trace.TraceInformation($"[CompilerOptions] KeyFile:{compilerOptions.KeyFile}");
                Trace.TraceInformation($"[CompilerOptions] LanguageVersion:{compilerOptions.LanguageVersion}");
                if (compilerOptions.Optimize.HasValue)
                    Trace.TraceInformation($"[CompilerOptions] Optimize:{compilerOptions.Optimize}");
                if (compilerOptions.Platform != null)
                    Trace.TraceInformation($"[CompilerOptions] Platform:{compilerOptions.Platform}");
                if (compilerOptions.WarningsAsErrors.HasValue)
                    Trace.TraceInformation($"[CompilerOptions] WarningsAsErrors:{compilerOptions.WarningsAsErrors}");
            }

            // RuntimeEnvironment
                Trace.TraceInformation($"[RuntimeEnvironment] OperatingSystem:{(RuntimeEnvironment.OperatingSystem)}");
                Trace.TraceInformation($"[RuntimeEnvironment] OperatingSystemPlatform:{(RuntimeEnvironment.OperatingSystemPlatform)}");
                Trace.TraceInformation($"[RuntimeEnvironment] OperatingSystemVersion:{(RuntimeEnvironment.OperatingSystemVersion)}");
                Trace.TraceInformation($"[RuntimeEnvironment] RuntimeArchitecture:{(RuntimeEnvironment.RuntimeArchitecture)}");

            // ApplicationEnvironment
            if (applicationEnvironment != null)
            {
                Trace.TraceInformation($"[ApplicationEnvironment] ApplicationBasePath:{(applicationEnvironment.ApplicationBasePath)}");
                Trace.TraceInformation($"[ApplicationEnvironment] ApplicationName:{(applicationEnvironment.ApplicationName)}");
                Trace.TraceInformation($"[ApplicationEnvironment] ApplicationVersion:{(applicationEnvironment.ApplicationVersion)}");
                Trace.TraceInformation($"[ApplicationEnvironment] RuntimeFramework:{(applicationEnvironment.RuntimeFramework.FullName)}");
                Trace.TraceInformation($"[ApplicationEnvironment] RuntimeFrameworkIdentifier:{(applicationEnvironment.RuntimeFramework.Identifier)}");
                Trace.TraceInformation($"[ApplicationEnvironment] RuntimeFrameworkVersion:{applicationEnvironment.RuntimeFramework.Version}");
            }

            // Define.Libraries
            if (Define.Libraries != null)
            {
                Trace.TraceInformation($"[LibraryManager]");
                foreach (var lib in Define.Libraries)
                {
                    Trace.TraceInformation($"[LibraryManager]({lib.Type}) {lib.Name.PadRight(61)} {lib.Version.PadRight(17)} Assemblies({lib.Assemblies.Count().ToString().PadLeft(2)}) Dependencies({ lib.Dependencies.Count().ToString().PadLeft(2)})");
                }
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
            Define.ExecutableDirectory = System.IO.Directory.GetCurrentDirectory();
            Define.ExecutablePath = AppContext.BaseDirectory;
            Define.ProductVersion = asmName.Version.ToString();

            Define.OperatingSystem = $"{RuntimeEnvironment.OperatingSystem} {RuntimeEnvironment.OperatingSystemVersion}";
            Define.IsWindows = (RuntimeEnvironment.OperatingSystem == "Windows");

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

        private Define() { }


    }
}
