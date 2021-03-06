﻿using Bjd.Memory;
using System;

namespace Bjd.ProxyHttpServer
{
    class BodyBuf : IDisposable
    {
        byte[] _buf;
        int _threwLength;//切り捨てたサイズ
        public bool CanUse { get; private set; }//キャッシュや、制限文字列で使用可能かどうかのフラグ
        readonly int _max;//指定サイズ以上のバッファリングをしない

        public BodyBuf(int max)
        {
            _max = max;
            Set(new byte[0]);
        }
        public void Dispose()
        {
            _buf = null;
        }
        public int Length
        {
            get
            {
                return _buf.Length + _threwLength;
            }
        }
        public void Add(byte[] b)
        {
            if (_buf.Length != 0)
            {
                //var tmp =  new byte[buf.Length];
                //Buffer.BlockCopy(buf, 0, tmp, 0, buf.Length);
                var tmp = _buf;
                _buf = new byte[tmp.Length + b.Length];
                Buffer.BlockCopy(tmp, 0, _buf, 0, tmp.Length);
                Buffer.BlockCopy(b, 0, _buf, tmp.Length, b.Length);
            }
            else
            {
                _buf = b;
            }
        }
        public void Add(BufferData b)
        {
            var tmp = _buf;
            _buf = new byte[tmp.Length + b.DataSize];
            if (tmp.Length != 0) Buffer.BlockCopy(tmp, 0, _buf, 0, tmp.Length);
            Buffer.BlockCopy(b.Data, 0, _buf, tmp.Length, b.DataSize);
        }

        public void Set(byte[] b)
        {
            _buf = b ?? new byte[0];
            CanUse = true;
            _threwLength = 0;
        }

        public void Set(BufferData b)
        {
            if (b == null || b.Length == 0)
            {
                _buf = new byte[0];
            }
            else
            {
                _buf = new byte[b.DataSize];
                Buffer.BlockCopy(b.Data, 0, _buf, 0, b.DataSize);
            }
            CanUse = true;
            _threwLength = 0;
        }


        public byte[] Get()
        {
            return CanUse ? _buf : null;
        }

        public byte[] SendBuf(int start)
        {
            if (start < 0)
            {
                CanUse = false;
            }

            if (CanUse)
            {
                var len = _buf.Length - start;

                if (len == 0)
                    return null;//これ以上データは無い

                var b = new byte[len];
                Buffer.BlockCopy(_buf, start, b, 0, len);

                if (_buf.Length > _max)
                {
                    CanUse = false;

                    _threwLength += _buf.Length;//サイズ保存
                    _buf = new byte[0];//現在のバッファを捨てる
                }
                return b;
            }
            //start が<0の時、intをオーバーしているので条件判断しない
            if (start != _threwLength && start >= 0)
            {
                return null;
            }
            var buf = new byte[_buf.Length];
            Buffer.BlockCopy(_buf, 0, buf, 0, _buf.Length);

            _threwLength += _buf.Length; //サイズ保存
            _buf = new byte[0]; //現在のバッファを捨てる

            return buf;
        }

        public BufferData GetSendBuffer(int start)
        {
            if (start < 0)
            {
                CanUse = false;
            }

            if (CanUse)
            {
                var len = _buf.Length - start;

                //これ以上データは無い
                if (len == 0) return null;
                
                var b = BufferPool.GetMaximum(len);
                Buffer.BlockCopy(_buf, start, b.Data, 0, len);
                b.DataSize = len;

                if (_buf.Length > _max)
                {
                    CanUse = false;

                    _threwLength += _buf.Length;//サイズ保存
                    _buf = new byte[0];//現在のバッファを捨てる
                }
                return b;
            }
            //start が<0の時、intをオーバーしているので条件判断しない
            if (start != _threwLength && start >= 0)
            {
                return null;
            }
            var buf = BufferPool.GetMaximum(_buf.Length);
            Buffer.BlockCopy(_buf, 0, buf.Data, 0, _buf.Length);
            buf.DataSize = _buf.Length;
            _threwLength += _buf.Length; //サイズ保存
            _buf = new byte[0]; //現在のバッファを捨てる

            return buf;
        }

    }
}
