using Bjd.Memory;
using Bjd.Threading;
using Bjd.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bjd.Net.Sockets
{
    public static class SocketExtensions
    {
        private static readonly byte[] CrLf = new byte[] { 0x0D, 0x0A };

        public static string StringRecv(this ISocket sock, int sec, ILife iLife)
        {
            return sock.StringRecv(Encoding.ASCII, sec, iLife);
        }

        //１行のString受信
        public static string StringRecv(this ISocket sock, Encoding enc, int sec, ILife iLife)
        {
            try
            {
                using (var buffer = sock.LineBufferRecv(sec, iLife))
                {
                    if (buffer == null) return null;
                    return enc.GetString(buffer.Data, 0, buffer.DataSize);
                }
            }
            catch (Exception e)
            {
                Util.RuntimeException(e.Message);
            }
            return null;
        }

        //１行のString受信
        public static async ValueTask<string> StringRecvAsync(this ISocket sock, Encoding enc, int sec, ILife iLife)
        {
            try
            {
                using (var buffer = await sock.LineBufferRecvAsync(sec))
                {
                    if (buffer == null) return null;
                    return enc.GetString(buffer.Data, 0, buffer.DataSize);
                }
            }
            catch (Exception e)
            {
                Util.RuntimeException(e.Message);
            }
            return null;
        }

        //１行のString受信
        public static string StringRecv(this ISocket sock, string charsetName, int sec, ILife iLife)
        {
            try
            {
                var enc = CodePagesEncodingProvider.Instance.GetEncoding(charsetName);
                if (enc == null)
                    enc = Encoding.GetEncoding(charsetName);
                return StringRecv(sock, enc, sec, iLife);
            }
            catch (Exception e)
            {
                Util.RuntimeException(e.Message);
            }
            return null;
        }

        public static async ValueTask<string> StringRecvAsync(this ISocket sock, string charsetName, int sec, ILife iLife)
        {
            try
            {
                var enc = CodePagesEncodingProvider.Instance.GetEncoding(charsetName);
                if (enc == null)
                    enc = Encoding.GetEncoding(charsetName);
                return await StringRecvAsync(sock, enc, sec, iLife);
            }
            catch (Exception e)
            {
                Util.RuntimeException(e.Message);
            }
            return null;
        }

        // 【１行受信】
        //切断されている場合、nullが返される
        //public string AsciiRecv(int timeout, OperateCrlf operateCrlf, ILife iLife) {
        public static string AsciiRecv(this ISocket sock, int timeout, ILife iLife)
        {
            using (var buf = sock.LineBufferRecv(timeout, iLife))
            {
                return buf == null ? null : Encoding.ASCII.GetString(buf.Data, 0, buf.DataSize);
            }
        }
        public static async ValueTask<string> AsciiRecvAsync(this ISocket sock, int timeout)
        {
            using (var buf = await sock.LineBufferRecvAsync(timeout))
            {
                return buf == null ? null : Encoding.ASCII.GetString(buf.Data, 0, buf.DataSize);
            }
        }

        // 【１行受信】
        //切断されている場合、nullが返される
        public static CharsData AsciiRecvChars(this ISocket sock, int timeout, ILife iLife)
        {
            using (var buf = sock.LineBufferRecv(timeout, iLife))
            {
                return buf == null ? null : buf.ToAsciiCharsData();
            }
        }
        public static async ValueTask<CharsData> AsciiRecvCharsAsync(this ISocket sock, int timeoutSec)
        {
            var result = await sock.LineBufferRecvAsync(timeoutSec * 1000);
            try
            {
                return result.ToAsciiCharsData();
            }
            finally
            {
                if (result != BufferData.Empty)
                {
                    result.Dispose();
                }
            }
        }





        public static int Send(this ISocket sock, byte[] buf, int length)
        {
            return sock.Send(buf, 0, length);
        }

        public static int Send(this ISocket sock, byte[] buf)
        {
            return sock.Send(buf, 0, buf.Length);
        }
        //1行送信
        //内部でCRLFの２バイトが付かされる
        public static int LineSend(this ISocket sock, byte[] buf)
        {
            var d = new[] { new ArraySegment<byte>(buf), new ArraySegment<byte>(CrLf) };
            return sock.Send(d);
        }


        //１行のString送信 (\r\nが付加される)
        public static bool StringSend(this ISocket sock, string str, Encoding enc)
        {
            try
            {
                var buf = enc.GetBytes(str);
                LineSend(sock, buf);
                return true;
            }
            catch (Exception e)
            {
                Util.RuntimeException(e.Message);
            }
            return false;
        }
        //１行のString送信(ASCII)  (\r\nが付加される)
        public static bool StringSend(this ISocket sock, string str)
        {
            return StringSend(sock, str, Encoding.ASCII);
        }

        //１行のString送信 (\r\nが付加される)
        public static bool StringSend(this ISocket sock, string str, string charsetName)
        {
            try
            {
                var enc = CodePagesEncodingProvider.Instance.GetEncoding(charsetName);
                if (enc == null)
                    enc = Encoding.GetEncoding(charsetName);
                return StringSend(sock, str, enc);
            }
            catch (Exception e)
            {
                Util.RuntimeException(e.Message);
            }
            return false;
        }

        //【送信】テキスト（バイナリかテキストかが不明な場合もこちら）
        public static int SendUseEncode(this ISocket sock, byte[] buf)
        {
            //実際の送信処理にテキストとバイナリの区別はない
            return Send(sock, buf);
        }


        public static async ValueTask<bool> AsciiSendAsync(this ISocket sock, CharsData data)
        {
            using (var buf = data.ToAsciiBufferData())
            {
                await sock.SendAsync(buf);
            }
            return true;
        }


        public static async ValueTask<bool> AsciiLineSendAsync(this ISocket sock, CharsData data)
        {
            using (var buf = data.ToAsciiLineBufferData())
            {
                await sock.SendAsync(buf);
            }
            return true;
        }

        ////内部でASCIIコードとしてエンコードする１行送信  (\r\nが付加される)
        ////LineSend()のオーバーライドバージョン
        ////public int AsciiSend(string str, OperateCrlf operateCrlf) {
        //public static int AsciiSend(this ISocket sock, string str)
        //{
        //    _lastLineSend = str;
        //    var buf = Encoding.ASCII.GetBytes(str);
        //    //return LineSend(buf, operateCrlf);
        //    //とりあえずCrLfの設定を無視している
        //    return LineSend(buf);
        //}

        //【送信】バイナリ
        public static int SendNoEncode(this ISocket sock, byte[] buf)
        {
            //実際の送信処理にテキストとバイナリの区別はない
            return sock.SendNoTrace(buf);
        }
    }
}
