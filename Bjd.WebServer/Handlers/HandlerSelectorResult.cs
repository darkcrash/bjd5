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
            _FullPath = null;
            DocumentRoot = "";
            PhysicalRootPath = "";
            TargetKind = HandlerKind.Non;
            _Attr = null;
            FileInfo = null;
            CgiCmd = "";
            Uri = null;
        }

        internal void ResetFullPath(string path)
        {
            _FullPath = path;
            _Attr = null;
            FileInfo = null;
            _Ext = Path.GetExtension(_FullPath);

        }

        private string _FullPath;
        private string _Ext;
        private FileAttributes? _Attr;

        public string DocumentRoot { get; internal set; }//ドキュメントルート
        public string PhysicalRootPath { get; internal set; }
        public string FullPath { get => _FullPath; }
        public HandlerKind TargetKind { get; internal set; }
        public WebDavKind WebDavKind { get; internal set; }//Ver5.1.x
        public FileAttributes Attr
        {
            get
            {
                if (!_Attr.HasValue)
                {
                    _Attr = File.GetAttributes(FullPath);
                }
                return _Attr.Value;
            }
        }
        public string Ext { get => _Ext; }
        //ファイルのアトリビュート
        public FileInfo FileInfo { get; internal set; }//ファイルインフォメーション
        public string CgiCmd { get; internal set; }//CGI実行プログラム
        public string Uri { get; internal set; }
        public IHandler Handler { get; internal set; }
        public bool FileExists { get; internal set; }
    }
}

