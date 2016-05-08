using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.PlatformAbstractions;

namespace Bjd
{
    public class Define
    {

        private Define() { }

        static Define _Instance = new Define();
        static Define Instance
        {
            get
            {
                if (!isInitialize) Initialize(null);
                return _Instance;
            }
        }
        static bool isInitialize = false;
        string _executablePath;
        string _executableDirectory;
        string _productVersion;
        string _OperatingSystem;
        bool isWindows = false;
        Library[] _libraries;

        public static void Initialize(IServiceProvider sb)
        {
            isInitialize = true;
            Trace.TraceInformation("Define.Initialize Start");

            // get service
            var runtimeServices = GetService<Microsoft.Extensions.PlatformAbstractions.IRuntimeServices>(sb);
            var compilerOptions = GetService<ICompilerOptions>(sb);
            var runtimeEnvironment = GetService<Microsoft.Extensions.PlatformAbstractions.IRuntimeEnvironment>(sb);
            var applicationEnvironment = GetService<IApplicationEnvironment>(sb);
            var libraryManager = GetService<ILibraryManager>(sb);

            if (applicationEnvironment == null)
                applicationEnvironment = PlatformServices.Default.Application;
            if (runtimeEnvironment == null)
                runtimeEnvironment = PlatformServices.Default.Runtime;
            if (libraryManager == null)
                libraryManager = PlatformServices.Default.LibraryManager;

            if (applicationEnvironment != null)
            {
                Trace.TraceError($"----------------------------------------------------------------");
                Trace.TraceError($"- {applicationEnvironment.ApplicationName} - {applicationEnvironment.ApplicationVersion}");
                Trace.TraceError($"- {applicationEnvironment.RuntimeFramework.FullName} ");
                Trace.TraceError($"----------------------------------------------------------------");
            }

            // RuntimeServices
            if (runtimeServices != null)
            {
                Trace.TraceInformation($"RuntimeServices");
                Trace.Indent();
                foreach (var sv in runtimeServices.Services)
                {
                    Trace.TraceInformation($"{(sv.FullName)}");
                }
                Trace.Unindent();
            }

            // CompilerOptions
            if (compilerOptions != null)
            {
                Trace.TraceInformation($"CompilerOptions");
                Trace.Indent();
                if (compilerOptions.AllowUnsafe.HasValue)
                    Trace.TraceInformation($"AllowUnsafe:{(compilerOptions.AllowUnsafe)}");
                if (compilerOptions.DelaySign.HasValue)
                    Trace.TraceInformation($"DelaySign:{compilerOptions.DelaySign}");
                if (compilerOptions.EmitEntryPoint.HasValue)
                    Trace.TraceInformation($"EmitEntryPoint:{compilerOptions.EmitEntryPoint}");
                if (compilerOptions.KeyFile != null)
                    Trace.TraceInformation($"KeyFile:{compilerOptions.KeyFile}");
                Trace.TraceInformation($"LanguageVersion:{compilerOptions.LanguageVersion}");
                if (compilerOptions.Optimize.HasValue)
                    Trace.TraceInformation($"Optimize:{compilerOptions.Optimize}");
                if (compilerOptions.Platform != null)
                    Trace.TraceInformation($"Platform:{compilerOptions.Platform}");
                if (compilerOptions.UseOssSigning.HasValue)
                    Trace.TraceInformation($"UseOssSigning:{compilerOptions.UseOssSigning}");
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
                Trace.TraceInformation($"RuntimePath:{(runtimeEnvironment.RuntimePath)}");
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
                Trace.TraceInformation($"Configuration:{(applicationEnvironment.Configuration)}");
                Trace.TraceInformation($"RuntimeFramework:{(applicationEnvironment.RuntimeFramework.FullName)}");
                Trace.TraceInformation($"RuntimeFrameworkIdentifier:{(applicationEnvironment.RuntimeFramework.Identifier)}");
                Trace.TraceInformation($"RuntimeFrameworkVersion:{applicationEnvironment.RuntimeFramework.Version}");
                Trace.Unindent();
            }

            // LibraryManager
            if (libraryManager != null)
            {
                Trace.TraceInformation($"LibraryManager");
                Trace.Indent();
                Instance._libraries = libraryManager.GetLibraries().ToArray();
                foreach (var lib in Instance._libraries)
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


            // set define
            Instance._executableDirectory = AppContext.BaseDirectory;
            Instance._executablePath = System.IO.Path.Combine(Instance._executableDirectory, "BJD.CoreCLR");
            Instance._productVersion = asmName.Version.ToString();

            if (runtimeEnvironment != null)
            {
                Instance._OperatingSystem = $"{runtimeEnvironment.OperatingSystem} {runtimeEnvironment.OperatingSystemVersion}";
                Instance.isWindows = (runtimeEnvironment.OperatingSystem == "Windows");
            }
            else
            {
                Instance._OperatingSystem = "Unknow";
                Instance.isWindows = false;
            }
            OnChangeOperationSystem();


            Trace.TraceInformation("Define.Initialize End");
        }

        public static void TestInitalize()
        {
            Initialize(null);
            var parent = System.IO.Path.GetDirectoryName(AppContext.BaseDirectory);
            Instance._executableDirectory = System.IO.Path.Combine(parent, "Bjd.CoreCLR");

        }

        public static event EventHandler ChangeOperationSystem;
        protected static void OnChangeOperationSystem()
        {
            if (ChangeOperationSystem != null)
                ChangeOperationSystem(Instance, EventArgs.Empty);
        }

        private static T GetService<T>(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) return default(T);
            return (T)serviceProvider.GetService(typeof(T));
        }

        internal static Library[] Libraries
        {
            get
            { return Instance._libraries; }
        }

        public static string Copyright()
        {
            return "Copyright(c) 1998/05.. by SIN/SapporoWorks";
        }
        public static string HostName()
        {
            InitLocalInformation();//メンバ変数「localAddress」の初期化
            return _localName;
        }
        public static string ApplicationName()
        {
            return "BlackJumboDog";
        }
        public static string ExecutablePath
        {
            get
            {
                return Instance._executablePath;
            }
        }
        public static string ExecutableDirectory
        {
            get
            {
                return Instance._executableDirectory;
            }
        }
        public static string ProductVersion
        {
            get
            {
                return Instance._productVersion;
            }
        }

        public static string OperatingSystem
        {
            get
            {
                return Instance._OperatingSystem;
            }
        }
        public static bool IsWindows
        {
            get
            {
                return Instance.isWindows;
            }
        }


        public static string Date()
        {
            DateTime dt = DateTime.Now;
            //return dt.ToShortDateString() + " " + dt.ToLongTimeString();
            var formatDate = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            var formatTime = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
            return dt.ToString(formatDate) + " " + dt.ToString(formatTime);
        }
        public static string ServerAddress()
        {
            InitLocalInformation();//メンバ変数「localAddress」の初期化
            if (_localAddress.Count > 0)
                return _localAddress[0];
            return "127.0.0.1";
        }
        public static List<string> ServerAddressList()
        {
            InitLocalInformation();//メンバ変数「localAddress」の初期化
            return _localAddress;
        }
        public static string WebHome()
        {
            return "http://www.sapporoworks.ne.jp/spw/";
        }
        public static string WebDocument()
        {
            return "http://www.sapporoworks.ne.jp/spw/?page_id=517";
        }
        public static string WebSupport()
        {
            return "http://www.sapporoworks.ne.jp/sbbs/sbbs.cgi?book=bjd";
        }


        static List<string> _localAddress;//アドレス
        static string _localName;//ホスト名
        static void InitLocalInformation()
        {
            if (_localAddress == null)
            {//プログラム起動から初めて呼び出されたとき、１度だけ実行される
                _localAddress = new List<string>();
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface nic in nics)
                {
                    if (nic.OperationalStatus != OperationalStatus.Up)
                        continue;
                    IPInterfaceProperties props = nic.GetIPProperties();
                    foreach (UnicastIPAddressInformation info in props.UnicastAddresses)
                    {
                        if (info.Address.AddressFamily == AddressFamily.InterNetwork)
                            _localAddress.Add(info.Address.ToString());
                    }
                }

                _localName = Dns.GetHostName();
            }
        }
    }
}
