﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bjd;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Configurations;
using Bjd.Net.Sockets;
using Bjd.Utils;
using Bjd.Threading;
using Bjd.Memory;

namespace Bjd.WebServer
{
    class HttpResponseBody
    {

        enum KindBuf
        {
            Memory = 0,
            Disk = 1,
        }

        static BufferData empty = BufferData.Empty;

        Kernel _kernel;
        KindBuf _kindBuf;
        BufferData _doc;
        long docLength;

        string _fileName;
        long _rangeFrom;
        long _rangeTo;

        public HttpResponseBody(Kernel kernel)
        {
            _kernel = kernel;
            Clear();
        }
        public void Clear()
        {
            _kindBuf = KindBuf.Memory;
            if (_doc != null && _doc != empty) _doc.Dispose();
            _doc = empty;
            _fileName = string.Empty;
            _rangeFrom = 0;
            _rangeTo = 0;
            docLength = 0;
        }

        public void Set(string fileName, long rangeFrom, long rangeTo)
        {
            _kindBuf = KindBuf.Disk;
            _fileName = fileName;
            _rangeFrom = rangeFrom;
            _rangeTo = rangeTo;
        }
        public void Set(byte[] buf)
        {
            _kindBuf = KindBuf.Memory;
            _doc = buf.ToBufferData();
            _doc.DataSize = buf.Length;
            docLength = buf.Length;
            _kernel.Logger.DebugInformation($"HttpResponseBody.Set Length={buf.Length}");
        }
        public void Set(BufferData buf)
        {
            _kindBuf = KindBuf.Memory;
            _doc = buf;
            docLength = buf.DataSize;
            _kernel.Logger.DebugInformation($"HttpResponseBody.Set Length={buf.Length}");
        }

        public long Length
        {
            get
            {
                if (_kindBuf == KindBuf.Memory)
                {
                    return docLength;
                }
                return _rangeTo - _rangeFrom + ((_rangeFrom == 0) ? 0 : 1);
            }
        }

        public async Task<bool> SendAsync(ISocket tcpObj, bool encode, ILife iLife)
        {
            _kernel.Logger.DebugInformation($"HttpResponseBody.Send encode={encode}");
            if (_kindBuf == KindBuf.Memory)
            {
                //tcpObj.SendAsync(_doc);
                try
                {
                    await tcpObj.SendAsync(_doc);
                }
                finally
                {
                    _doc.Dispose();
                    _doc = empty;
                }
            }
            else
            {
                var bufSize = 65536;
                using (var fs = Common.IO.CachedFileStream.GetFileStream(_fileName))
                using (var b = Bjd.Memory.BufferPool.Get(bufSize))
                {
                    fs.Seek(_rangeFrom, SeekOrigin.Begin);
                    var start = _rangeFrom;
                    while (iLife.IsLife())
                    {
                        long size = _rangeTo - start + 1;
                        if (size > bufSize) size = bufSize;
                        if (size <= 0) break;

                        int len = await fs.ReadAsync(b.Data, 0, (int)size);
                        if (len <= 0) break;

                        b.DataSize = len;
                        //tcpObj.SendAsync(b);
                        await tcpObj.SendAsync(b);

                        start += len;
                        if (_rangeTo - start <= 0) break;
                    }

                }
            }
            return true;
        }

    }

}
