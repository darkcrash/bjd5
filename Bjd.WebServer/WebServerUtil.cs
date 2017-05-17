using Bjd.Memory;
using Bjd.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Bjd.WebServer
{
    class WebServerUtil
    {
        //ETagを生成する サイズ+更新日時（秒単位）
        public static string Etag(FileInfo fileInfo)
        {
            if (fileInfo != null)
                return string.Format("\"{0:x}-{1:x}\"", fileInfo.Length, (fileInfo.LastWriteTimeUtc.Ticks / 10000000));
            return "";
        }

        public static string Etag(Handlers.HandlerSelectorResult result)
        {
            if (result != null)
                return string.Format("\"{0:x}-{1:x}\"", result.FileSize, (result.LastWriteTimeUtc.Ticks / 10000000));
            return "";
        }


        public static string StatusMessage(int code)
        {
            var statusMessage = "";
            switch (code)
            {
                case 102:
                    statusMessage = "Processiong"; //RFC2518(10.1)
                    break;
                case 200:
                    statusMessage = "Document follows";
                    break;
                case 201:
                    statusMessage = "Created";
                    break;
                case 204:
                    statusMessage = "No Content";
                    break;
                case 206:
                    statusMessage = "Partial Content";
                    break;
                case 207:
                    statusMessage = "Multi-Status"; //RFC2518(10.2)
                    break;
                case 301:
                    statusMessage = "Moved Permanently";
                    break;
                case 302:
                    statusMessage = "Moved Temporarily";
                    break;
                case 304:
                    statusMessage = "Not Modified";
                    break;
                case 400:
                    statusMessage = "Missing Host header or incompatible headers detected.";
                    break;
                case 401:
                    statusMessage = "Unauthorized";
                    break;
                case 402:
                    statusMessage = "Payment Required";
                    break;
                case 403:
                    statusMessage = "Forbidden";
                    break;
                case 404:
                    statusMessage = "Not Found";
                    break;
                case 405:
                    statusMessage = "Method Not Allowed";
                    break;
                case 412:
                    statusMessage = "Precondition Failed";
                    break;
                case 422:
                    statusMessage = "Unprocessable"; //RFC2518(10.3)
                    break;
                case 423:
                    statusMessage = "Locked"; //RFC2518(10.4)
                    break;
                case 424:
                    statusMessage = "Failed Dependency"; //RFC2518(10.5)
                    break;
                case 500:
                    statusMessage = "Internal Server Error";
                    break;
                case 501:
                    statusMessage = "Request method not implemented";
                    break;
                case 507:
                    statusMessage = "Insufficient Storage"; //RFC2518(10.6)
                    break;
            }
            return statusMessage;
        }

        public static string UrlDecode(string s)
        {
            //var enc = Inet.GetUrlEncoding(s);
            //var b = new List<byte>();
            //for (var i = 0; i < s.Length; i++)
            //{
            //    switch (s[i])
            //    {
            //        case '%':
            //            b.Add((byte)int.Parse(s[++i].ToString() + s[++i].ToString(), NumberStyles.HexNumber));
            //            break;
            //        case '+':
            //            b.Add(0x20);
            //            break;
            //        default:
            //            b.Add((byte)s[i]);
            //            break;
            //    }
            //}
            //return enc.GetString(b.ToArray(), 0, b.Count);

            var enc = Inet.GetUrlEncoding(s);
            using (var b = BufferPool.Get(s.Length))
            {

                for (var i = 0; i < s.Length; i++)
                {
                    switch (s[i])
                    {
                        case '%':
                            b.Append((byte)int.Parse(s[++i].ToString() + s[++i].ToString(), NumberStyles.HexNumber));
                            break;
                        case '+':
                            b.Append(0x20);
                            break;
                        default:
                            b.Append((byte)s[i]);
                            break;
                    }
                }
                using (var c = b.ToCharsData(enc))
                {
                    return c.ToString();
                }
            }


        }

        public static string UrlDecode(CharsData s)
        {
            var enc = Inet.GetUrlEncoding(s);
            using (var b = BufferPool.Get(s.Length))
            {

                for (var i = 0; i < s.DataSize; i++)
                {
                    switch (s[i])
                    {
                        case '%':
                            b.Append((byte)int.Parse(s[++i].ToString() + s[++i].ToString(), NumberStyles.HexNumber));
                            break;
                        case '+':
                            b.Append(0x20);
                            break;
                        default:
                            b.Append((byte)s[i]);
                            break;
                    }
                }
                using (var c = b.ToCharsData(enc))
                {
                    return c.ToString();
                }
            }

        }

    }
}