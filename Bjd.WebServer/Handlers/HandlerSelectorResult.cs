using System;
using System.Collections.Generic;
using System.IO;
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
    class HandlerSelectorResult
    {
        public HandlerSelectorResult()
        {
            DocumentRoot = "";
            PhysicalRootPath = "";
            FullPath = "";
            TargetKind = HandlerKind.Non;
            Attr = new FileAttributes();
            FileInfo = null;
            CgiCmd = "";
            Uri = null;
        }

        public string DocumentRoot { get; internal set; }//ドキュメントルート
        public string PhysicalRootPath { get; internal set; }
        public string FullPath { get; internal set; }
        public HandlerKind TargetKind { get; internal set; }
        public WebDavKind WebDavKind { get; internal set; }//Ver5.1.x
        public FileAttributes Attr { get; internal set; }//ファイルのアトリビュート
        public FileInfo FileInfo { get; internal set; }//ファイルインフォメーション
        public string CgiCmd { get; internal set; }//CGI実行プログラム
        public string Uri { get; internal set; }
        public IHandler Handler { get; internal set; }
        public bool FileExists { get; internal set; }
    }
}

