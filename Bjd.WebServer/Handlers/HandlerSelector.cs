using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bjd;
using Bjd.Logs;
using Bjd.Configurations;
using Bjd.Utils;
using Bjd.WebServer.WebDav;
using System.Collections.Concurrent;
using Bjd.Common.IO;

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
        private StaticFileHandler _HandlerDefault;
        private MoveHandler _HandlerMove;
        private NotFoundHandler _HandlerNotFound;
        private SsiHandler _HandlerSsi;


        private bool useWebDav = false;
        private DatRecord[] webDavPathData;
        private bool useCgi = false;
        private DatRecord[] cgiPathData;
        private List<Configurations.WebServerOption.cgiPathClass> cgiPathDataConfig;
        private DatRecord[] aliaseListData;
        private List<Configurations.WebServerOption.aliaseListClass> aliaseListDataConfig;
        private string[] welcomeFileNames;
        private DatRecord[] cgiCmdData;
        private List<Configurations.WebServerOption.cgiCmdClass> cgiCmdDataConfig;
        private bool useSsi = false;
        private List<string> ssiExtList;
        private bool useHidden = false;
        private bool useDirectoryEnum = false;

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
            _HandlerDefault = new StaticFileHandler(kernel, conf, logger);
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
            cgiPathDataConfig = cgiPathData.Select(_ =>
                    new Configurations.WebServerOption.cgiPathClass
                    {
                        CgiPath = _.ColumnValueList[0].ToUpper(),
                        cgiDirectory = _.ColumnValueList[1]
                    }
                ).ToList();


            var aliaseList = (Dat)_conf.Get("aliaseList");
            aliaseListData = aliaseList.Where(_ => _.Enable).ToArray();
            aliaseListDataConfig = aliaseListData.Select(_ =>
                    new Configurations.WebServerOption.aliaseListClass
                    {
                        aliasName = _.ColumnValueList[0].ToUpper(),
                        aliasDirectory = _.ColumnValueList[1]
                    }
                ).ToList();

            welcomeFileNames = ((string)_conf.Get("welcomeFileName")).Split(',');

            var cgiCmd = (Dat)_conf.Get("cgiCmd");
            cgiCmdData = cgiCmd.Where(_ => _.Enable).ToArray();
            cgiCmdDataConfig = cgiCmdData.Select(_ =>
                    new Configurations.WebServerOption.cgiCmdClass
                    {
                        cgiExtension = _.ColumnValueList[0].ToUpper(),
                        Program = _.ColumnValueList[1].ToUpper()
                    }
                ).ToList();

            useSsi = (bool)_conf.Get("useSsi");
            ssiExtList = new List<string>(((string)_conf.Get("ssiExt")).Split(','));

            useHidden = (bool)_conf.Get("useHidden");

            useDirectoryEnum = (bool)_conf.Get("useDirectoryEnum");

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
            _kernel.Logger.DebugInformation($"HandlerSelector.InitFromUri {uri}");
            var result = new HandlerSelectorResult();
            result.DocumentRoot = DocumentRoot;
            result.PhysicalRootPath = PhysicalRootPath;

            Init(result, uri);

            SetHandler(result);
            _kernel.Logger.DebugInformation($"HandlerSelector.InitFromUri TargetKind {result.TargetKind} WebDavKind {result.WebDavKind}");
            return result;
        }
        //filenameによる初期化
        public HandlerSelectorResult InitFromFile(string file)
        {
            _kernel.Logger.DebugInformation($"HandlerSelector.InitFromFile {file}");
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
            _kernel.Logger.DebugInformation($"HandlerSelector.InitFromCmd {fullPath}");
            var result = new HandlerSelectorResult();
            result.DocumentRoot = DocumentRoot;
            result.PhysicalRootPath = PhysicalRootPath;

            result.ResetFullPath(fullPath);
            result.TargetKind = HandlerKind.Cgi;
            result.CgiCmd = "COMSPEC";

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
            if (useWebDav)
            {
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
            if (result.WebDavKind == WebDavKind.Non && useCgi)
            {

                foreach (var o in cgiPathDataConfig)
                {
                    useCgiPath = true;//有効なCGIパスの定義が存在する
                    var name = o.CgiPath;
                    var dir = o.cgiDirectory;

                    if (uri.ToUpper().IndexOf(name) != 0) continue;

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
                foreach (var o in aliaseListDataConfig)
                {
                    var name = o.aliasName;
                    var dir = o.aliasDirectory;
                    var uriUpper = uri.ToUpper();

                    if (uriUpper + "/" == name)
                    {
                        //ファイル指定されたターゲットがファイルではなくディレクトリの場合
                        result.TargetKind = HandlerKind.Move;
                        return;
                    }

                    if (uriUpper.IndexOf(name) != 0) continue;

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
            var newFullPath = Util.SwapChar('/', Path.DirectorySeparatorChar, result.DocumentRoot + uri);
            result.ResetFullPath(newFullPath);
            _kernel.Logger.DebugInformation($"Target.Init {result.FullPath}");

            /*************************************************/
            //ファイル指定されたターゲットがファイルではなくディレクトリの場合
            /*************************************************/
            if (result.WebDavKind == WebDavKind.Non)
            {
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
                        foreach (string welcomeFileName in welcomeFileNames)
                        {
                            var newPath = Path.Combine(Path.GetDirectoryName(result.FullPath), welcomeFileName);
                            if (!CachedFileExists.ExistsFile(newPath)) continue;

                            result.ResetFullPath(newPath, true);
                            _kernel.Logger.DebugInformation($"Target.Init welcomeFileName {result.FullPath}");
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
            if (!result.FileExists)
            {
                //ディレクトリとして存在しない場合
                if (!Directory.Exists(result.FullPath))
                {
                    //存在しない
                    result.TargetKind = HandlerKind.Non;
                    return;
                }

                //if ((bool)_conf.Get("useDirectoryEnum") && result.WebDavKind == WebDavKind.Non)
                if (useDirectoryEnum && result.WebDavKind == WebDavKind.Non)
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
                var ext = result.Ext;
                if (ext != null && ext.Length > 1)
                {
                    ext = ext.Substring(1);
                    var extUpper = ext.ToUpper();
                    foreach (var o in cgiCmdDataConfig)
                    {
                        if (o.cgiExtension == extUpper)
                        {
                            result.TargetKind = HandlerKind.Cgi;//CGIである
                            result.CgiCmd = o.Program;
                        }

                    }

                }
            }

            /*************************************************/
            // ターゲットがSSIかどうかの判断
            /*************************************************/
            if (result.WebDavKind == WebDavKind.Non)
            {
                if (result.TargetKind == HandlerKind.File && useSsi)
                {
                    //「SSIを使用する」場合
                    // SSI指定拡張子かどうかの判断
                    var ext = result.Ext;
                    if (ext != null && 1 <= ext.Length)
                    {
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
                //***************************************************************
                //  隠し属性のファイルへのアクセス制御
                //***************************************************************
                if (!useHidden)
                {
                    if ((result.Attr & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        result.TargetKind = HandlerKind.Non;
                    }
                }

            }

        }

    }

}

