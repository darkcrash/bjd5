using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using Bjd;
using Bjd.Logs;
using Bjd.Net;
using Bjd.Options;
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
            _kernel.Trace.TraceInformation($"HttpResponseBody.Set Length={buf.Length}");
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
            _kernel.Trace.TraceInformation($"HttpResponseBody.Send encode={encode}");
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
                using (var fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        _doc = new byte[1048560];
                        fs.Seek(_rangeFrom, SeekOrigin.Begin);
                        var start = _rangeFrom;
                        while (iLife.IsLife())
                        {
                            long size = _rangeTo - start + 1;
                            if (size > 1048560)
                                size = 1048560;
                            if (size <= 0)
                                break;

                            int len = br.Read(_doc, 0, (int)size);
                            if (len <= 0)
                                break;

                            //if (len != size)
                            //{
                            //    var tmp = new byte[len];
                            //    Buffer.BlockCopy(_doc, 0, tmp, 0, len);
                            //    _doc = tmp;
                            //}
                            var segment = new ArraySegment<byte>(_doc, 0, len);

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
                            start += _doc.Length;
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
