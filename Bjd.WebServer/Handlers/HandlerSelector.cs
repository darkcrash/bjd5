using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bjd;
using Bjd.Logs;
using Bjd.Configurations;
using Bjd.Utils;
using Bjd.WebServer.WebDav;

namespace Bjd.WebServer.Handlers
{
    /*******************************************************/
    //対象（ファイル）に関する各種の情報をまとめて扱うクラス
    /*******************************************************/
    class HandlerSelector
    {
        //readonly OneOption _oneOption;
        readonly Kernel _kernel;
        readonly Conf _conf;
        readonly Logger _logger;
        readonly string PhysicalRootPath;
        public string DocumentRoot { get; private set; }

        private CgiHandler _HandlerCgi;
        private DirectoryListHandler _HandlerDirectoryList;
        private DefaultHandler _HandlerDefault;
        private MoveHandler _HandlerMove;
        private NotFoundHandler _HandlerNotFound;
        private SsiHandler _HandlerSsi;


        private bool useWebDav = false;
        private DatRecord[] webDavPathData;
        private bool useCgi = false;
        private DatRecord[] cgiPathData;
        private DatRecord[] aliaseListData;
        private string[] welcomeFileNames;
        private DatRecord[] cgiCmdData;
        private bool useSsi = false;
        private List<string> ssiExtList;
        private bool useHidden = false;

        public HandlerSelector(Kernel kernel, Conf conf, Logger logger)
        {
            //kernel.Trace.TraceInformation($"Target..ctor ");
            //_oneOption = oneOption;
            _kernel = kernel;
            _conf = conf;
            _logger = logger;
            PhysicalRootPath = kernel.Enviroment.ExecutableDirectory;
            DocumentRoot = (string)_conf.Get("documentRoot");
            if (!Path.IsPathRooted(DocumentRoot))
            {
                DocumentRoot = Path.GetFullPath(Path.Combine(kernel.Enviroment.ExecutableDirectory, DocumentRoot));
            }
            if (!Directory.Exists(DocumentRoot))
            {
                _kernel.Logger.TraceError($"HandlerSelector..ctor DocumentRoot not exists ");
                DocumentRoot = null;//ドキュメントルート無効
            }

            _HandlerCgi = new CgiHandler(kernel, conf);
            _HandlerDefault = new DefaultHandler(kernel, conf, logger);
            _HandlerDirectoryList = new DirectoryListHandler();
            _HandlerMove = new MoveHandler();
            _HandlerNotFound = new NotFoundHandler();
            _HandlerSsi = new SsiHandler(kernel, conf);

            useWebDav = (bool)_conf.Get("useWebDav");
            var webDavPath = (Dat)_conf.Get("webDavPath");
            webDavPathData = webDavPath.Where(_ => _.Enable).ToArray();

            useCgi = (bool)_conf.Get("useCgi");
            var cgiPath = (Dat)_conf.Get("cgiPath");
            cgiPathData = webDavPath.Where(_ => _.Enable).ToArray();

            var aliaseList = (Dat)_conf.Get("aliaseList");
            aliaseListData = aliaseList.Where(_ => _.Enable).ToArray();

            welcomeFileNames = ((string)_conf.Get("welcomeFileName")).Split(',');

            var cgiCmd = (Dat)_conf.Get("cgiCmd");
            cgiCmdData = cgiCmd.Where(_ => _.Enable).ToArray();

            useSsi = (bool)_conf.Get("useSsi");
            ssiExtList = new List<string>(((string)_conf.Get("ssiExt")).Split(','));

            useHidden = (bool)_conf.Get("useHidden");

        }

        private void SetHandler(HandlerSelectorResult result)
        {
            switch (result.TargetKind)
            {
                case HandlerKind.AspNetCore:
                    break;
                case HandlerKind.Cgi:
                    result.Handler = _HandlerCgi;
                    break;
                case HandlerKind.Dir:
                    result.Handler = _HandlerDirectoryList;
                    break;
                case HandlerKind.File:
                    result.Handler = _HandlerDefault;
                    break;
                case HandlerKind.Move:
                    result.Handler = _HandlerMove;
                    break;
                case HandlerKind.Non:
                    result.Handler = _HandlerNotFound;
                    break;
                case HandlerKind.Ssi:
                    result.Handler = _HandlerSsi;
                    break;
            }
        }

        /*************************************************/
        // 初期化
        /*************************************************/
        //uriによる初期化
        public HandlerSelectorResult InitFromUri(string uri)
        {
            _kernel.Logger.TraceInformation($"HandlerSelector.InitFromUri {uri}");
            var result = new HandlerSelectorResult();
            result.DocumentRoot = DocumentRoot;
            result.PhysicalRootPath = PhysicalRootPath;

            Init(result, uri);

            SetHandler(result);
            _kernel.Logger.TraceInformation($"HandlerSelector.InitFromUri TargetKind {result.TargetKind} WebDavKind {result.WebDavKind}");
            return result;
        }
        //filenameによる初期化
        public HandlerSelectorResult InitFromFile(string file)
        {
            _kernel.Logger.TraceInformation($"HandlerSelector.InitFromFile {file}");
            var result = new HandlerSelectorResult();
            result.DocumentRoot = DocumentRoot;
            result.PhysicalRootPath = PhysicalRootPath;

            var tmp = file.ToLower();// fullPathからuriを生成する
            var root = DocumentRoot.ToLower();
            if (tmp.IndexOf(root) != 0)
                return result;
            var uri = file.Substring(root.Length);
            //uri = Util.SwapChar('\\', '/', uri);
            uri = Util.SwapChar(Path.DirectorySeparatorChar, '/', uri);
            if (string.IsNullOrEmpty(uri))
                uri = "/";

            Init(result, uri);

            SetHandler(result);
            return result;
        }

        //コマンドによる初期化
        public HandlerSelectorResult InitFromCmd(string fullPath)
        {
            _kernel.Logger.TraceInformation($"HandlerSelector.InitFromCmd {fullPath}");
            var result = new HandlerSelectorResult();
            result.DocumentRoot = DocumentRoot;
            result.PhysicalRootPath = PhysicalRootPath;

            result.TargetKind = HandlerKind.Cgi;
            result.CgiCmd = "COMSPEC";
            result.FullPath = fullPath;

            SetHandler(result);
            return result;
        }
        private void Init(HandlerSelectorResult result, string uri)
        {

            result.Uri = uri;

            result.TargetKind = HandlerKind.File;//通常ファイルであると仮置きする
            var enableCgiPath = false;//フォルダがCGI実行可能かどうか
            result.WebDavKind = WebDavKind.Non;//Ver5.1.x WebDAV対象外であることを仮置きする

            //****************************************************************
            //WebDavパスにヒットした場合、uri及びドキュメントルートを修正する
            //****************************************************************
            //if ((bool)_conf.Get("useWebDav"))
            if (useWebDav)
            {
                //var db = (Dat)_conf.Get("webDavPath");
                //foreach (var o in db)
                foreach (var o in webDavPathData)
                {
                    //if (!o.Enable) continue;

                    var name = o.ColumnValueList[0];
                    var write = Convert.ToBoolean(o.ColumnValueList[1]);//書き込み許可
                    var dir = o.ColumnValueList[2];
                    if (uri.ToUpper().IndexOf(name.ToUpper()) == 0)
                    {
                        if (name.Length >= 1)
                        {
                            uri = uri.Substring(name.Length - 1);
                        }
                        else
                        {
                            uri = "/";
                        }
                        result.DocumentRoot = dir;
                        //WevDavパス定義にヒットした場合
                        result.WebDavKind = (write) ? WebDavKind.Write : WebDavKind.Read;
                        break;
                    }

                }

                // 最後が/で無い場合は、保管してヒットするかどうかを確認する
                if (uri[uri.Length - 1] != '/')
                {
                    var exUri = uri + "/";
                    //foreach (var o in db)
                    foreach (var o in webDavPathData)
                    {
                        //if (!o.Enable) continue;
                        var name = o.ColumnValueList[0];
                        var write = Convert.ToBoolean(o.ColumnValueList[1]);//書き込み許可
                        var dir = o.ColumnValueList[2];
                        if (exUri.ToUpper().IndexOf(name.ToUpper()) == 0)
                        {
                            if (name.Length >= 1)
                            {
                                uri = exUri.Substring(name.Length - 1);
                            }
                            else
                            {
                                uri = "/";
                            }
                            result.Uri = exUri;//リクエストに既に/が付いていたように動作させる
                            result.DocumentRoot = dir;
                            //WevDavパス定義にヒットした場合
                            result.WebDavKind = (write) ? WebDavKind.Write : WebDavKind.Read;
                            break;
                        }

                    }
                }


            }

            //****************************************************************
            //CGIパスにヒットした場合、uri及びドキュメントルートを修正する
            //****************************************************************
            bool useCgiPath = false;//CGIパス定義が存在するかどうかのフラグ
            //if (result.WebDavKind == WebDavKind.Non && (bool)_conf.Get("useCgi"))
            if (result.WebDavKind == WebDavKind.Non && useCgi)
            {
                //foreach (var o in (Dat)_conf.Get("cgiPath"))
                foreach (var o in cgiPathData)
                {
                    //if (!o.Enable) continue;

                    useCgiPath = true;//有効なCGIパスの定義が存在する
                    var name = o.ColumnValueList[0];
                    var dir = o.ColumnValueList[1];

                    if (uri.ToUpper().IndexOf(name.ToUpper()) != 0) continue;

                    if (name.Length >= 1)
                    {
                        uri = uri.Substring(name.Length - 1);
                    }
                    else
                    {
                        uri = "/";
                    }
                    result.DocumentRoot = dir;
                    //CGIパス定義にヒットした場合
                    enableCgiPath = true;//CGI実行が可能なフォルダである
                    break;

                }

                if (!useCgiPath)
                {//有効なCGIパス定義が無い場合は、
                    enableCgiPath = true;//CGI実行が可能なフォルダである
                }

            }


            //****************************************************************
            //別名にヒットした場合、uri及びドキュメントルートを修正する
            //****************************************************************
            if (result.WebDavKind == WebDavKind.Non && !useCgiPath && uri.Length >= 1)
            {
                //foreach (var o in (Dat)_conf.Get("aliaseList"))
                foreach (var o in aliaseListData)
                {
                    //if (!o.Enable) continue;

                    var name = o.ColumnValueList[0];
                    var dir = o.ColumnValueList[1];

                    if (uri.ToUpper() + "/" == name.ToUpper())
                    {
                        //ファイル指定されたターゲットがファイルではなくディレクトリの場合
                        result.TargetKind = HandlerKind.Move;
                        return;
                    }

                    if (uri.ToUpper().IndexOf(name.ToUpper()) != 0) continue;

                    if (name.Length >= 1)
                    {
                        uri = uri.Substring(name.Length - 1);
                    }
                    else
                    {
                        uri = "/";
                    }

                    result.DocumentRoot = dir;
                    break;

                }
            }

            /*************************************************/
            // uriから物理的なパス名を生成する
            /*************************************************/
            //FullPath = Util.SwapChar('/', '\\', DocumentRoot + uri);
            result.FullPath = Util.SwapChar('/', Path.DirectorySeparatorChar, result.DocumentRoot + uri);
            _kernel.Logger.TraceInformation($"Target.Init {result.FullPath}");

            /*************************************************/
            //ファイル指定されたターゲットがファイルではなくディレクトリの場合
            /*************************************************/
            if (result.WebDavKind == WebDavKind.Non)
            {
                //if (FullPath[FullPath.Length - 1] != '\\')
                if (result.FullPath[result.FullPath.Length - 1] != Path.DirectorySeparatorChar && Directory.Exists(result.FullPath))
                {
                    result.TargetKind = HandlerKind.Move;
                    return;
                }
            }
            else
            {
                if (result.TargetKind == HandlerKind.File && Directory.Exists(result.FullPath))
                {
                    result.TargetKind = HandlerKind.Dir;
                    return;
                }
            }

            /*************************************************/
            // welcomeファイルのセット
            /*************************************************/
            //Uriでファイル名が指定されていない場合で、当該ディレクトリにwelcomeFileNameが存在する場合
            //ファイル名として使用する
            if (result.WebDavKind == WebDavKind.Non)
            {
                //Ver5.1.3
                try
                {
                    if (Path.GetFileName(result.FullPath) == "")
                    {
                        //var tmp = ((string)_conf.Get("welcomeFileName")).Split(',');
                        //foreach (string welcomeFileName in tmp)
                        foreach (string welcomeFileName in welcomeFileNames)
                        {
                            //var newPath = Path.GetDirectoryName(FullPath) + "\\" + welcomeFileName;
                            var newPath = Path.Combine(Path.GetDirectoryName(result.FullPath), welcomeFileName);
                            if (!File.Exists(newPath)) continue;

                            result.FullPath = newPath;
                            _kernel.Logger.TraceInformation($"Target.Init welcomeFileName {result.FullPath}");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Set(LogKind.Error, null, 37, string.Format("uri={0} FullPath={1} {2}", uri, result.FullPath, ex.Message));
                    result.TargetKind = HandlerKind.Non;
                    return;
                }

            }
            /*************************************************/
            //ターゲットはファイルとして存在するか
            /*************************************************/
            if (!File.Exists(result.FullPath))
            {
                //ディレクトリとして存在しない場合
                if (!Directory.Exists(result.FullPath))
                {
                    //存在しない
                    result.TargetKind = HandlerKind.Non;
                    return;
                }

                if ((bool)_conf.Get("useDirectoryEnum") && result.WebDavKind == WebDavKind.Non)
                {
                    result.TargetKind = HandlerKind.Dir;
                    return;
                }
            }

            /*************************************************/
            // 拡張子判断
            /*************************************************/
            // 「CGI実行が可能なフォルダの場合　拡張子がヒットすればターゲットはCGIである
            if (result.WebDavKind == WebDavKind.Non && enableCgiPath)
            {
                var ext = Path.GetExtension(result.FullPath);
                if (ext != null && ext.Length > 1)
                {
                    ext = ext.Substring(1);
                    //foreach (var o in (Dat)_conf.Get("cgiCmd"))
                    foreach (var o in cgiCmdData)
                    {
                        if (!o.Enable) continue;

                        var cgiExt = o.ColumnValueList[0];
                        var cgiCmd = o.ColumnValueList[1];
                        if (cgiExt.ToUpper() == ext.ToUpper())
                        {
                            result.TargetKind = HandlerKind.Cgi;//CGIである
                            result.CgiCmd = cgiCmd;
                        }

                    }
                }
            }

            /*************************************************/
            // ターゲットがSSIかどうかの判断
            /*************************************************/
            if (result.WebDavKind == WebDavKind.Non)
            {
                //if (result.TargetKind == HandlerKind.File && (bool)_conf.Get("useSsi"))
                if (result.TargetKind == HandlerKind.File && useSsi)
                {
                    //「SSIを使用する」場合
                    // SSI指定拡張子かどうかの判断
                    var ext = Path.GetExtension(result.FullPath);
                    if (ext != null && 1 <= ext.Length)
                    {
                        //var ssiExtList = new List<string>(((string)_conf.Get("ssiExt")).Split(','));
                        if (0 <= ssiExtList.IndexOf(ext.Substring(1)))
                        {
                            //ターゲットファイルにキーワードが含まれているかどうかの確認
                            var physicalFullPath = Path.Combine(this.PhysicalRootPath, result.FullPath);
                            if (0 <= Util.IndexOf(physicalFullPath, "<!--#"))
                            {
                                result.TargetKind = HandlerKind.Ssi;
                            }
                        }
                    }
                }
            }
            /*************************************************/
            // アトリビュート及びインフォメーションの取得
            /*************************************************/
            if (result.TargetKind == HandlerKind.File || result.TargetKind == HandlerKind.Ssi)
            {
                //ファイルアトリビュートの取得
                result.Attr = File.GetAttributes(result.FullPath);
                //ファイルインフォメーションの取得
                result.FileInfo = new FileInfo(result.FullPath);

                //***************************************************************
                //  隠し属性のファイルへのアクセス制御
                //***************************************************************
                //if (!(bool)_conf.Get("useHidden"))
                if (!useHidden)
                {
                    if ((result.Attr & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        result.TargetKind = HandlerKind.Non;
                    }
                }

            }


        }

        //リストにヒットした場合、uri及びドキュメントルートを書き換える
        //Ver5.0.0-a13修正
        /*
         * bool Aliase(Dat2 db) {
            int index = uri.Substring(1).IndexOf('/');//先頭の'/'以降で最初に現れる'/'を検索する
            if (0 < index) {
                string topDir = uri.Substring(1, index);
                foreach (OneLine oneLine in db.Lines) {
                    if (oneLine.Enabled) {
                        string name = (string)oneLine.ValList[0].Obj;
                        string dir = (string)oneLine.ValList[1].Obj;
                        if (name.ToLower() == topDir.ToLower()) {
                            DocumentRoot = dir;
                            uri = uri.Substring(index);
                            return true;//変換（ヒット）した
                        }
                    }
                }
            }
            return false;
        }
         * */

    }
}

