using System;
using System.Collections.Generic;
using System.Globalization;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.Utils;

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
            foreach(var method in Methods)
            {
                MethodsDic.Add(method.ToString().ToUpper(), method);
                MethodsDic.Add(method.ToString(), method);
            }
        }

        public HttpRequest(Kernel kernel, Logger logger, SockTcp sockTcp)
        {

            //Logger出力用(void Log()の中でのみ使用される)
            _kernel = kernel;
            _logger = logger;
            _sockObj = sockTcp;

            Method = HttpMethod.Unknown;
            Uri = "";
            Param = "";
            Ver = "";
            LogStr = "";

        }

        readonly Kernel _kernel;
        readonly Logger _logger;
        readonly SockTcp _sockObj;//Logger出力用

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
        public bool Init(string requestStr)
        {
            _kernel.Logger.DebugInformation($"Request.Init");

            //既存のデータが残っている場合は削除してから受信にはいる
            Uri = "";
            Param = "";
            Ver = "";
            Method = HttpMethod.Unknown;//Ver5.1.x

            //string str = sockTcp.AsciiRecv(timeout,OperateCrlf.Yes,ref life);
            //if (str == null)
            //    return false;

            // メソッド・URI・バージョンに分割

            //リクエスト行がURLエンコードされている場合は、その文字コードを取得する
            try
            {
                LogStr = System.Uri.UnescapeDataString(requestStr);//リクエスト文字列をそのまま保存する（ログ表示用）
            }
            catch
            {
                LogStr = UrlDecode(requestStr);
            }

            var tmp = requestStr.Split(' ');
            if (tmp.Length != 3)
            {
                Log(LogKind.Secure, 0, string.Format("Length={0} {1}", tmp.Length, requestStr));//リクエストの解釈に失敗しました（不正なリクエストの可能性があるため切断しました
                return false;
            }
            if (tmp[0] == "" || tmp[1] == "" || tmp[1] == "")
            {
                Log(LogKind.Secure, 0, string.Format("{0}", requestStr));//リクエストの解釈に失敗しました（不正なリクエストの可能性があるため切断しました
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
            var reqMethod = tmp[0];
            if (MethodsDic.ContainsKey(reqMethod))
            {
                Method = MethodsDic[reqMethod];
            }

            if (Method == HttpMethod.Unknown)
            {
                Log(LogKind.Secure, 1, string.Format("{0}", requestStr));//�T�|�[�g�O�̃��\�b�h�ł��i������p���ł��܂���j
                return false;
            }
            //バージョンの取得
            if (tmp[2] == "HTTP/0.9" || tmp[2] == "HTTP/1.0" || tmp[2] == "HTTP/1.1")
            {
                Ver = tmp[2];
            }
            else
            {
                Log(LogKind.Secure, 2, string.Format("{0}", requestStr));//�T�|�[�g�O�̃o�[�W�����ł��i������p���ł��܂���j
                return false;
            }
            //パラメータの取得
            var tmp2 = tmp[1].Split('?');
            if (2 <= tmp2.Length)
                Param = tmp2[1];
            // Uri の中の%xx をデコード
            try
            {
                Uri = System.Uri.UnescapeDataString(tmp2[0]);
                Uri = UrlDecode(tmp2[0]);
            }
            catch
            {
                Uri = UrlDecode(tmp2[0]);
            }

            //Ver5.1.3-b5 制御文字が含まれる場合、デコードに失敗している
            for (var i = 0; i < Uri.Length; i++)
            {
                if (18 >= Uri[i])
                {
                    Uri = tmp2[0];
                    break;
                }
            }


            //Uriに/が続く場合の対処
            Uri = Util.SwapStr("//", "/", Uri);

            //Ver5.8.8
            if (Uri == "" || Uri[0] != '/')
            {
                Log(LogKind.Secure, 5, LogStr);
                return false;
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
    }
}
