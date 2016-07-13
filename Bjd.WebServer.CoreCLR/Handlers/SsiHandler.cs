﻿using Bjd.Logs;
using Bjd.Options;
using Bjd.Utils;
using Bjd.WebServer.Outside;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bjd.WebServer.Handlers
{
    internal class SsiHandler : IHandler
    {
        Kernel _kernel;
        Conf _conf;
        public SsiHandler(Kernel kernel, Conf conf)
        {
            _kernel = kernel;
            _conf = conf;
        }

        public bool Request(HttpRequestContext context, HandlerSelectorResult result)
        {
            var connection = context.Connection;
            var Logger = connection.Logger;

            connection.KeepAlive = false;//デフォルトで切断

            //環境変数作成
            var env = new Env(_kernel, _conf, context.Request, context.Header, connection.Connection, result.FullPath);

            // 詳細ログ
            Logger.Set(LogKind.Detail, connection.Connection, 18, string.Format("{0} {1}", result.CgiCmd, Path.GetFileName(result.FullPath)));

            //SSI
            var ssi = new Ssi(_kernel, Logger, _conf, connection.Connection, context.Request, context.Header);
            if (!ssi.Exec(result, env, context.OutputStream))
            {
                // エラー出力
                Logger.Set(LogKind.Error, connection.Connection, 22, MLang.GetString(context.OutputStream.GetBytes()));
                context.ResponseCode = 500;
                return true;

            }
            context.Response.CreateFromSsi(context.OutputStream.GetBytes(), result.FullPath);

            return true;

        }
    }
}