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
        //readonly HttpListener _listener;
        readonly WebListener _listener;


        public TsWeb(int port, string documentRoot)
        {
            _documentRoot = documentRoot;


            string prefix = string.Format("http://127.0.0.1:{0}/", port); // 受け付けるURL
            //_listener = new HttpListener();
            _listener = new WebListener();
            _listener.UrlPrefixes.Add(prefix); // プレフィックスの登録

            _listener.Start();
            //_listener.BeginGetContext(OnRequested, _listener);

            Listen();

        }

        private void Listen()
        {
            var t = _listener.GetContextAsync();
            t.ContinueWith(_ => OnRequested(null));

        }

        public void Dispose()
        {
            //_listener.Abort();
            //listener.Stop();
            //_listener.Close();
            _listener.Dispose();
        }
        //  要求を受信した時に実行するメソッド。
        //public void OnRequested(IAsyncResult result)
        //{
        //    //var listener = (HttpListener)result.AsyncState;
        //    var listener = _listener;
        //    if (!listener.IsListening)
        //    {
        //        return;
        //    }

        //    var ctx = listener.EndGetContext(result);
        //    var req = ctx.Request;
        //    var res = ctx.Response;

        //    var path = _documentRoot + req.RawUrl.Replace("/", "\\");

        //    // ファイルが存在すればレスポンス・ストリームに書き出す
        //    if (File.Exists(path))
        //    {
        //        byte[] content = File.ReadAllBytes(path);
        //        res.OutputStream.Write(content, 0, content.Length);
        //    }
        //    else
        //    {
        //        res.StatusCode = 404;
        //    }
        //    res.Close();
        //}
        public async void OnRequested(Task<RequestContext> result)
        {
            var listener = _listener;
            if (!listener.IsListening)
            {
                return;
            }

            var ctx = result.Result;
            var req = ctx.Request;
            var res = ctx.Response;

            //var path = _documentRoot + req.RawUrl.Replace("/", "\\");
            var path = _documentRoot + req.Path.Replace("/", "\\");

            // ファイルが存在すればレスポンス・ストリームに書き出す
            if (File.Exists(path))
            {
                var cancel = new System.Threading.CancellationTokenSource();
                byte[] content = File.ReadAllBytes(path);
                //res.OutputStream.Write(content, 0, content.Length);
                await res.SendFileAsync(path, 0, content.Length, cancel.Token);
            }
            else
            {
                res.StatusCode = 404;
                res.Body.Flush();
            }
            //res.Close();
            ctx.Dispose();
        }


    }
}