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
            CgiCmd = "";
            Uri = null;
        }

        internal void ResetFullPath(string path)
        {
            _FullPath = path;
            _Attr = null;
            _LastWriteTimeUtc = null;
            _FileInfo = null;
            _FileSize = null;
            _FileExists = null;
            _Ext = Path.GetExtension(_FullPath);
        }
        internal void ResetFullPath(string path, bool exists)
        {
            _FullPath = path;
            _Attr = null;
            _LastWriteTimeUtc = null;
            _FileInfo = null;
            _FileSize = null;
            _FileExists = exists;
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
                    //_Attr = Bjd.Common.IO.CachedFileAttributes.GetAttributes(FullPath);
                    _Attr = FileInfo.Attributes;
                }
                return _Attr.Value;
            }
        }
        public string Ext { get => _Ext; }
        //ファイルのアトリビュート
        public string CgiCmd { get; internal set; }//CGI実行プログラム
        public string Uri { get; internal set; }
        public IHandler Handler { get; internal set; }

        private bool? _FileExists;
        public bool FileExists
        {
            get
            {
                if (!_FileExists.HasValue)
                {
                    _FileExists = Bjd.Common.IO.CachedFileExists.ExistsFile(_FullPath);
                }
                return _FileExists.Value;
            }
        }


        private DateTime? _LastWriteTimeUtc;
        public DateTime LastWriteTimeUtc
        {
            get
            {
                if (!_LastWriteTimeUtc.HasValue)
                {
                    //_LastWriteTimeUtc = System.IO.File.GetLastWriteTimeUtc(_FullPath);
                    _LastWriteTimeUtc = FileInfo.LastWriteTimeUtc;
                }
                return _LastWriteTimeUtc.Value;
            }
        }

        private DateTime? _LastWriteTime;
        public DateTime LastWriteTime
        {
            get
            {
                if (!_LastWriteTime.HasValue)
                {
                    //_LastWriteTime = System.IO.File.GetLastWriteTime(_FullPath);
                    _LastWriteTime = FileInfo.LastWriteTime;
                }
                return _LastWriteTime.Value;
            }
        }

        private FileInfo _FileInfo;
        private FileInfo FileInfo
        {
            get
            {
                if (_FileInfo == null)
                {
                    _FileInfo = Bjd.Common.IO.CachedFileInfo.GetFileInfo(_FullPath);
                }
                return _FileInfo;
            }
        }

        private long? _FileSize;
        public long FileSize
        {
            get
            {
                if (!_FileSize.HasValue)
                {
                    _FileSize = FileInfo.Length;
                }
                return _FileSize.Value;
            }
        }



    }
}

