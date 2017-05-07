using Bjd.Configurations;
using Bjd.Logs;
using Bjd.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bjd.WebServer.Handlers
{
    internal class DirectoryListHandler : IHandler
    {
        Kernel _kernel;
        Conf _conf;
        Logger _logger;

        List<string> indexDocument;

        public DirectoryListHandler(Kernel kernel, Conf conf, Logger logger)
        {
            _kernel = kernel;
            _conf = conf;
            _logger = logger;
            indexDocument = Inet.GetLines((string)_conf.Get("indexDocument"));
        }


        public bool Request(HttpContext context, HandlerSelectorResult selector)
        {
            //インデックスドキュメントを生成する
            if (!CreateFromIndex(context, context.Request, selector.FullPath))
                return false;
            return true;
        }

        public bool CreateFromIndex(HttpContext context, HttpRequest request, string path)
        {
            //_kernel.Trace.TraceInformation($"Document.CreateFromIndex");

            //Ver5.0.0-a20 エンコードはオプション設定に従う
            Encoding encoding;
            string charset;
            if (!context.Response.GetEncodeOption(out encoding, out charset))
            {
                return false;
            }

            //レスポンス用の雛形取得
            var lines = indexDocument;
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

            context.Response.Body.Set(encoding.GetBytes(sb.ToString()));
            context.Response.Headers.SetContentLength(context.Response.Body.Length);
            context.Response.Headers.SetContentType(string.Format("text/html;charset={0}", charset));
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
