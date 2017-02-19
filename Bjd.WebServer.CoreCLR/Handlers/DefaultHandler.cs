using Bjd.Acls;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Options;
using Bjd.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.WebServer.Handlers
{
    internal class DefaultHandler : IHandler
    {
        readonly AttackDb _attackDb;//自動拒否
        private Kernel _kernel;
        private Conf _conf;
        private AclList AclList;

        public DefaultHandler(Kernel kernel, Conf conf, Logger logger)
        {
            _kernel = kernel;
            _conf = conf;

            //ACLリスト 定義が無い場合は、aclListを生成しない
            var acl = (Dat)_conf.Get("acl");
            AclList = new AclList(acl, (int)_conf.Get("enableAcl"), logger);

            var useAutoAcl = (bool)_conf.Get("useAutoAcl");// ACL拒否リストへ自動追加する
            if (useAutoAcl)
            {
                const int max = 1; //発生回数
                const int sec = 120; // 対象期間(秒)
                _attackDb = new AttackDb(sec, max);
            }

        }


        public bool Request(HttpRequestContext context, HandlerSelectorResult result)
        {
            var connection = context.Connection;
            var Logger = connection.Logger;

            //以下は、通常ファイルの処理 TARGET_KIND.FILE

            //********************************************************************
            //Modified処理
            //********************************************************************
            if (context.Header.GetVal("If_Modified_Since") != null)
            {
                var dt = Util.Str2Time(context.Header.GetVal("If-Modified-Since"));
                if (result.FileInfo.LastWriteTimeUtc.Ticks / 10000000 <= dt.Ticks / 10000000)
                {
                    context.ResponseCode = 304;
                    //goto SEND;
                    return true;
                }
            }
            if (context.Header.GetVal("If_Unmodified_Since") != null)
            {
                var dt = Util.Str2Time(context.Header.GetVal("If_Unmodified_Since"));
                if (result.FileInfo.LastWriteTimeUtc.Ticks / 10000000 > dt.Ticks / 10000000)
                {
                    context.ResponseCode = 412;
                    //goto SEND;
                    return true;
                }
            }
            context.Response.AddHeader("Last-Modified", Util.UtcTime2Str(result.FileInfo.LastWriteTimeUtc));
            //********************************************************************
            //ETag処理
            //********************************************************************
            // (1) useEtagがtrueの場合は、送信時にETagを付加する
            // (2) If-None-Match 若しくはIf-Matchヘッダが指定されている場合は、排除対象かどうかの判断が必要になる
            if ((bool)_conf.Get("useEtag") || context.Header.GetVal("If-Match") != null || context.Header.GetVal("If-None-Match") != null)
            {
                //Ver5.1.5
                //string etagStr = string.Format("\"{0:x}-{1:x}\"", target.FileInfo.Length, (target.FileInfo.LastWriteTimeUtc.Ticks / 10000000));
                var etagStr = WebServerUtil.Etag(result.FileInfo);
                string str;
                if (null != (str = context.Header.GetVal("If-Match")))
                {
                    if (str != "*" && str != etagStr)
                    {
                        context.ResponseCode = 412;
                        //goto SEND;
                        return true;
                    }

                }
                if (null != (str = context.Header.GetVal("If-None-Match")))
                {
                    if (str != "*" && str == etagStr)
                    {
                        context.ResponseCode = 304;
                        //goto SEND;
                        return true;
                    }
                }
                if ((bool)_conf.Get("useEtag"))
                    context.Response.AddHeader("ETag", etagStr);
            }
            //********************************************************************
            //Range処理
            //********************************************************************
            context.Response.AddHeader("Accept-Range", "bytes");
            var rangeFrom = 0L;//デフォルトは最初から
            var rangeTo = result.FileInfo.Length;//デフォルトは最後まで（ファイルサイズ）
            if (context.Header.GetVal("Range") != null)
            {//レンジ指定のあるリクエストの場合
                var range = context.Header.GetVal("Range");
                //指定範囲を取得する（マルチ指定には未対応）
                if (range.IndexOf("bytes=") == 0)
                {
                    range = range.Substring(6);
                    var tmp = range.Split('-');


                    //Ver5.3.5 ApacheKiller対処
                    if (tmp.Length > 20)
                    {
                        Logger.Set(LogKind.Secure, connection.Connection, 9000054, string.Format("[ Apache Killer ]Range:{0}", range));

                        AutoDeny(Logger, false, connection.RemoteIp);
                        context.ResponseCode = 503;
                        connection.KeepAlive = false;//切断
                        //goto SEND;
                        return true;
                    }

                    if (tmp.Length == 2)
                    {

                        //Ver5.3.6 のデバッグ用
                        //tmp[1] = "499";

                        if (tmp[0] != "")
                        {
                            if (tmp[1] != "")
                            {// bytes=0-10 0～10の11バイト

                                //Ver5.5.9
                                rangeFrom = Convert.ToInt64(tmp[0]);
                                if (tmp[1] != "")
                                {
                                    //Ver5.5.9
                                    rangeTo = Convert.ToInt64(tmp[1]);
                                    if (result.FileInfo.Length <= rangeTo)
                                    {
                                        rangeTo = result.FileInfo.Length - 1;
                                    }
                                    else
                                    {
                                        context.Response.SetRangeTo = true;//Ver5.4.0
                                    }
                                }
                            }
                            else
                            {// bytes=3- 3～最後まで
                                rangeTo = result.FileInfo.Length - 1;
                                rangeFrom = Convert.ToInt64(tmp[0]);
                            }
                        }
                        else
                        {
                            if (tmp[1] != "")
                            {// bytes=-3 最後から3バイト
                                var len = Convert.ToInt64(tmp[1]);
                                rangeTo = result.FileInfo.Length - 1;
                                rangeFrom = rangeTo - len + 1;
                                if (rangeFrom < 0)
                                    rangeFrom = 0;
                                context.Response.SetRangeTo = true;//Ver5.4.0
                            }

                        }
                        if (rangeFrom <= rangeTo)
                        {
                            //正常に範囲を取得できた場合、事後Rangeモードで動作する
                            context.Response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", rangeFrom, rangeTo, result.FileInfo.Length));
                            context.ResponseCode = 206;
                        }
                    }
                }
            }
            //通常ファイルのドキュメント
            if (context.Request.Method != HttpMethod.Head)
            {
                if (!context.Response.CreateFromFile(result.FullPath, rangeFrom, rangeTo))
                    return false;
            }

            return true;
        }

        void AutoDeny(Logger Logger, bool success, Ip remoteIp)
        {
            _kernel.Trace.TraceWarning($"WebServer.AutoDeny ");
            if (_attackDb == null)
                return;
            //データベースへの登録
            if (!_attackDb.IsInjustice(success, remoteIp))
                return;

            //ブルートフォースアタック
            if (AclList.Append(remoteIp))
            {//ACL自動拒否設定(「許可する」に設定されている場合、機能しない)
                //追加に成功した場合、オプションを書き換える
                var d = (Dat)_conf.Get("acl");
                var name = string.Format("AutoDeny-{0}", DateTime.Now);
                var ipStr = remoteIp.ToString();
                d.Add(true, string.Format("{0}\t{1}", name, ipStr));
                _conf.Set("acl", d);
                _conf.Save(_kernel.Configuration);

                Logger.Set(LogKind.Secure, null, 9000055, string.Format("{0},{1}", name, ipStr));
            }
            else
            {
                Logger.Set(LogKind.Secure, null, 9000056, remoteIp.ToString());
            }
        }

    }
}
