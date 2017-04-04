using System;
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

namespace Bjd.WebServer
{
    class HttpResponseBody
    {

        enum KindBuf
        {
            Memory = 0,
            Disk = 1,
        }

        //const int bufferSize = 1048560;
        const int bufferSizeL = 1048560;
        const int bufferSize = 16384;

        static byte[] empty = new byte[0];

        Kernel _kernel;
        KindBuf _kindBuf;
        byte[] _doc;

        string _fileName;
        long _rangeFrom;
        long _rangeTo;

        public HttpResponseBody(Kernel kernel)
        {
            _kernel = kernel;
            //_kindBuf = KindBuf.Memory;
            //_doc = empty;
            Clear();
        }
        public void Clear()
        {
            _kindBuf = KindBuf.Memory;
            _doc = empty;
            _fileName = string.Empty;
            _rangeFrom = 0;
            _rangeTo = 0;
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
            _doc = new byte[buf.Length];
            Buffer.BlockCopy(buf, 0, _doc, 0, buf.Length);
            _kernel.Logger.DebugInformation($"HttpResponseBody.Set Length={buf.Length}");
        }

        public long Length
        {
            get
            {
                if (_kindBuf == KindBuf.Memory)
                {
                    return _doc.Length;
                }
                return _rangeTo - _rangeFrom + ((_rangeFrom == 0) ? 0 : 1);
            }
        }
        //public bool Send(SockTcp tcpObj,bool encode,ref bool life){
        public bool Send(SockTcp tcpObj, bool encode, ILife iLife)
        {
            _kernel.Logger.DebugInformation($"HttpResponseBody.Send encode={encode}");
            if (_kindBuf == KindBuf.Memory)
            {
                if (encode)
                {
                    if (-1 == tcpObj.SendUseEncode(_doc))
                        return false;
                }
                else
                {
                    if (-1 == tcpObj.SendNoEncode(_doc))
                        return false;
                }
            }
            else
            {
                using (var fs = Common.IO.CachedFileStream.GetFileStream(_fileName))
                {
                    using (var buf = Bjd.Memory.BufferPool.Get(fs.Length))
                    {
                        var bufSize = buf.Length;
                        fs.Seek(_rangeFrom, SeekOrigin.Begin);
                        var start = _rangeFrom;
                        while (iLife.IsLife())
                        {
                            long size = _rangeTo - start + 1;
                            if (size > bufSize)
                                size = bufSize;
                            if (size <= 0)
                                break;

                            int len = fs.Read(buf.Data, 0, (int)size);
                            if (len <= 0)
                                break;

                            if (-1 == tcpObj.Send(buf.Data, len))
                            {
                                return false;
                            }

                            //var segment = new ArraySegment<byte>(buf.Data, 0, len);

                            //if (encode)
                            //{
                            //    if (-1 == tcpObj.SendUseEncode(segment))
                            //    {
                            //        return false;
                            //    }
                            //}
                            //else
                            //{
                            //    if (-1 == tcpObj.SendNoEncode(segment))
                            //    {
                            //        return false;
                            //    }
                            //}
                            start += len;
                            if (_rangeTo - start <= 0)
                                break;

                        }
                    }
                }
            }
            return true;
        }
    }

}
