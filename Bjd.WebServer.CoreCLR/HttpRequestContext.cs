using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.WebServer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.WebServer
{
    public class HttpRequestContext : IDisposable
    {
        public HttpConnectionContext Connection;
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

        public void Dispose()
        {
            Connection = null;

            if (InputStream != null)
                InputStream.Dispose();
            if (OutputStream != null)
                OutputStream.Dispose();
        }
    }
}
