﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Bjd.Net;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Threading;
using Bjd.Memory;
using System.Threading.Tasks;

namespace Bjd
{
    public class HttpHeaders : IEnumerable<IHeader>, IDisposable
    {
        protected readonly List<IHeader> _ar = new List<IHeader>(25);
        static byte Cr = 0x0D;
        static byte Lf = 0x0A;
        static byte Colon = (byte)':';
        static byte Space = (byte)' ';

        public HttpHeaders()
        {
            //ContentLength = new OneHeader("Content-Length", "0");
        }
        public HttpHeaders(HttpHeaders header)
        {
            _ar = new List<IHeader>(header);
        }

        public HttpHeaders(byte[] buf)
        {
            _ar = new List<IHeader>();

            //\r\nを排除した行単位に加工する
            var lines = from b in Inet.GetLines(buf) select Inet.TrimCrlf(b);
            var key = "";
            foreach (var line in lines)
            {
                BufferData val = GetKeyVal(line, ref key);
                Append(key, val);
            }
        }

        ~HttpHeaders()
        {
            foreach (var h in _ar)
            {
                h.Dispose();
            }
            _ar.Clear();
        }

        public void Dispose()
        {
            foreach (var h in _ar)
            {
                h.Dispose();
            }
            _ar.Clear();
        }

        public virtual void Clear()
        {
            foreach (var h in _ar)
            {
                if (h is OneHeader) h.Dispose();
            }
            _ar.Clear();
        }


        //IEnumerable<T>の実装
        public IEnumerator<IHeader> GetEnumerator()
        {
            return ((IEnumerable<IHeader>)_ar).GetEnumerator();
        }

        //IEnumerable<T>の実装
        System.Collections.IEnumerator
            System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        //IEnumerable<T>の実装(関連プロパティ)
        public int Count
        {
            get
            {
                return _ar.Count;
            }
        }

        //GetValをstringに変換して返す
        public string GetVal(string key)
        {
            //Keyの存在確認
            var kUpper = key.ToUpper();
            var o = _ar.Find(h => h.KeyUpper == kUpper);
            return o == null ? null : o.ValString;
        }

        //Ver5.4.4 ヘッダの削除
        public void Remove(string key)
        {
            //Keyの存在確認
            var kUpper = key.ToUpper();
            var o = _ar.Find(h => h.KeyUpper == kUpper);
            if (o != null)
            {
                _ar.Remove(o);//存在する場合は、削除する
            }
        }


        //同一のヘッダがあった場合は置き換える
        public void Replace(string key, string valStr)
        {
            var kUpper = key.ToUpper();
            //byte[] への変換
            //var val = Encoding.ASCII.GetBytes(valStr);
            var val = valStr.ToAsciiBufferData();
            //Keyの存在確認
            var o = _ar.Find(h => h.KeyUpper == kUpper);
            if (o == null)
            {
                Append(key, val);//存在しない場合は追加
            }
            else
            {
                o.Val = val;//存在する場合は置き換え
            }
        }

        //同一のヘッダがあっても無条件に追加する
        public IHeader Append(string key, BufferData val)
        {
            var header = new OneHeader(key, val);
            _ar.Add(header);
            return header;
        }

        public IHeader Append(string key, string keyUpper, BufferData val)
        {
            var header = new OneHeader(key, keyUpper, val);
            _ar.Add(header);
            return header;
        }


        public IHeader Append(string key, string val)
        {
            var header = new OneHeader(key, val);
            _ar.Add(header);
            return header;
        }


        protected virtual bool AppendHeader(string keyUpper, BufferData val)
        {
            return false;
        }

        public async Task<bool> RecvAsync(ISocket sockTcp, int timeout, ILife iLife)
        {

            ////ヘッダ取得（データは初期化される）
            //_ar.Clear();

            var key = "";
            while (iLife.IsLife())
            {
                using (var line = await sockTcp.LineBufferRecvAsync(timeout))
                {
                    // error
                    if (line == null)
                        return false;

                    // remove cr lf
                    Inet.TrimCrlf(line);

                    // end header
                    if (line.DataSize == 0)
                        return true;

                    //１行分のデータからKeyとValを取得する
                    var val = GetKeyVal(line, ref key);
                    if (key != "")
                    {
                        var keyUpper = key.ToUpper();

                        // know header
                        if (AppendHeader(keyUpper, val)) continue;

                        // other header
                        Append(key, keyUpper, val);
                        continue;
                    }
                    val?.Dispose();

                    //Ver5.4.4 HTTP/1.0 200 OKを２行返すサーバがいるものに対処
                    var s = Encoding.ASCII.GetString(line.Data, 0, line.DataSize);
                    if (s.IndexOf("HTTP/") != 0)
                        return false;//ヘッダ異常
                }
            }
            return false;
        }

        public byte[] GetBytes()
        {

            //高速化のため、Buffer.BlockCopyに修正
            //byte[] b = new byte[0];
            //foreach(var o in Lines) {
            //    b = Bytes.Create(b,Encoding.ASCII.GetBytes(o.Key),": ",o.Val,"\r\n");
            //}
            //b = Bytes.Create(b,"\r\n");
            //return b;

            int size = 2;//空白行 \r\n
            _ar.ForEach(o =>
            {
                size += o.Key.Length + o.Val.DataSize + 4; //':'+' '+\r+\n
            });
            var buf = new byte[size];
            int p = 0;//書き込みポインタ
            _ar.ForEach(o =>
            {
                if (!o.Enabled) return;

                var k = Encoding.ASCII.GetBytes(o.Key);
                Buffer.BlockCopy(k, 0, buf, p, k.Length);
                p += k.Length;
                buf[p] = Colon;
                buf[p + 1] = Space;
                p += 2;
                Buffer.BlockCopy(o.Val.Data, 0, buf, p, o.Val.DataSize);
                p += o.Val.DataSize;
                buf[p] = Cr;
                buf[p + 1] = Lf;
                p += 2;
            });
            buf[p] = Cr;
            buf[p + 1] = Lf;

            return buf;

        }

        public BufferData GetBuffer()
        {
            //高速化のため、Buffer.BlockCopyに修正
            int size = 2;//空白行 \r\n
            foreach (var o in _ar)
            {
                if (!o.Enabled) continue;
                size += o.Key.Length + o.Val.DataSize + 4; //':'+' '+\r+\n
            }
            var buf = BufferPool.GetMaximum(size);
            ref int p = ref buf.DataSize;
            //_ar.ForEach(o =>
            //{
            //    if (!o.Enabled) return;

            //    //var k = Encoding.ASCII.GetBytes(o.Key);
            //    //Buffer.BlockCopy(k, 0, buf.Data, buf.DataSize, k.Length);
            //    //buf.DataSize += k.Length;
            //    buf.DataSize += Encoding.ASCII.GetBytes(o.Key, 0, o.Key.Length, buf.Data, buf.DataSize);

            //    buf[buf.DataSize++] = Colon;
            //    buf[buf.DataSize++] = Space;
            //    Buffer.BlockCopy(o.Val, 0, buf.Data, buf.DataSize, o.Val.Length);
            //    buf.DataSize += o.Val.Length;
            //    buf[buf.DataSize++] = Cr;
            //    buf[buf.DataSize++] = Lf;
            //});

            foreach (var o in _ar)
            {
                if (!o.Enabled) continue;

                buf.DataSize += Encoding.ASCII.GetBytes(o.Key, 0, o.Key.Length, buf.Data, buf.DataSize);
                buf[buf.DataSize++] = Colon;
                buf[buf.DataSize++] = Space;
                o.Val.CopyTo(buf, 0, buf.DataSize, o.Val.DataSize);
                buf[buf.DataSize++] = Cr;
                buf[buf.DataSize++] = Lf;
            }

            buf[buf.DataSize++] = Cr;
            buf[buf.DataSize++] = Lf;

            return buf;

        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            _ar.ForEach(o =>
            {
                //sb.Append(string.Format("{0}: {1}\r\n", o.Key, Encoding.ASCII.GetString(o.Val)));
                sb.Append(string.Format("{0}: {1}\r\n", o.Key, o.Val.ToAsciiString()));
            });
            sb.Append("\r\n");
            return sb.ToString();
        }
        //１行分のデータからKeyとValを取得する
        BufferData GetKeyVal(byte[] line, ref string key)
        {
            key = "";
            for (int i = 0; i < line.Length; i++)
            {
                if (key == "")
                {
                    //if (line[i] == ':')
                    if (line[i] == Colon)
                    {
                        var tmp = new byte[i];
                        Buffer.BlockCopy(line, 0, tmp, 0, i);
                        key = Encoding.ASCII.GetString(tmp);
                    }
                }
                else
                {
                    if (line[i] != ' ')
                    {
                        //var val = new byte[line.Length - i];
                        //Buffer.BlockCopy(line, i, val, 0, line.Length - i);
                        var len = line.Length - i;
                        var val = BufferPool.GetMaximum(len);
                        Buffer.BlockCopy(line, i, val.Data, 0, len);
                        val.DataSize = len;
                        return val;
                    }
                }
            }
            return BufferData.Empty;
        }
        //１行分のデータからKeyとValを取得する
        BufferData GetKeyVal(BufferData line, ref string key)
        {
            key = "";
            for (int i = 0; i < line.DataSize; i++)
            {
                if (key == "")
                {
                    //if (line.Data[i] == ':')
                    //if (line.Data[i] == Colon)
                    if (line[i] == Colon)
                    {
                        key = Encoding.ASCII.GetString(line.Data, 0, i);
                    }
                }
                else
                {
                    if (line.Data[i] != ' ')
                    {
                        //var val = new byte[line.DataSize - i];
                        //Buffer.BlockCopy(line.Data, i, val, 0, line.DataSize - i);
                        var len = line.DataSize - i;
                        var val = BufferPool.GetMaximum(len);
                        line.CopyTo(val, i, 0, len);
                        return val;
                    }
                }
            }
            return BufferData.Empty;
        }

    }
}
