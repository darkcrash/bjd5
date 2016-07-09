using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.WebServer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.WebServer
{
    public class HttpConnectionContext
    {
        public SockTcp Connection;
        public Ip RemoteIp;
        //レスポンスが終了したとき接続を切断しないで継続する 
        public bool keepAlive = true;
        //1回目の通信でバーチャルホストの検索を実施する
        public bool checkVirtual = true;
        public Logs.Logger Logger;

        public HttpRequestContext CreateRequestContext()
        {
            var context = new HttpRequestContext();
            context.ConnectionContext = this;
            context.HttpRequest = new Request(Logger, Connection);
            context.HttpHeader = new Header();
            context.InputStream = null;
            context.OutputStream = new WebStream(-1);
            return context;
        }
    }
}
