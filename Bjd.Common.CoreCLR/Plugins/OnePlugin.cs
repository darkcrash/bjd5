﻿using System;
using System.IO;
using Bjd.Net;
using Bjd.Options;
using Bjd.Servers;
using Bjd.Utils;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;

namespace Bjd.Plugins
{
    public class OnePlugin : IDisposable
    {
        readonly string _path;
        readonly RuntimeLibrary _lib;

        public OnePlugin(string path)
        {
            _path = path;

            var str = Path.GetFileNameWithoutExtension(_path);
            this.Name = this.GetName(str);

        }
        public OnePlugin(RuntimeLibrary lib)
        {
            _lib = lib;

            this.Name = this.GetName(lib.Name);

        }

        private string GetName(string str)
        {
            if (str.StartsWith("Bjd."))
            {
                str = str.Remove(0, 4);
            }
            var index = str.IndexOf("Server.CoreCLR");
            if (index != 0 && (str.Length - index) == 14)
            {
                str = str.Substring(0, index);
            }
            return str;
        }

        public string Name { get; }

        //プラグイン固有のOptionインスタンスの生成
        public OneOption CreateOption(Kernel kernel, String className, string nameTag)
        {
            //return (OneOption)Util.CreateInstance(kernel, _path, className, new object[] { kernel, _path, nameTag });
            //return (OneOption)Util.CreateInstance(kernel, _lib, className, new object[] { kernel, _lib.Path, nameTag });
            return (OneOption)Util.CreateInstance(kernel, _lib, className, new object[] { kernel, kernel.Enviroment.ExecutableDirectory, nameTag });
        }

        public OneServer CreateServer(Kernel kernel, Conf conf, OneBind oneBind)
        {
            //return (OneServer)Util.CreateInstance(kernel, _path, "Server", new Object[] { kernel, conf, oneBind });
            return (OneServer)Util.CreateInstance(kernel, _lib, "Server", new Object[] { kernel, conf, oneBind });
        }

        public void Dispose() { }

    }
}