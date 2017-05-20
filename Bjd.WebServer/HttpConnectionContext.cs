using Bjd.Memory;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.WebServer.Memory;
using Bjd.WebServer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bjd.Configurations;
using Bjd.Logs;

namespace Bjd.WebServer
{
    public class HttpConnectionContext : IDisposable, IPoolBuffer
    {
        private HttpContextPool _pool;
        private Kernel _Kernel;
        private Logs.Logger _Logger;
        private Conf _Conf;
        private HttpContentType _ContentType;

        //レスポンスが終了したとき接続を切断しないで継続する 
        public bool KeepAlive = true;
        //1回目の通信でバーチャルホストの検索を実施する
        public bool CheckVirtual = true;

        public SockTcp Connection;
        public Ip RemoteIp;
        private HttpContext requestContext;

        public Logs.Logger Logger => _Logger;


        public HttpConnectionContext(HttpContextPool pool, Kernel kernel, Logger logger, Conf conf, HttpContentType contentType)
        {
            _pool = pool;
            _Kernel = kernel;
            _Logger = logger;
            _Conf = conf;
            _ContentType = contentType;
        }

        public HttpContext GetRequestContext()
        {
            if (requestContext == null)
            {
                requestContext = new HttpContext(this);
                requestContext.RequestHeader = new HttpRequestHeaders();
                requestContext.Request = new HttpRequest(_Kernel, _Logger);
                requestContext.ResponseHeader = new HttpResponseHeaders();
                requestContext.Response = new HttpResponse(_Kernel, _Logger, _Conf, _ContentType, requestContext.ResponseHeader);
            }
            requestContext.Clear();
            requestContext.Request.Initialize(Connection);
            requestContext.RequestHeader.Clear();
            requestContext.Auth = null;
            requestContext.AuthName = null;
            requestContext.InputStream = null;
            requestContext.OutputStream = null;
            //requestContext.Response = null;
            requestContext.Response.Initialize(Connection);
            requestContext.ResponseCode = 0;
            requestContext.ResponseHeader.Clear();
            requestContext.Url = null;
            return requestContext;
        }



        public void Dispose()
        {
            Connection = null;
            RemoteIp = null;
            _pool.PoolInternal(this);
        }

        public void Initialize()
        {
            // null
            KeepAlive = true;
            CheckVirtual = true;
            Connection = null;
            RemoteIp = null;
        }

        public void DisposeInternal()
        {
            Connection = null;
            RemoteIp = null;
            _Logger = null;
            requestContext?.RequestHeader?.Dispose();
            requestContext?.ResponseHeader?.Dispose();
        }
    }
}
