using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
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
            _kindBuf = KindBuf.Memory;
            _doc = empty;
        }
        public void Clear()
        {
            _kindBuf = KindBuf.Memory;
            _doc = empty;
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
            _kernel.Logger.TraceInformation($"HttpResponseBody.Set Length={buf.Length}");
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
            _kernel.Logger.TraceInformation($"HttpResponseBody.Send encode={encode}");
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
                using (var fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan))
                {
                    using (var buf = Bjd.Common.Memory.BufferPool.Get(fs.Length))
                    //var bufSize = (fs.Length > bufferSize ? bufferSizeL : bufferSize);
                    //using (var br = new BinaryReader(fs))
                    {
                        var bufSize = buf.Length;
                        //_doc = new byte[bufSize];
                        fs.Seek(_rangeFrom, SeekOrigin.Begin);
                        var start = _rangeFrom;
                        while (iLife.IsLife())
                        {
                            long size = _rangeTo - start + 1;
                            if (size > bufSize)
                                size = bufSize;
                            if (size <= 0)
                                break;

                            //int len = br.Read(_doc, 0, (int)size);
                            //int len = br.Read(buf.Data, 0, (int)size);
                            int len = fs.Read(buf.Data, 0, (int)size);
                            if (len <= 0)
                                break;

                            //var segment = new ArraySegment<byte>(_doc, 0, len);
                            var segment = new ArraySegment<byte>(buf.Data, 0, len);

                            if (encode)
                            {
                                if (-1 == tcpObj.SendUseEncode(segment))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                if (-1 == tcpObj.SendNoEncode(segment))
                                {
                                    return false;
                                }
                            }
                            //start += _doc.Length;
                            start += bufSize;
                            if (_rangeTo - start <= 0)
                                break;
                            //Thread.Sleep(1);
                        }
                        //br.Close();
                    }
                    //fs.Close();
                }
            }
            return true;
        }
    }

}
