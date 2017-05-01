using System;
using System.Collections.Generic;
using System.Globalization;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Memory;
using System.Linq;

namespace Bjd.WebServer
{
    //********************************************************
    //リクエスト/レスポンス処理クラス
    //********************************************************
    internal class HttpRequest
    {
        static HttpMethod[] Methods = (HttpMethod[])Enum.GetValues(typeof(HttpMethod));
        static Dictionary<string, HttpMethod> MethodsDic = new Dictionary<string, HttpMethod>();

        static HttpRequest()
        {
            foreach (var method in Methods)
            {
                MethodsDic.Add(method.ToString().ToUpper(), method);
                MethodsDic.Add(method.ToString(), method);
            }
        }

        public HttpRequest(Kernel kernel, Logger logger)
        {
            //Logger出力用(void Log()の中でのみ使用される)
            _kernel = kernel;
            _logger = logger;

        }

        private void Clear()
        {
            Method = HttpMethod.Unknown;
            Uri = "";
            Param = "";
            Ver = "";
            LogStr = "";
        }

        public void Initialize(SockTcp sockObj)
        {
            Clear();
            _sockObj = sockObj;
        }

        readonly Kernel _kernel;
        readonly Logger _logger;
        public SockTcp _sockObj { get; private set; }

        void Log(LogKind logKind, int messageNo, string msg)
        {
            if (_logger != null)
            {
                _logger.Set(logKind, _sockObj, messageNo, msg);
            }
        }

        public HttpMethod Method { get; private set; }
        public string Uri { get; private set; }
        public string Param { get; private set; }
        public string Ver { get; private set; }
        public string LogStr { get; private set; }

        //データ取得（内部データは、初期化される）
        //public bool Recv(int timeout,sockTcp sockTcp,ref bool life) {
        public bool Init(CharsData requestStr)
        {
            _kernel.Logger.DebugInformation("Request.Init");

            //既存のデータが残っている場合は削除してから受信にはいる
            Uri = "";
            Param = "";
            Ver = "";
            Method = HttpMethod.Unknown;//Ver5.1.x


            // メソッド・URI・バージョンに分割

            //リクエスト行がURLエンコードされている場合は、その文字コードを取得する
            //try
            //{
            //    LogStr = System.Uri.UnescapeDataString(requestStr);//リクエスト文字列をそのまま保存する（ログ表示用）
            //}
            //catch
            //{
            //    LogStr = UrlDecode(requestStr);
            //}
            LogStr = requestStr.ToString();

            //var tmp = requestStr.Split(' ');
            var tmp = requestStr.Split(' ').ToArray();
            try
            {


                if (tmp.Length != 3)
                {
                    //Log(LogKind.Secure, 0, string.Format("Length={0} {1}", tmp.Length, requestStr));//リクエストの解釈に失敗しました（不正なリクエストの可能性があるため切断しました
                    Log(LogKind.Secure, 0, string.Format("Length={0} {1}", tmp.Length, LogStr));//リクエストの解釈に失敗しました（不正なリクエストの可能性があるため切断しました
                    return false;
                }

                if (tmp[0] == "" || tmp[1] == "" || tmp[2] == "")
                {
                    //Log(LogKind.Secure, 0, string.Format("{0}", requestStr));//リクエストの解釈に失敗しました（不正なリクエストの可能性があるため切断しました
                    Log(LogKind.Secure, 0, LogStr);//リクエストの解釈に失敗しました（不正なリクエストの可能性があるため切断しました
                    return false;
                }

                // メソッドの取得
                //foreach (HttpMethod m in Enum.GetValues(typeof(HttpMethod)))
                //foreach (HttpMethod m in Methods)
                //{
                //    if (tmp[0].ToUpper() == m.ToString().ToUpper())
                //    {
                //        Method = m;
                //        break;
                //    }
                //}
                var reqMethod = tmp[0].ToString();


                if (MethodsDic.ContainsKey(reqMethod))
                {
                    Method = MethodsDic[reqMethod];
                }

                if (Method == HttpMethod.Unknown)
                {
                    Log(LogKind.Secure, 1, LogStr);
                    return false;
                }

                //バージョンの取得
                Ver = tmp[2].ToString();
                if (Ver != "HTTP/1.1" && Ver != "HTTP/1.0" && Ver != "HTTP/0.9")
                {
                    // サポート外のバージョンです（処理を継続できません）
                    Log(LogKind.Secure, 2, LogStr);
                    return false;
                }

                //パラメータの取得
                //var tmp2 = tmp[1].Split('?');
                var tmp2 = tmp[1].Split('?').ToArray();
                try
                {
                    if (2 <= tmp2.Length)
                    {
                        Param = tmp2[1].ToString();
                    }

                    var uriTemp = tmp2[0].ToString();


                    // Uri の中の%xx をデコード
                    try
                    {
                        Uri = System.Uri.UnescapeDataString(uriTemp);
                        Uri = UrlDecode(uriTemp);
                    }
                    catch
                    {
                        Uri = UrlDecode(uriTemp);
                    }

                    //Ver5.1.3-b5 制御文字が含まれる場合、デコードに失敗している
                    for (var i = 0; i < Uri.Length; i++)
                    {
                        if (18 >= Uri[i])
                        {
                            Uri = uriTemp;
                            break;
                        }
                    }

                }
                finally
                {
                    for (var i = 0; i < tmp2.Length; i++) tmp2[i].Dispose();
                }


                //Uriに/が続く場合の対処
                Uri = Util.SwapStr("//", "/", Uri);

                //Ver5.8.8
                if (Uri == "" || Uri[0] != '/')
                {
                    Log(LogKind.Secure, 5, LogStr);
                    return false;
                }

            }
            finally
            {
                for (var i = 0; i < tmp.Length; i++) tmp[i].Dispose();
            }

            return true;
        }

        string UrlDecode(string s)
        {
            //Ver5.9.0
            try
            {
                return WebServerUtil.UrlDecode(s);
            }
            catch (Exception ex)
            {
                //Ver5.9.0
                _logger.Set(LogKind.Error, null, 0, string.Format("Exception ex.Message={0} [WebServer.Request.UrlDecode({1})]", ex.Message, s));
                return s;
            }
        }

        //レスポンスの送信
        //public void Send(sockTcp sockTcp,int code) {
        //    string str = string.Format("{0} {1} {2}", Ver, code,StatusMessage(code));
        //    sockTcp.AsciiSend(str,OperateCrlf.Yes);//レスポンス送信
        //    logger.Set(LogKind.Detail,sockTcp,4,str);//ログ

        //}
        //レスポンス行の作成
        public string CreateResponse(int code)
        {
            return string.Format("{0} {1} {2}", Ver, code, WebServerUtil.StatusMessage(code));
        }

        public CharsData CreateResponseChars(int code)
        {
            var len = 10 + 4 + 55;
            var chars = CharsPool.GetMaximum(len);
            chars.Append(Ver);
            chars.Append(' ');
            CachedIntConverter.AppendFormatString(chars, 1, code);
            chars.Append(' ');
            chars.Append(WebServerUtil.StatusMessage(code));
            return chars;
        }

    }
}
