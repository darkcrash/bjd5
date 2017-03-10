using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.Threading;

namespace Bjd.Utils
{
    public class Inet
    {
        private Inet() { }//デフォルトコンストラクタの隠蔽


        //**************************************************************************
        //バイナリ-文字列変換(バイナリデータをテキスト化して送受信するため使用する)
        //**************************************************************************
        static public byte[] ToBytes(string str)
        {
            if (str == null)
            {
                str = "";
            }
            return Encoding.Unicode.GetBytes(str);
        }
        static public string FromBytes(byte[] buf)
        {
            if (buf == null)
            {
                buf = new byte[0];
            }
            return Encoding.Unicode.GetString(buf);
        }
        //**************************************************************************

        //Ver5.0.0-a11 高速化
        //テキスト処理クラス　stringを\r\nでList<string>に分割する
        static public List<string> GetLines(string str)
        {
            return str.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
        }

        static public List<byte[]> GetLines(byte[] buf)
        {
            var lines = new List<byte[]>();

            if (buf == null || buf.Length == 0)
            {
                return lines;
            }
            int start = 0;
            for (var end = 0; ; end++)
            {
                if (buf[end] == '\n')
                {
                    if (1 <= end && buf[end - 1] == '\r')
                    {
                        var tmp = new byte[end - start + 1];//\r\nを削除しない
                        Buffer.BlockCopy(buf, start, tmp, 0, end - start + 1);//\r\nを削除しない
                        lines.Add(tmp);
                        //string str = Encoding.ASCII.GetString(tmp);
                        //ar.Add(str);
                        start = end + 1;
                        //Unicode
                    }
                    else if (2 <= end && end + 1 < buf.Length && buf[end + 1] == '\0' && buf[end - 1] == '\0' && buf[end - 2] == '\r')
                    {
                        var tmp = new byte[end - start + 2];//\r\nを削除しない
                        Buffer.BlockCopy(buf, start, tmp, 0, end - start + 2);//\r\nを削除しない
                        lines.Add(tmp);
                        start = end + 2;
                    }
                    else
                    {//\nのみ
                        var tmp = new byte[end - start + 1];//\nを削除しない
                        Buffer.BlockCopy(buf, start, tmp, 0, end - start + 1);//\nを削除しない
                        lines.Add(tmp);
                        start = end + 1;
                    }
                }
                if (end >= buf.Length - 1)
                {
                    if (0 < (end - start + 1))
                    {
                        var tmp = new byte[end - start + 1];//\r\nを削除しない
                        Buffer.BlockCopy(buf, start, tmp, 0, end - start + 1);//\r\nを削除しない
                        lines.Add(tmp);
                    }
                    break;
                }
            }
            return lines;
        }

        //行単位での切り出し(\r\nは削除しない)
        static public List<byte[]> GetLines(ArraySegment<byte> buf)
        {
            var lines = new List<byte[]>();

            if (buf == null || buf.Count == 0)
            {
                return lines;
            }
            int start = 0;
            for (var end = buf.Offset; ; end++)
            {
                var ary = buf.Array;
                if (ary[end] == '\n')
                {
                    if (1 <= end && ary[end - 1] == '\r')
                    {
                        var tmp = new byte[end - start + 1];//\r\nを削除しない
                        Buffer.BlockCopy(ary, start, tmp, 0, end - start + 1);//\r\nを削除しない
                        lines.Add(tmp);
                        //string str = Encoding.ASCII.GetString(tmp);
                        //ar.Add(str);
                        start = end + 1;
                        //Unicode
                    }
                    else if (2 <= end && end + 1 < (buf.Offset + buf.Count) && ary[end + 1] == '\0' && ary[end - 1] == '\0' && ary[end - 2] == '\r')
                    {
                        var tmp = new byte[end - start + 2];//\r\nを削除しない
                        Buffer.BlockCopy(ary, start, tmp, 0, end - start + 2);//\r\nを削除しない
                        lines.Add(tmp);
                        start = end + 2;
                    }
                    else
                    {//\nのみ
                        var tmp = new byte[end - start + 1];//\nを削除しない
                        Buffer.BlockCopy(ary, start, tmp, 0, end - start + 1);//\nを削除しない
                        lines.Add(tmp);
                        start = end + 1;
                    }
                }
                if (end >= (buf.Offset + buf.Count) - 1)
                {
                    if (0 < (end - start + 1))
                    {
                        var tmp = new byte[end - start + 1];//\r\nを削除しない
                        Buffer.BlockCopy(ary, start, tmp, 0, end - start + 1);//\r\nを削除しない
                        lines.Add(tmp);
                    }
                    break;
                }
            }
            return lines;
        }

        //\r\nの削除
        static public byte[] TrimCrlf(byte[] buf)
        {
            if (buf.Length >= 1 && buf[buf.Length - 1] == '\n')
            {
                var count = 1;
                if (buf.Length >= 2 && buf[buf.Length - 2] == '\r')
                {
                    count++;
                }
                var tmp = new byte[buf.Length - count];
                Buffer.BlockCopy(buf, 0, tmp, 0, buf.Length - count);
                return tmp;
            }
            else if (buf.Length >= 1 && buf[buf.Length - 1] == '\r')
            {
                var count = 1;
                var tmp = new byte[buf.Length - count];
                Buffer.BlockCopy(buf, 0, tmp, 0, buf.Length - count);
                return tmp;
            }
            return buf;
        }
        //\r\nの削除
        static public string TrimCrlf(string str)
        {
            if (str.Length >= 1 && str[str.Length - 1] == '\n')
            {
                var count = 1;
                if (str.Length >= 2 && str[str.Length - 2] == '\r')
                {
                    count++;
                }
                return str.Substring(0, str.Length - count);
            }
            return str;
        }

        //サニタイズ処理(１行対応)
        public static string Sanitize(string str)
        {
            str = Util.SwapStr("&", "&amp;", str);
            str = Util.SwapStr("<", "&lt;", str);
            str = Util.SwapStr(">", "&gt;", str);
            //Ver5.0.0-a17            
            str = Util.SwapStr("~", "%7E", str);
            return str;

        }

        //暫定
        static public SockTcp Connect(Kernel kernel, Ip ip, int port, int timeout, Ssl ssl)
        {
            return new SockTcp(kernel, ip, port, timeout, ssl);
        }

        //指定した長さのランダム文字列を取得する（チャレンジ文字列用）
        static public string ChallengeStr(int len)
        {
            const string val = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var bytes = new byte[len];
            //var rngcsp = new RNGCryptoServiceProvider();
            //rngcsp.GetNonZeroBytes(bytes);
            var rngcsp = RandomNumberGenerator.Create();
            rngcsp.GetBytes(bytes);

            // 乱数をもとに使用文字を組み合わせる
            var str = String.Empty;
            foreach (var b in bytes)
            {
                var rnd = new Random(b);
                int index = rnd.Next(val.Length);
                str += val[index];
            }
            return str;
        }

        //ハッシュ文字列の作成（MD5）
        static public string Md5Str(string str)
        {
            if (str == null)
            {
                return "";
            }
            var md5 = System.Security.Cryptography.MD5.Create();
            md5.Initialize();
            //var md5EncryptionObject = new MD5CryptoServiceProvider();
            var originalStringBytes = Encoding.UTF8.GetBytes(str);
            //var encodedStringBytes = md5EncryptionObject.ComputeHash(originalStringBytes);
            var encodedStringBytes = md5.ComputeHash(originalStringBytes);
            return BitConverter.ToString(encodedStringBytes);
        }

        //リクエスト行がURLエンコードされている場合は、その文字コードを取得する
        static public Encoding GetUrlEncoding(string str)
        {
            var tmp = str.Split(' ');
            if (tmp.Length >= 3)
                str = tmp[1];

            var buf = new byte[str.Length];
            var len = 0;
            var find = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '%')
                {
                    find = true;
                    var hex = string.Format("{0}{1}", str[i + 1], str[i + 2]);
                    var n = Convert.ToInt32(hex, 16);
                    buf[len++] = (byte)n;
                    i += 2;
                }
                else
                {
                    buf[len++] = (byte)str[i];
                }
            }
            if (!find)
                return Encoding.ASCII;
            var buf2 = new byte[len];
            Buffer.BlockCopy(buf, 0, buf2, 0, len);
            return MLang.GetEncoding(buf2);
        }

        static public List<String> RecvLines(SockTcp cl, int sec, ILife iLife)
        {
            var lines = new List<string>();
            while (true)
            {
                var buf = cl.LineRecv(sec, iLife);
                if (buf == null)
                    break;
                if (buf.Length == 0)
                    break;
                var s = Encoding.ASCII.GetString(TrimCrlf(buf));
                //if (s == "")
                //    break;
                lines.Add(s);
            }
            return lines;
        }
    }

}
