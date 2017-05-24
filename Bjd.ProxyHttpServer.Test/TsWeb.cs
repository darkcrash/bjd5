using System;
using System.Net.Sockets;
using System.IO;
using Microsoft.Net.Http.Server;
using System.Threading.Tasks;

namespace ProxyHttpServerTest
{
    public class TsWeb : IDisposable
    {

        readonly string _documentRoot;
        readonly WebListener _listener;


        public TsWeb(ref int port, string documentRoot)
        {
            _documentRoot = documentRoot;

            // プレフィックスの登録
            while (true)
            {
                try
                {
                    var setting = new WebListenerSettings();
                    UrlPrefix pre = UrlPrefix.Create("http", "127.0.0.1", port, "/");
                    setting.UrlPrefixes.Add(pre);
                    setting.ThrowWriteExceptions = true;
                    setting.Authentication.AllowAnonymous = true;

                    _listener = new WebListener(setting);
                    _listener.Start();
                    break;
                }
                catch (Microsoft.Net.Http.Server.WebListenerException)
                {
                    port++;
                    continue;
                }
            }

            Listen();

        }

        private void Listen()
        {
            var t = _listener.AcceptAsync();
            t.ContinueWith(_ => OnRequested(_));

        }

        public void Dispose()
        {
            _listener.Dispose();
        }
        //  要求を受信した時に実行するメソッド。
        public async void OnRequested(Task<RequestContext> result)
        {
            if (result.IsFaulted) throw result.Exception;

            var listener = _listener;
            if (!listener.IsListening)
            {
                return;
            }

            var ctx = result.Result;
            var req = ctx.Request;
            var res = ctx.Response;

            var path = _documentRoot + req.Path.Replace('/', Path.DirectorySeparatorChar);

            // ファイルが存在すればレスポンス・ストリームに書き出す
            if (File.Exists(path))
            {
                var cancel = new System.Threading.CancellationTokenSource();
                byte[] content = File.ReadAllBytes(path);
                await res.SendFileAsync(path, 0, content.Length, cancel.Token);
            }
            else
            {
                res.StatusCode = 404;
                res.Body.Flush();
            }
            ctx.Dispose();
        }


    }
}