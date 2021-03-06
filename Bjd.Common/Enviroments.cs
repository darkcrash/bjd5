﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd
{
    /// <summary>
    /// enviroments for kernel class
    /// </summary>
    public class Enviroments
    {
        public string ExecutableDirectory { get; internal set; } = Define.ExecutableDirectory;
        public string ConfigurationDirectory { get { return ExecutableDirectory; } }

        public string ServerAddress
        {
            get
            {
                return Define.ServerAddress();
            }
        }

        public string OperatingSystem
        {
            get
            {
                return Define.OperatingSystem;
            }
        }

        public string ApplicationName
        {
            get
            {
                return Define.ApplicationName;
            }
        }

        public string Copyright
        {
            get
            {
                return Define.Copyright;
            }
        }

        public string ProductVersion
        {
            get
            {
                return Define.ProductVersion;
            }
        }

    }
}
