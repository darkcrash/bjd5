using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bjd;
using Bjd.Net;
using Bjd.Options;
using Bjd.Net.Sockets;

namespace Bjd.WebServer.Outside
{
    class Env {
        //Ver5.6.2
        //StringDictionaryが大文字、小文字を区別できないので変更する
        //readonly StringDictionary ar = new StringDictionary();
        readonly List<EnvKeyValue> _ar = new List<EnvKeyValue>();

        //public Env(Kernel kernel, Request request, Header recvHeader, System.Net.IPAddress remoteAddress, string remoteHostName, string fileName) {
        public Env(Kernel kernel,Conf conf, HttpRequest request, HttpHeader recvHeader,SockTcp tcpObj,string fileName) {



            //Ver5.6.2
            var documetnRoot = (string)conf.Get("documentRoot");
            _ar.Add(new EnvKeyValue("DOCUMENT_ROOT",documetnRoot));
            var serverAdmin = (string)conf.Get("serverAdmin");
            _ar.Add(new EnvKeyValue("SERVER_ADMIN", serverAdmin));
            

            _ar.Add(new EnvKeyValue("SystemRoot", Environment.GetEnvironmentVariable("SystemRoot")));
            _ar.Add(new EnvKeyValue("Path", Environment.GetEnvironmentVariable("Path")));
            //Ver5.6.2追加
            _ar.Add(new EnvKeyValue("COMSPEC", Environment.GetEnvironmentVariable("COMSPEC")));
            _ar.Add(new EnvKeyValue("PATHEXT", Environment.GetEnvironmentVariable("PATHEXT")));
            _ar.Add(new EnvKeyValue("WINDIR", Environment.GetEnvironmentVariable("windir")));



            _ar.Add(new EnvKeyValue("SERVER_SOFTWARE", string.Format("{0}/{1} (Windows)", kernel.Enviroment.ApplicationName, kernel.Enviroment.ProductVersion)));

            _ar.Add(new EnvKeyValue("REQUEST_METHOD", request.Method.ToString().ToUpper()));
            _ar.Add(new EnvKeyValue("REQUEST_URI", request.Uri));
            if (request.Uri == "/") { // ルートディレクトリか？
                _ar.Add(new EnvKeyValue("SCRIPT_NAME", Path.GetFileName(fileName)));  // Welcomeファイルを設定する
            } else { // URIで指定されたCGIを設定する
                _ar.Add(new EnvKeyValue("SCRIPT_NAME", request.Uri));
            }
            _ar.Add(new EnvKeyValue("SERVER_PROTOCOL", request.Ver));
            _ar.Add(new EnvKeyValue("QUERY_STRING", request.Param));

            _ar.Add(new EnvKeyValue("REMOTE_HOST", tcpObj.RemoteHostname));
            //Ver5.6.2
            //ar.Add(new OneEnv("REMOTE_ADDR", tcpObj.RemoteAddr.IPAddress.ToString()));
            var addr = (tcpObj.RemoteAddress != null) ? tcpObj.RemoteAddress.Address.ToString() : "";
            _ar.Add(new EnvKeyValue("REMOTE_ADDR", addr));

            //Ver5.6.2
            int port = (tcpObj.RemoteAddress!=null)?tcpObj.RemoteAddress.Port:0;
            _ar.Add(new EnvKeyValue("REMOTE_PORT", port.ToString()));
            port = (tcpObj.LocalAddress != null) ? tcpObj.LocalAddress.Port : 0;
            _ar.Add(new EnvKeyValue("SERVER_PORT", port.ToString()));
            addr = (tcpObj.LocalAddress != null) ? tcpObj.LocalAddress.Address.ToString() : "";
            _ar.Add(new EnvKeyValue("SERVER_ADDR", addr));


            //Ver5.6.2
            SetEnvValue(recvHeader, _ar, "accept-charset", "HTTP_ACCEPT_CHARSET");
            SetEnvValue(recvHeader, _ar, "accept-encoding", "HTTP_ACCEPT_ENCODING");
            SetEnvValue(recvHeader, _ar, "accept-language", "HTTP_ACCEPT_LANGUAGE");
            
            SetEnvValue(recvHeader, _ar, "User-Agent", "HTTP_USER_AGENT");
            SetEnvValue(recvHeader, _ar, "Content-Type", "CONTENT_TYPE");
            SetEnvValue(recvHeader, _ar, "host", "SERVER_NAME");

            SetEnvValue(recvHeader, _ar, "Content-Length", "CONTENT_LENGTH");
            SetEnvValue(recvHeader, _ar, "AuthUser", "REMOTE_USER");

            //PathInfo/PathTranslatedの取得と環境変数へのセットについて再考察
            SetEnvValue(recvHeader, _ar, "PathInfo", "PATH_INFO");
            SetEnvValue(recvHeader, _ar, "PathTranslated", "PATH_TRANSLATED");

            _ar.Add(new EnvKeyValue("SCRIPT_FILENAME", fileName));

            //HTTP_で環境変数をセットしないヘッダの（除外）リスト
            var exclusionList = new List<string>{
                "accept-charset",
                "accept-encoding",
                "accept-language",
                "authorization",
                "content-length",
                "content-type",
                "date",
                "expires",
                "from",
                "host",
                "if-modified-since",
                "if-match",
                "if-none-match",
                "if-range",
                "if-unmodified-since",
                "last-modified",
                "pragma",
                "range",
                "remote-user",
                "remote-host-wp",
                "transfer-encoding",
                "upgrade",
                "user-agent"
            };
            //Ver5.6.2
            //exclusionList.Add("connection");

            //DEBUG
            //recvHeader.Append("accept", Encoding.ASCII.GetBytes("ABC"));
            foreach (var line in recvHeader) {
                //取得したタグが除外リストにヒットしない場合
                //HTTP_を付加して環境変数にセットする
                if (exclusionList.IndexOf(line.Key.ToLower()) < 0) {
                    //5.5.4重複による例外を回避
                    //ar.Add("HTTP_" + line.Key.ToUpper(), recvHeader.GetVal(line.Key));
                    var tag = "HTTP_" + line.Key.ToUpper();
                    //if (null == ar[tag]) {
                    //    ar.Add(tag, recvHeader.GetVal(line.Key));
                    //}
                    bool find = _ar.Any(a => a.Key == tag);
                    if(!find){
                        _ar.Add(new EnvKeyValue(tag, recvHeader.GetVal(line.Key)));
                    }

                }
            }
        }

        //public IEnumerator<DictionaryEntry> GetEnumerator() {
        //    foreach (DictionaryEntry p in ar)
        //        yield return p;
        //}
        public IEnumerator<EnvKeyValue> GetEnumerator(){
            return ((IEnumerable<EnvKeyValue>) _ar).GetEnumerator();
        }

        //***************************************************
        //環境変数の設定
        //***************************************************
        //void SetEnvValue(Header recvHeader, StringDictionary env, string headerTag, string envTag) {
        //    string value = recvHeader.GetVal(headerTag);
        //    if (value != null)
        //        env.Add(envTag, value);
        //}
        
        //***************************************************
        //環境変数の設定
        //***************************************************
        void SetEnvValue(HttpHeader recvHeader, List<EnvKeyValue> env, string headerTag, string envTag) {
            string value = recvHeader.GetVal(headerTag);
            if (value != null)
                env.Add(new EnvKeyValue(envTag, value));
        }

    }
}
