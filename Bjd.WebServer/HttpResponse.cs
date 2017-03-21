using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using Bjd;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Threading;
using Bjd.WebServer.Handlers;

namespace Bjd.WebServer
{
    //********************************************************
    //ドキュメント生成クラス
    //********************************************************
    class HttpResponse
    {
        readonly Kernel _kernel;
        readonly Logger _logger;
        //readonly OneOption _oneOption;
        readonly Conf _conf;
        readonly SockTcp _sockTcp;
        readonly HttpContentType _contentType;

        //byte[] doc = new byte[0];
        readonly HttpResponseBody _body;

        //送信ヘッダ
        readonly HttpHeader _sendHeader;

        public bool SetRangeTo { get; set; }//Rangeヘッダで範囲（終わり）が指定された場合True

        public HttpResponse(Kernel kernel, Logger logger, Conf conf, SockTcp tcpObj, HttpContentType contentType)
        {
            _kernel = kernel;
            _logger = logger;
            //_oneOption = oneOption;
            _conf = conf;
            _sockTcp = tcpObj;
            _contentType = contentType;

            _kernel.Logger.DebugInformation($"HttpResponse..ctor");

            SetRangeTo = false;

            //送信ヘッダ初期化
            _sendHeader = new HttpHeader();
            _sendHeader.Replace("Server", Util.SwapStr("$v", kernel.Enviroment.ProductVersion, (string)_conf.Get("serverHeader")));
            _sendHeader.Replace("MIME-Version", "1.0");
            _sendHeader.Replace("Date", Util.UtcTime2Str(DateTime.UtcNow));

            _body = new HttpResponseBody(_kernel);
        }
        //Location:ヘッダを含むかどうか
        public bool SearchLocation()
        {
            return null != _sendHeader.GetVal("Location");
        }

        public void Clear()
        {
            _body.Clear();
            _sendHeader.Replace("Content-Length", _body.Length.ToString());
            //_sendHeader.Replace("Content-Length",string.Format("{0}",_body.Length));
        }

        //*********************************************************************
        // 送信
        //*********************************************************************
        //public void Send(bool keepAlive,ref bool life) {
        public void Send(bool keepAlive, ILife iLife)
        {
            //_kernel.Trace.TraceInformation($"Document.Send");
            _sendHeader.Replace("Connection", keepAlive ? "Keep-Alive" : "close");

            //ヘッダ送信
            _sockTcp.SendUseEncode(_sendHeader.GetBytes());//ヘッダ送信
            //本文送信
            if (_body.Length > 0)
            {
                //Ver5.0.0-b12
                //if(sendHeader.GetVal("Content-Type").ToLower().IndexOf("text")!=-1){
                var contentType = _sendHeader.GetVal("Content-Type");
                if (contentType != null && contentType.ToLower().IndexOf("text") != -1)
                {
                    _body.Send(_sockTcp, true, iLife);
                    //tcpObj.SendUseEncode(body.Get());   
                }
                else
                {
                    _body.Send(_sockTcp, false, iLife);
                    //tcpObj.SendNoEncode(body.Get());   
                }
            }
        }

        public void AddHeader(string key, string val)
        {
            _sendHeader.Append(key, Encoding.ASCII.GetBytes(val));
        }
        //Encoding.ASCII以外でエンコードしたい場合、こちらを使用する
        public void AddHeader(string key, byte[] val)
        {
            _sendHeader.Append(key, val);
        }

        //*********************************************************************
        // ドキュメント生成
        //*********************************************************************
        public bool CreateFromFile(HandlerSelectorResult result, long rangeFrom, long rangeTo)
        {
            ////_kernel.Trace.TraceInformation($"Document.CreateFromFile");
            //if (File.Exists(fileName))
            //{

            //    _body.Set(fileName, rangeFrom, rangeTo);

            //    //Ver5.4.0
            //    var l = _body.Length;
            //    if (SetRangeTo && rangeFrom == 0)
            //        l++;
            //    _sendHeader.Replace("Content-Length", l.ToString());
            //    _sendHeader.Replace("Content-Type", _contentType.Get(fileName));

            //    return true;
            //}
            //return false;

            if (!result.FileExists) return false;

            _body.Set(result.FullPath, rangeFrom, rangeTo);

            //Ver5.4.0
            var l = _body.Length;
            if (SetRangeTo && rangeFrom == 0)
                l++;
            _sendHeader.Replace("Content-Length", l.ToString());
            _sendHeader.Replace("Content-Type", _contentType.Get(result.FullPath));

            return true;

        }
        //public void SetDoc(byte [] buf){
        //    doc = new byte[buf.Length];
        //    Buffer.BlockCopy(buf,0,
        //    sendHeader.Replace("Content-Length",doc.Length.ToString());
        //}

        public void CreateFromXml(string str)
        {
            //_kernel.Trace.TraceInformation($"Document.CreateFromXml");

            _body.Set(Encoding.UTF8.GetBytes(str));
            _sendHeader.Replace("Content-Length", _body.Length.ToString());

            _sendHeader.Replace("Content-Type", "text/xml; charset=\"utf-8\"");
        }

        public void CreateFromSsi(byte[] output, string fileName)
        {
            //_kernel.Trace.TraceInformation($"Document.CreateFromSsi");
            _body.Set(output);
            _sendHeader.Replace("Content-Length", _body.Length.ToString());
            _sendHeader.Replace("Content-Type", _contentType.Get(fileName));
        }

        // CGIで得られた出力から、SendHeader及びdocを生成する
        public bool CreateFromCgi(byte[] output)
        {
            //_kernel.Trace.TraceInformation($"Document.CreateFromCgi");
            while (true)
            {
                var tmp = new byte[output.Length];
                Buffer.BlockCopy(output, 0, tmp, 0, output.Length);
                for (var i = 0; ; i++)
                {
                    if (tmp.Length <= i)
                        return false;
                    if (tmp[i] != 0x0a)
                        continue; //'\n'
                    var buf = new byte[i];
                    Buffer.BlockCopy(tmp, 0, buf, 0, i);
                    var line = Encoding.ASCII.GetString(buf);
                    line = line.TrimEnd('\r');

                    if (line.Length > 0)
                    {
                        var n = line.IndexOf(':');
                        if (0 <= n)
                        {
                            var tag = line.Substring(0, n);
                            var val = line.Substring(n + 1).Trim();
                            _sendHeader.Append(tag.Trim(), Encoding.ASCII.GetBytes(val));
                        }
                        else
                        {
                            goto end;
                        }
                    }
                    var len = output.Length - i - 1;
                    output = new byte[len];
                    Buffer.BlockCopy(tmp, i + 1, output, 0, len);

                    if (line.Length == 0)
                    {
                        _body.Set(output);
                        _sendHeader.Replace("Content-Length", _body.Length.ToString());
                        return true;
                    }
                    break;
                end:
                    _body.Set(output);
                    _sendHeader.Replace("Content-Length", _body.Length.ToString());
                    return true;
                }
            }
        }
        //Ver5.0.0-a20 エンコードはオプション設定に従う
        bool GetEncodeOption(out Encoding encoding, out string charset)
        {
            //_kernel.Trace.TraceInformation($"Document.GetEncodeOption");
            charset = "utf-8";
            encoding = Encoding.UTF8;
            var enc = _conf.Get("encode");
            if (enc == null) return false;
            if (enc is EncodingKind)
            {
                switch ((EncodingKind)enc)
                {
                    case EncodingKind.Utf8://UTF-8
                        return true;
                    case EncodingKind.ShiftJis://shift-jis
                        charset = "Shift-JIS";
                        encoding = CodePagesEncodingProvider.Instance.GetEncoding("shift-jis");
                        return true;
                    case EncodingKind.EucJp://euc
                        charset = "euc-jp";
                        encoding = CodePagesEncodingProvider.Instance.GetEncoding("euc-jp");
                        return true;
                }
            }
            else
            {
                var encString = enc.ToString();
                var confEncoding1 = CodePagesEncodingProvider.Instance.GetEncoding(encString);
                if (confEncoding1 != null) encoding = confEncoding1;
                var confEncoding2 = Encoding.GetEncoding(encString);
                if (confEncoding2 != null) encoding = confEncoding2;
            }
            //charset = enc;
            charset = encoding.WebName;
            return true;
        }


        public bool CreateFromErrorCode(HttpRequest request, int responseCode)
        {
            //_kernel.Trace.TraceInformation($"Document.CreateFromErrorCode");

            //Ver5.0.0-a20 エンコードはオプション設定に従う
            Encoding encoding;
            string charset;
            if (!GetEncodeOption(out encoding, out charset))
            {
                return false;
            }

            //レスポンス用の雛形取得
            var lines = Inet.GetLines((string)_conf.Get("errorDocument"));
            if (lines.Count == 0)
            {
                _logger.Set(LogKind.Error, null, 25, "");
                return false;
            }

            //バッファの初期化
            var sb = new StringBuilder();

            //文字列uriを出力用にサイタイズする（クロスサイトスクリプティング対応）
            var uri = Inet.Sanitize(request.Uri);

            //雛形を１行づつ読み込んでキーワード変換したのち出力用バッファに蓄積する
            foreach (string line in lines)
            {
                string str = line;
                str = Util.SwapStr("$MSG", WebServerUtil.StatusMessage(responseCode), str);
                str = Util.SwapStr("$CODE", responseCode.ToString(), str);
                str = Util.SwapStr("$SERVER", _kernel.Enviroment.ApplicationName, str);
                str = Util.SwapStr("$VER", request.Ver, str);
                str = Util.SwapStr("$URI", uri, str);
                sb.Append(str + "\r\n");
            }
            _body.Set(encoding.GetBytes(sb.ToString()));
            _sendHeader.Replace("Content-Length", _body.Length.ToString());
            _sendHeader.Replace("Content-Type", string.Format("text/html;charset={0}", charset));
            return true;

        }
        public bool CreateFromIndex(HttpRequest request, string path)
        {
            //_kernel.Trace.TraceInformation($"Document.CreateFromIndex");

            //Ver5.0.0-a20 エンコードはオプション設定に従う
            Encoding encoding;
            string charset;
            if (!GetEncodeOption(out encoding, out charset))
            {
                return false;
            }

            //レスポンス用の雛形取得
            var lines = Inet.GetLines((string)_conf.Get("indexDocument"));
            if (lines.Count == 0)
            {
                _logger.Set(LogKind.Error, null, 26, "");
                return false;
            }

            //バッファの初期化
            var sb = new StringBuilder();

            //文字列uriを出力用にサイタイズする（クロスサイトスクリプティング対応）
            var uri = Inet.Sanitize(request.Uri);


            //雛形を１行づつ読み込んでキーワード変換したのち出力用バッファに蓄積する
            foreach (string line in lines)
            {
                var str = line;
                if (str.IndexOf("<!--$LOOP-->") == 0)
                {
                    str = str.Substring(12);//１行の雛型

                    //一覧情報の取得(１行分はLineData)
                    var lineDataList = new List<LineData>();
                    var dir = request.Uri;
                    if (1 < dir.Length)
                    {
                        if (dir[dir.Length - 1] != '/')
                            dir = dir + '/';
                    }
                    //string dirStr = dir.Substring(0,dir.LastIndexOf('/'));
                    if (dir != "/")
                    {
                        //string parentDirStr = dirStr.Substring(0,dirStr.LastIndexOf('/') + 1);
                        //lineDataList.Add(new LineData(parentDirStr,"Parent Directory","&lt;DIR&gt;","-"));
                        lineDataList.Add(new LineData("../", "Parent Directory", "&lt;DIR&gt;", "-"));
                    }

                    var di = new DirectoryInfo(path);
                    foreach (var info in di.GetDirectories("*.*"))
                    {
                        var href = Uri.EscapeDataString(info.Name) + '/';
                        lineDataList.Add(new LineData(href, info.Name, "&lt;DIR&gt;", "-"));
                    }
                    foreach (var info in di.GetFiles("*.*"))
                    {
                        string href = Uri.EscapeDataString(info.Name);
                        lineDataList.Add(new LineData(href, info.Name, info.LastWriteTime.ToString(), info.Length.ToString()));
                    }

                    //位置情報を雛形で整形してStringBuilderに追加する
                    foreach (var lineData in lineDataList)
                        sb.Append(lineData.Get(str) + "\r\n");

                }
                else
                {//一覧行以外の処理
                    str = Util.SwapStr("$URI", uri, str);
                    str = Util.SwapStr("$SERVER", _kernel.Enviroment.ApplicationName, str);
                    str = Util.SwapStr("$VER", request.Ver, str);
                    sb.Append(str + "\r\n");
                }
            }
            _body.Set(encoding.GetBytes(sb.ToString()));
            _sendHeader.Replace("Content-Length", _body.Length.ToString());
            _sendHeader.Replace("Content-Type", string.Format("text/html;charset={0}", charset));
            return true;
        }
        //CreateIndexDocument()で使用される
        private class LineData
        {
            readonly string _href;
            readonly string _name;
            readonly string _date;
            readonly string _size;
            public LineData(string href, string name, string date, string size)
            {
                _href = Util.SwapStr(" ", "%20", href);
                _name = name;
                _date = date;
                _size = size;
            }
            //雛型(str)のキーワードを置き変えて１行のデータを取得する
            public string Get(string str)
            {
                var tmp = str;
                tmp = Util.SwapStr("$HREF", _href, tmp);
                tmp = Util.SwapStr("$NAME", _name, tmp);
                tmp = Util.SwapStr("$DATE", _date, tmp);
                tmp = Util.SwapStr("$SIZE", _size, tmp);
                return tmp;
            }
        }

    }

}
