using Bjd.Memory;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.WebServer.Memory;
using Bjd.WebServer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.WebServer
{
    public class HttpConnectionContext : IDisposable, IPoolBuffer
    {

        public SockTcp Connection;
        public Ip RemoteIp;
        //レスポンスが終了したとき接続を切断しないで継続する 
        public bool KeepAlive = true;
        //1回目の通信でバーチャルホストの検索を実施する
        public bool CheckVirtual = true;
        public Kernel Kernel;
        public Logs.Logger Logger;

        private HttpRequestContext requestContext;

        private HttpContextPool _pool;


        public HttpConnectionContext(HttpContextPool pool)
        {
            _pool = pool;
        }

        public HttpRequestContext GetRequestContext()
        {
            if (requestContext == null)
            {
                requestContext = new HttpRequestContext(this);
                requestContext.Request = new HttpRequest(Kernel, Logger);
                requestContext.Header = new HttpHeader();
            }
            requestContext.Clear();
            requestContext.Request.Initialize(this.Connection);
            requestContext.Header.Clear();
            requestContext.Auth = null;
            requestContext.AuthName = null;
            requestContext.ContentType = null;
            requestContext.InputStream = null;
            requestContext.OutputStream = null;
            requestContext.Response = null;
            requestContext.ResponseCode = 0;
            requestContext.Url = null;
            return requestContext;
        }


        public void Dispose()
        {
            _pool.PoolInternal(this);
        }

        public void Initialize()
        {
            // null
        }

        public void DisposeInternal()
        {
            Connection = null;
            RemoteIp = null;
            Logger = null;
        }
    }
}
