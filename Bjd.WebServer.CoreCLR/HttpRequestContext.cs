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
        public HttpConnectionContext ConnectionContext;
        //受信ヘッダ
        internal Header HttpHeader;
        //リクエストライン処理クラス
        internal Request HttpRequest;
        internal WebStream InputStream;
        internal WebStream OutputStream;
        public string Url;
        public int ResponseCode;
        public ContentType ContentType;

        public void Dispose()
        {
            ConnectionContext = null;

            if (InputStream != null)
                InputStream.Dispose();
            if (OutputStream != null)
                OutputStream.Dispose();
        }
    }
}
