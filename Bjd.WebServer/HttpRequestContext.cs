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
    public class HttpRequestContext : IDisposable
    {
        public HttpConnectionContext Connection { get; private set; }
        //受信ヘッダ
        internal HttpHeader Header;
        //リクエストライン処理クラス
        internal HttpRequest Request;
        internal WebStream InputStream;
        internal WebStream OutputStream;
        public string Url;
        public int ResponseCode;
        public HttpContentType ContentType;
        internal Authorization Auth;
        internal string AuthName;
        internal HttpResponse Response;

        public HttpRequestContext(HttpConnectionContext connection)
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
