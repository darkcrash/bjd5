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
        private static object Lock = new object();

        public static void Initialize(Kernel kernel)
        {
            if (IsInitialize)
            {
                kernel.Logger.TraceInformation("Define.Initialize Skip");
                return;
            }
            IsInitialize = true;
            kernel.Logger.TraceInformation("Define.Initialize Start");

            // get service
            var runtimeServices = PlatformServices.Default;
            var compilerOptions = CompilationOptions.Default;
            var applicationEnvironment = runtimeServices.Application;

            Define.Libraries = DependencyContext.Default.RuntimeLibraries.ToArray();

            if (applicationEnvironment == null)
                applicationEnvironment = PlatformServices.Default.Application;

            if (applicationEnvironment != null)
            {
                kernel.Logger.TraceError($"----------------------------------------------------------------");
                kernel.Logger.TraceError($"- {applicationEnvironment.ApplicationName} - {applicationEnvironment.ApplicationVersion}");
                kernel.Logger.TraceError($"- {applicationEnvironment.RuntimeFramework.FullName} ");
                kernel.Logger.TraceError($"----------------------------------------------------------------");
            }

            // CompilerOptions
            if (compilerOptions != null)
            {
                kernel.Logger.TraceInformation($"[CompilerOptions]");
                if (compilerOptions.AllowUnsafe.HasValue)
                    kernel.Logger.TraceInformation($"[CompilerOptions] AllowUnsafe:{(compilerOptions.AllowUnsafe)}");
                if (compilerOptions.DebugType != null)
                    kernel.Logger.TraceInformation($"[CompilerOptions] DebugType:{compilerOptions.DebugType}");
                if (compilerOptions.DelaySign.HasValue)
                    kernel.Logger.TraceInformation($"[CompilerOptions] DelaySign:{compilerOptions.DelaySign}");
                if (compilerOptions.Defines != null)
                {
                    foreach (var def in compilerOptions.Defines.ToArray())
                        kernel.Logger.TraceInformation($"[CompilerOptions][Defines] {def}");
                }
                if (compilerOptions.EmitEntryPoint.HasValue)
                    kernel.Logger.TraceInformation($"[CompilerOptions] EmitEntryPoint:{compilerOptions.EmitEntryPoint}");
                if (compilerOptions.KeyFile != null)
                    kernel.Logger.TraceInformation($"[CompilerOptions] KeyFile:{compilerOptions.KeyFile}");
                kernel.Logger.TraceInformation($"[CompilerOptions] LanguageVersion:{compilerOptions.LanguageVersion}");
                if (compilerOptions.Optimize.HasValue)
                    kernel.Logger.TraceInformation($"[CompilerOptions] Optimize:{compilerOptions.Optimize}");
                if (compilerOptions.Platform != null)
                    kernel.Logger.TraceInformation($"[CompilerOptions] Platform:{compilerOptions.Platform}");
                if (compilerOptions.WarningsAsErrors.HasValue)
                    kernel.Logger.TraceInformation($"[CompilerOptions] WarningsAsErrors:{compilerOptions.WarningsAsErrors}");
            }

            // RuntimeEnvironment
            kernel.Logger.TraceInformation($"[RuntimeEnvironment] OperatingSystem:{(RuntimeEnvironment.OperatingSystem)}");
            kernel.Logger.TraceInformation($"[RuntimeEnvironment] OperatingSystemPlatform:{(RuntimeEnvironment.OperatingSystemPlatform)}");
            kernel.Logger.TraceInformation($"[RuntimeEnvironment] OperatingSystemVersion:{(RuntimeEnvironment.OperatingSystemVersion)}");
            kernel.Logger.TraceInformation($"[RuntimeEnvironment] RuntimeArchitecture:{(RuntimeEnvironment.RuntimeArchitecture)}");

            // ApplicationEnvironment
            if (applicationEnvironment != null)
            {
                kernel.Logger.TraceInformation($"[ApplicationEnvironment] ApplicationBasePath:{(applicationEnvironment.ApplicationBasePath)}");
                kernel.Logger.TraceInformation($"[ApplicationEnvironment] ApplicationName:{(applicationEnvironment.ApplicationName)}");
                kernel.Logger.TraceInformation($"[ApplicationEnvironment] ApplicationVersion:{(applicationEnvironment.ApplicationVersion)}");
                kernel.Logger.TraceInformation($"[ApplicationEnvironment] RuntimeFramework:{(applicationEnvironment.RuntimeFramework.FullName)}");
                kernel.Logger.TraceInformation($"[ApplicationEnvironment] RuntimeFrameworkIdentifier:{(applicationEnvironment.RuntimeFramework.Identifier)}");
                kernel.Logger.TraceInformation($"[ApplicationEnvironment] RuntimeFrameworkVersion:{applicationEnvironment.RuntimeFramework.Version}");
            }

            // Define.Libraries
            if (Define.Libraries != null)
            {
                kernel.Logger.TraceInformation($"[LibraryManager]");
                foreach (var lib in Define.Libraries)
                {
                    kernel.Logger.TraceInformation($"[LibraryManager]({lib.Type}) {lib.Name.PadRight(61)} {lib.Version.PadRight(17)} Assemblies({lib.Assemblies.Count().ToString().PadLeft(2)}) Dependencies({ lib.Dependencies.Count().ToString().PadLeft(2)})");
                }
            }

            // current directory
            var dir = System.IO.Directory.GetCurrentDirectory();
            var asm = typeof(Define).GetTypeInfo().Assembly;
            var asmName = asm.GetName();
            kernel.Logger.TraceInformation($"CurrentDirectory:{dir}");
            kernel.Logger.TraceInformation($"AppContext.BaseDirectory:{AppContext.BaseDirectory}");
            kernel.Logger.TraceInformation($"Assembly.Location:{asm.Location}");

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
                    kernel.Logger.TraceError($"ServerAddressList:{ex.Message}");
                    kernel.Logger.TraceError(ex.StackTrace);
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

            kernel.Logger.TraceInformation("Define.Initialize End");
        }

        public static void TestInitalize(Kernel kernel)
        {
            lock (Lock)
            {
                if (IsInitialize)
                {
                    kernel.Logger.TraceInformation("Define.TestInitalize Skip");
                }
                else
                {
                    Initialize(kernel);
                    var parent = System.IO.Path.GetDirectoryName(AppContext.BaseDirectory);
                    parent = System.IO.Path.GetDirectoryName(parent);
                    Define.ExecutableDirectory = parent;
                }
            }
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
