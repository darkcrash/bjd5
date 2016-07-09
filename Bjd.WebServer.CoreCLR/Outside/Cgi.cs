using Bjd.WebServer.Handlers;
using Bjd.WebServer.IO;
using System;
using System.IO;
using System.Text;

namespace Bjd.WebServer.Outside
{
    class Cgi
    {

        public bool Exec(HandlerSelectorResult result, string param, Env env, WebStream inputStream, out WebStream outputStream, int cgiTimeout)
        {
            var cmd = result.CgiCmd;
            if (cmd == null)
            {
                outputStream = new WebStream(-1);
                outputStream.Add(Encoding.ASCII.GetBytes("cmd==null"));
                return false;
            }
            if (cmd.ToUpper().IndexOf("COMSPEC") == 0)
            {
                cmd = Environment.GetEnvironmentVariable("ComSpec");
                // /cがウインドウクローズのために必要
                param = "/c " + param;
            }
            else if (cmd.ToUpper().IndexOf("CMD.EXE") != -1)
            {
                cmd = result.FullPath;
            }
            else
            {
                param = string.Format("{0} {1}", Path.GetFileName(result.FullPath), param);
            }

            var execProcess = new ExecProcess(cmd, param, Path.GetDirectoryName(result.FullPath), env);
            return execProcess.Start(inputStream, out outputStream);
        }

    }
}
