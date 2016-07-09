using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.WebServer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.WebServer
{
    public class HttpConnectionContext : IDisposable
    {
        public SockTcp Connection;
        public Ip RemoteIp;
        //レスポンスが終了したとき接続を切断しないで継続する 
        public bool KeepAlive = true;
        //1回目の通信でバーチャルホストの検索を実施する
        public bool CheckVirtual = true;
        public Logs.Logger Logger;

        public HttpRequestContext CreateRequestContext()
        {
            var context = new HttpRequestContext();
            context.Connection = this;
            context.Request = new HttpRequest(Logger, Connection);
            context.Header = new HttpHeader();
            context.InputStream = null;
            context.OutputStream = new WebStream(-1);
            return context;
        }

        public void Dispose()
        {
            Connection = null;
            RemoteIp = null;
            Logger = null;
        }
    }
}
