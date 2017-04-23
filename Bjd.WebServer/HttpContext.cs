using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.WebServer.Authority;
using Bjd.WebServer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.WebServer
{
    public class HttpContext : IDisposable
    {
        public HttpConnectionContext Connection { get; private set; }
        //受信ヘッダ
        internal HttpRequestHeaders Header;
        //リクエストライン処理クラス
        internal HttpRequest Request;
        internal WebStream InputStream;
        internal WebStream OutputStream;
        public string Url;
        public int ResponseCode;
        internal Authorization Auth;
        internal string AuthName;
        internal HttpResponse Response;

        public HttpContext(HttpConnectionContext connection)
        {
            Connection = connection;
        }

        public void Clear()
        {
            if (InputStream != null)
                InputStream.Dispose();
            InputStream = null;
            if (OutputStream != null)
                OutputStream.Dispose();
            OutputStream = null;
        }

        public void Dispose()
        {
            Clear();
            Connection = null;
        }
    }
}
