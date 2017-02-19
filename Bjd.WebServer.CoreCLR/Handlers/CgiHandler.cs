using Bjd.Logs;
using Bjd.Options;
using Bjd.WebServer.Outside;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bjd.WebServer.Handlers
{
    internal class CgiHandler : IHandler
    {
        Kernel _kernel;
        Conf _conf;
        Cgi cgi;
        int cgiTimeout;
        public CgiHandler(Kernel kernel, Conf conf)
        {
            _kernel = kernel;
            _conf = conf;
            cgi = new Cgi();
            cgiTimeout = (int)_conf.Get("cgiTimeout");
        }


        public bool Request(HttpRequestContext context, HandlerSelectorResult selector)
        {
            var connection = context.Connection;
            var Logger = connection.Logger;

            connection.KeepAlive = false;//デフォルトで切断

            //環境変数作成
            var env = new Env(_kernel, _conf, context.Request, context.Header, connection.Connection, selector.FullPath);

            // 詳細ログ
            Logger.Set(LogKind.Detail, connection.Connection, 18, string.Format("{0} {1}", selector.CgiCmd, Path.GetFileName(selector.FullPath)));

            if (!cgi.Exec(selector, context.Request.Param, env, context.InputStream, out context.OutputStream, cgiTimeout))
            {
                // エラー出力
                var errStr = Encoding.ASCII.GetString(context.OutputStream.GetBytes());

                Logger.Set(LogKind.Error, connection.Connection, 16, errStr);
                context.ResponseCode = 500;
                //goto SEND;
                return true;

            }

            //***************************************************
            // NPH (Non-Parsed Header CGI)スクリプト  nph-で始まる場合、サーバ処理（レスポンスコードやヘッダの追加）を経由しない
            //***************************************************
            if (Path.GetFileName(selector.FullPath).IndexOf("nph-") == 0)
            {
                _kernel.Trace.TraceInformation("CgiHandler.Request nph");
                connection.Connection.SendUseEncode(context.OutputStream.GetBytes());//CGI出力をそのまま送信する
                return false;
            }

            // CGIで得られた出力から、本体とヘッダを分離する
            if (!context.Response.CreateFromCgi(context.OutputStream.GetBytes()))
            {
                return false;
            }
            _kernel.Trace.TraceInformation("CgiHandler.CreateFromCgi ");

            // cgi出力で、Location:が含まれる場合、レスポンスコードを302にする
            if (context.Response.SearchLocation())//Location:ヘッダを含むかどうか
                context.ResponseCode = 302;

            return true;

        }
    }
}
