﻿using System;
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
using System.Threading.Tasks;
using Bjd.Memory;

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
        readonly HttpContentType _contentType;

        //byte[] doc = new byte[0];
        readonly HttpResponseBody _body;

        public HttpResponseBody Body { get => _body; }

        ////送信ヘッダ
        ////readonly HttpHeaders _sendHeader;
        readonly HttpResponseHeaders _sendHeader;

        //public HttpResponseHeaders Headers => _sendHeader;

        public bool SetRangeTo { get; set; }//Rangeヘッダで範囲（終わり）が指定された場合True

        public HttpResponse(Kernel kernel, Logger logger, Conf conf, HttpContentType contentType, HttpResponseHeaders responseHeader)
        {
            _kernel = kernel;
            _logger = logger;
            //_oneOption = oneOption;
            _conf = conf;
            _contentType = contentType;
            _kernel.Logger.DebugInformation($"HttpResponse..ctor");

            ////送信ヘッダ初期化
            ////_sendHeader = new HttpHeader();
            ////_sendHeader.Replace("Server", Util.SwapStr("$v", kernel.Enviroment.ProductVersion, (string)_conf.Get("serverHeader")));
            ////_sendHeader.Replace("MIME-Version", "1.0");
            ////_sendHeader.Replace("Date", Util.UtcTime2Str(DateTime.UtcNow));
            //_sendHeader = new HttpResponseHeaders();
            _sendHeader = responseHeader;
            _sendHeader.Server.ValString = Util.SwapStr("$v", kernel.Enviroment.ProductVersion, (string)_conf.Get("serverHeader"));
            _sendHeader.MIMEVersion.ValString = "1.0";

            _body = new HttpResponseBody(_kernel);
        }

        private ISocket _sockTcp;

        public void Initialize(ISocket tcpObj)
        {
            SetRangeTo = false;
            _sockTcp = tcpObj;
            Clear();
        }

        ////Location:ヘッダを含むかどうか
        //public bool SearchLocation()
        //{
        //    //return null != _sendHeader.GetVal("Location");
        //    return _sendHeader.Location.Enabled;
        //}

        public void Clear()
        {
            _body.Clear();
            //_sendHeader.Replace("Content-Length", _body.Length.ToString());
            //_sendHeader.Replace("Content-Length",string.Format("{0}",_body.Length));
            //_sendHeader.Replace("Date", Util.UtcTime2Str(DateTime.UtcNow));
            _sendHeader.SetContentLength(_body.Length);
        }


        public async Task SendAsync(bool keepAlive, ILife iLife)
        {
            _sendHeader.Connection.ValString = keepAlive ? "Keep-Alive" : "close";
            _sendHeader.Date.ValString = Util.UtcTime2String();

            //ヘッダ送信
            using (var headerBuffer = _sendHeader.GetBuffer())
            {
                //_sockTcp.SendAsync(_sendHeader.GetBuffer());
                await _sockTcp.SendAsync(headerBuffer);
            }

            //本文送信
            if (_body.Length > 0)
            {
                //var contentType = _sendHeader.ContentType.ValString;
                ////if (contentType != null && contentType.ToLower().IndexOf("text") != -1)
                //if (contentType != null && contentType.IndexOf("text", StringComparison.CurrentCultureIgnoreCase) != -1)
                if (_sendHeader.ContentType.Enabled && _sendHeader.IsText)
                {
                    await _body.SendAsync(_sockTcp, true, iLife);
                }
                else
                {
                    await _body.SendAsync(_sockTcp, false, iLife);
                }
            }
        }

        public void AddHeader(string key, string val)
        {
            _sendHeader.Append(key, val);
        }

        //*********************************************************************
        // ドキュメント生成
        //*********************************************************************
        public bool CreateFromFile(HandlerSelectorResult result, long rangeFrom, long rangeTo)
        {
            ////_kernel.Trace.TraceInformation($"Document.CreateFromFile");

            if (!result.FileExists) return false;

            _body.Set(result.FullPath, rangeFrom, rangeTo);

            //Ver5.4.0
            var l = _body.Length;
            if (SetRangeTo && rangeFrom == 0)
                l++;
            //_sendHeader.Replace("Content-Length", l.ToString());
            //_sendHeader.Replace("Content-Type", _contentType.Get(result.FullPath));
            _sendHeader.SetContentLength(l);
            _sendHeader.SetContentType(_contentType.Get(result.FullPath));

            return true;

        }

        public void CreateFromXml(string str)
        {
            //_kernel.Trace.TraceInformation($"Document.CreateFromXml");

            _body.Set(Encoding.UTF8.GetBytes(str));
            //_sendHeader.Replace("Content-Length", _body.Length.ToString());
            //_sendHeader.Replace("Content-Type", "text/xml; charset=\"utf-8\"");
            _sendHeader.SetContentLength(_body.Length);
            _sendHeader.SetContentType("text/xml; charset=\"utf-8\"");
        }

        public void CreateFromSsi(byte[] output, string fileName)
        {
            //_kernel.Trace.TraceInformation($"Document.CreateFromSsi");
            _body.Set(output);
            //_sendHeader.Replace("Content-Length", _body.Length.ToString());
            //_sendHeader.Replace("Content-Type", _contentType.Get(fileName));
            _sendHeader.SetContentLength(_body.Length);
            _sendHeader.SetContentType(_contentType.Get(fileName));
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
                            //_sendHeader.Append(tag.Trim(), Encoding.ASCII.GetBytes(val));
                            _sendHeader.Append(tag.Trim(), val);
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
                        //_sendHeader.Replace("Content-Length", _body.Length.ToString());
                        _sendHeader.SetContentLength(_body.Length);
                        return true;
                    }
                    break;
                    end:
                    _body.Set(output);
                    //_sendHeader.Replace("Content-Length", _body.Length.ToString());
                    _sendHeader.SetContentLength(_body.Length);
                    return true;
                }
            }
        }
        //Ver5.0.0-a20 エンコードはオプション設定に従う
        public bool GetEncodeOption(out Encoding encoding, out string charset)
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
            //_sendHeader.Replace("Content-Length", _body.Length.ToString());
            //_sendHeader.Replace("Content-Type", string.Format("text/html;charset={0}", charset));
            _sendHeader.SetContentLength(_body.Length);
            _sendHeader.SetContentType(string.Format("text/html;charset={0}", charset));
            return true;

        }


    }

}
