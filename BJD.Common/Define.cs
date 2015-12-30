using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;

namespace Bjd
{
    public class Define
    {

        private Define() { }
        static Define Instance = new Define();
        string _executablePath;
        string _executableDirectory;
        string _productVersion;
        string _Copyright;

        public static void Initialize(IServiceProvider sb)
        {
            Trace.WriteLine("Define.Initialize Start");

            // get service
            var runtimeServices = GetService<Microsoft.Extensions.PlatformAbstractions.IRuntimeServices>(sb);
            var libraryManager = GetService<ILibraryManager>(sb);
            var applicationEnvironment = GetService<IApplicationEnvironment>(sb);
            var compilerOptions = GetService<ICompilerOptions>(sb);
            var runtimeEnvironment = GetService<Microsoft.Extensions.PlatformAbstractions.IRuntimeEnvironment>(sb);

            if (runtimeServices != null)
            {
                Trace.WriteLine($"RuntimeServices");
                Trace.Indent();
                foreach(var sv in runtimeServices.Services)
                {
                    Trace.WriteLine($"{(sv.FullName)}");
                }
                Trace.Unindent();
            }

            if (compilerOptions != null)
            {
                Trace.WriteLine($"CompilerOptions");
                Trace.Indent();
                if (compilerOptions.AllowUnsafe.HasValue)
                    Trace.WriteLine($"AllowUnsafe:{(compilerOptions.AllowUnsafe)}");
                if (compilerOptions.DelaySign.HasValue)
                    Trace.WriteLine($"DelaySign:{compilerOptions.DelaySign}");
                if (compilerOptions.EmitEntryPoint.HasValue)
                    Trace.WriteLine($"EmitEntryPoint:{compilerOptions.EmitEntryPoint}");
                if (compilerOptions.KeyFile != null)
                    Trace.WriteLine($"KeyFile:{compilerOptions.KeyFile}");
                Trace.WriteLine($"LanguageVersion:{compilerOptions.LanguageVersion}");
                if (compilerOptions.Optimize.HasValue)
                    Trace.WriteLine($"Optimize:{compilerOptions.Optimize}");
                if (compilerOptions.Platform != null)
                    Trace.WriteLine($"Platform:{compilerOptions.Platform}");
                if (compilerOptions.UseOssSigning.HasValue)
                    Trace.WriteLine($"UseOssSigning:{compilerOptions.UseOssSigning}");
                if (compilerOptions.WarningsAsErrors.HasValue)
                    Trace.WriteLine($"WarningsAsErrors:{compilerOptions.WarningsAsErrors}");
                Trace.Unindent();
            }


            if (runtimeEnvironment != null)
            {
                Trace.WriteLine($"RuntimeEnvironment");
                Trace.Indent();
                Trace.WriteLine($"OperatingSystem:{(runtimeEnvironment.OperatingSystem)}");
                Trace.WriteLine($"OperatingSystemVersion:{(runtimeEnvironment.OperatingSystemVersion)}");
                Trace.WriteLine($"RuntimeArchitecture:{(runtimeEnvironment.RuntimeArchitecture)}");
                Trace.WriteLine($"RuntimePath:{(runtimeEnvironment.RuntimePath)}");
                Trace.WriteLine($"RuntimeType:{(runtimeEnvironment.RuntimeType)}");
                Trace.WriteLine($"RuntimeVersion:{(runtimeEnvironment.RuntimeVersion)}");
                Trace.Unindent();
            }


            // current directory
            var dir = System.IO.Directory.GetCurrentDirectory();
            var asm = typeof(Define).GetTypeInfo().Assembly;
            var asmName = asm.GetName();
            Trace.WriteLine($"CurrentDirectory:{dir}");
            Trace.WriteLine($"AppContext.BaseDirectory:{AppContext.BaseDirectory}");
            Trace.WriteLine($"Assembly.Location:{asm.Location}");


            // set define
            Instance._executableDirectory = AppContext.BaseDirectory;
            Instance._executablePath = System.IO.Path.Combine(AppContext.BaseDirectory, "BJD.CoreCLR");
            Instance._productVersion = asmName.Version.ToString();

            Trace.WriteLine("Define.Initialize End");
        }

        private static T GetService<T>(IServiceProvider serviceProvider)
        {
            return (T)serviceProvider.GetService(typeof(T));
        }

        public static string Copyright()
        {
            return "Copyright(c) 1998/05.. by SIN/SapporoWorks";
        }
        public static string HostName()
        {
            InitLocalInformation();//�����o�ϐ��ulocalAddress�v�̏�����
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
            InitLocalInformation();//�����o�ϐ��ulocalAddress�v�̏�����
            if (_localAddress.Count > 0)
                return _localAddress[0];
            return "127.0.0.1";
        }
        public static List<string> ServerAddressList()
        {
            InitLocalInformation();//�����o�ϐ��ulocalAddress�v�̏�����
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


        static List<string> _localAddress;//�A�h���X
        static string _localName;//�z�X�g��
        static void InitLocalInformation()
        {
            if (_localAddress == null)
            {//�v���O�����N�����珉�߂ČĂяo���ꂽ�Ƃ��A�P�x�������s�����
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
