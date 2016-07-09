using System;
using System.Collections.Generic;
using System.IO;
using Bjd;
using Bjd.Logs;
using Bjd.Options;
using Bjd.Utils;
using Bjd.WebServer.Inside;

namespace Bjd.WebServer.Handlers
{
    /*******************************************************/
    //対象（ファイル）に関する各種の情報をまとめて扱うクラス
    /*******************************************************/
    class HandlerSelector
    {
        //readonly OneOption _oneOption;
        readonly Conf _conf;
        readonly Logger _logger;
        public HandlerSelector(Kernel kernel, Conf conf, Logger logger)
        {
            //System.Diagnostics.Trace.TraceInformation($"Target..ctor ");
            //_oneOption = oneOption;
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
                System.Diagnostics.Trace.TraceError($"Target..ctor DocumentRoot not exists ");
                DocumentRoot = null;//ドキュメントルート無効
            }
            FullPath = "";
            TargetKind = TargetKind.Non;
            Attr = new FileAttributes();
            FileInfo = null;
            CgiCmd = "";
            Uri = null;
        }
        public string DocumentRoot { get; private set; }//ドキュメントルート
        public string FullPath { get; private set; }
        public string PhysicalRootPath { get; private set; }
        public TargetKind TargetKind { get; private set; }
        public WebDavKind WebDavKind { get; private set; }//Ver5.1.x
        public FileAttributes Attr { get; private set; }//ファイルのアトリビュート
        public FileInfo FileInfo { get; private set; }//ファイルインフォメーション
        public string CgiCmd { get; private set; }//CGI実行プログラム
        public string Uri { get; private set; }
        /*************************************************/
        // 初期化
        /*************************************************/
        //uriによる初期化
        public void InitFromUri(string uri)
        {
            System.Diagnostics.Trace.TraceInformation($"Target.InitFromUri {uri}");
            Init(uri);
            System.Diagnostics.Trace.TraceInformation($"Target.InitFromUri TargetKind {TargetKind} WebDavKind {WebDavKind}");
        }
        //filenameによる初期化
        public void InitFromFile(string file)
        {

            var tmp = file.ToLower();// fullPathからuriを生成する
            var root = DocumentRoot.ToLower();
            if (tmp.IndexOf(root) != 0)
                return;
            var uri = file.Substring(root.Length);
            //uri = Util.SwapChar('\\', '/', uri);
            uri = Util.SwapChar(Path.DirectorySeparatorChar, '/', uri);
            if (string.IsNullOrEmpty(uri))
                uri = "/";

            Init(uri);
        }
        //コマンドによる初期化
        public void InitFromCmd(string fullPath)
        {
            TargetKind = TargetKind.Cgi;
            CgiCmd = "COMSPEC";
            FullPath = fullPath;
        }
        void Init(string uri)
        {

            Uri = uri;

            TargetKind = TargetKind.File;//通常ファイルであると仮置きする
            var enableCgiPath = false;//フォルダがCGI実行可能かどうか
            WebDavKind = WebDavKind.Non;//Ver5.1.x WebDAV対象外であることを仮置きする

            //****************************************************************
            //WebDavパスにヒットした場合、uri及びドキュメントルートを修正する
            //****************************************************************
            if ((bool)_conf.Get("useWebDav"))
            {
                var db = (Dat)_conf.Get("webDavPath");
                foreach (var o in db)
                {
                    if (!o.Enable) continue;

                    var name = o.StrList[0];
                    var write = Convert.ToBoolean(o.StrList[1]);//書き込み許可
                    var dir = o.StrList[2];
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
                        DocumentRoot = dir;
                        //WevDavパス定義にヒットした場合
                        WebDavKind = (write) ? WebDavKind.Write : WebDavKind.Read;
                        break;
                    }

                }

                // 最後が/で無い場合は、保管してヒットするかどうかを確認する
                if (uri[uri.Length - 1] != '/')
                {
                    var exUri = uri + "/";
                    foreach (var o in db)
                    {
                        if (!o.Enable) continue;
                        var name = o.StrList[0];
                        var write = Convert.ToBoolean(o.StrList[1]);//書き込み許可
                        var dir = o.StrList[2];
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
                            Uri = exUri;//リクエストに既に/が付いていたように動作させる
                            DocumentRoot = dir;
                            //WevDavパス定義にヒットした場合
                            WebDavKind = (write) ? WebDavKind.Write : WebDavKind.Read;
                            break;
                        }

                    }
                }


            }

            //****************************************************************
            //CGIパスにヒットした場合、uri及びドキュメントルートを修正する
            //****************************************************************
            bool useCgiPath = false;//CGIパス定義が存在するかどうかのフラグ
            if (WebDavKind == WebDavKind.Non && (bool)_conf.Get("useCgi"))
            {
                foreach (var o in (Dat)_conf.Get("cgiPath"))
                {
                    if (!o.Enable) continue;

                    useCgiPath = true;//有効なCGIパスの定義が存在する
                    var name = o.StrList[0];
                    var dir = o.StrList[1];

                    if (uri.ToUpper().IndexOf(name.ToUpper()) != 0) continue;

                    if (name.Length >= 1)
                    {
                        uri = uri.Substring(name.Length - 1);
                    }
                    else
                    {
                        uri = "/";
                    }
                    DocumentRoot = dir;
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
            if (WebDavKind == WebDavKind.Non && !useCgiPath && uri.Length >= 1)
            {
                foreach (var o in (Dat)_conf.Get("aliaseList"))
                {
                    if (!o.Enable) continue;

                    var name = o.StrList[0];
                    var dir = o.StrList[1];

                    if (uri.ToUpper() + "/" == name.ToUpper())
                    {
                        //ファイル指定されたターゲットがファイルではなくディレクトリの場合
                        TargetKind = TargetKind.Move;
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

                    DocumentRoot = dir;
                    break;

                }
            }

            /*************************************************/
            // uriから物理的なパス名を生成する
            /*************************************************/
            //FullPath = Util.SwapChar('/', '\\', DocumentRoot + uri);
            FullPath = Util.SwapChar('/', Path.DirectorySeparatorChar, DocumentRoot + uri);
            System.Diagnostics.Trace.TraceInformation($"Target.Init {FullPath}");

            /*************************************************/
            //ファイル指定されたターゲットがファイルではなくディレクトリの場合
            /*************************************************/
            if (WebDavKind == WebDavKind.Non)
            {
                //if (FullPath[FullPath.Length - 1] != '\\')
                if (FullPath[FullPath.Length - 1] != Path.DirectorySeparatorChar && Directory.Exists(FullPath))
                {
                    TargetKind = TargetKind.Move;
                    return;
                }
            }
            else
            {
                if (TargetKind == TargetKind.File && Directory.Exists(FullPath))
                {
                    TargetKind = TargetKind.Dir;
                    return;
                }
            }

            /*************************************************/
            // welcomeファイルのセット
            /*************************************************/
            //Uriでファイル名が指定されていない場合で、当該ディレクトリにwelcomeFileNameが存在する場合
            //ファイル名として使用する
            if (WebDavKind == WebDavKind.Non)
            {
                //Ver5.1.3
                try
                {
                    if (Path.GetFileName(FullPath) == "")
                    {
                        var tmp = ((string)_conf.Get("welcomeFileName")).Split(',');
                        foreach (string welcomeFileName in tmp)
                        {
                            //var newPath = Path.GetDirectoryName(FullPath) + "\\" + welcomeFileName;
                            var newPath = Path.Combine(Path.GetDirectoryName(FullPath), welcomeFileName);
                            if (!File.Exists(newPath)) continue;

                            FullPath = newPath;
                            System.Diagnostics.Trace.TraceInformation($"Target.Init welcomeFileName {FullPath}");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Set(LogKind.Error, null, 37, string.Format("uri={0} FullPath={1} {2}", uri, FullPath, ex.Message));
                    TargetKind = TargetKind.Non;
                    return;
                }

            }
            /*************************************************/
            //ターゲットはファイルとして存在するか
            /*************************************************/
            if (!File.Exists(FullPath))
            {
                //ディレクトリとして存在しない場合
                if (!Directory.Exists(FullPath))
                {
                    //存在しない
                    TargetKind = TargetKind.Non;
                    return;
                }

                if ((bool)_conf.Get("useDirectoryEnum") && WebDavKind == WebDavKind.Non)
                {
                    TargetKind = TargetKind.Dir;
                    return;
                }
            }

            /*************************************************/
            // 拡張子判断
            /*************************************************/
            // 「CGI実行が可能なフォルダの場合　拡張子がヒットすればターゲットはCGIである
            if (WebDavKind == WebDavKind.Non && enableCgiPath)
            {
                var ext = Path.GetExtension(FullPath);
                if (ext != null && ext.Length > 1)
                {
                    ext = ext.Substring(1);
                    foreach (var o in (Dat)_conf.Get("cgiCmd"))
                    {
                        if (!o.Enable) continue;

                        var cgiExt = o.StrList[0];
                        var cgiCmd = o.StrList[1];
                        if (cgiExt.ToUpper() == ext.ToUpper())
                        {
                            TargetKind = TargetKind.Cgi;//CGIである
                            CgiCmd = cgiCmd;
                        }

                    }
                }
            }

            /*************************************************/
            // ターゲットがSSIかどうかの判断
            /*************************************************/
            if (WebDavKind == WebDavKind.Non)
            {
                if (TargetKind == TargetKind.File && (bool)_conf.Get("useSsi"))
                {
                    //「SSIを使用する」場合
                    // SSI指定拡張子かどうかの判断
                    var ext = Path.GetExtension(FullPath);
                    if (ext != null && 1 <= ext.Length)
                    {
                        var cgiExtList = new List<string>(((string)_conf.Get("ssiExt")).Split(','));
                        if (0 <= cgiExtList.IndexOf(ext.Substring(1)))
                        {
                            //ターゲットファイルにキーワードが含まれているかどうかの確認
                            var physicalFullPath = Path.Combine(this.PhysicalRootPath, FullPath);
                            if (0 <= Util.IndexOf(physicalFullPath, "<!--#"))
                            {
                                TargetKind = TargetKind.Ssi;
                            }
                        }
                    }
                }
            }
            /*************************************************/
            // アトリビュート及びインフォメーションの取得
            /*************************************************/
            if (TargetKind == TargetKind.File || TargetKind == TargetKind.Ssi)
            {
                //ファイルアトリビュートの取得
                Attr = File.GetAttributes(FullPath);
                //ファイルインフォメーションの取得
                FileInfo = new FileInfo(FullPath);
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

