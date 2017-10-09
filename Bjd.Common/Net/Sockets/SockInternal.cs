using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Bjd.Logs;
using Bjd.Memory;
using Bjd.Threading;

namespace Bjd.Net.Sockets
{
    public class SockInternal : SockObj, ISocket
    {
        private static Ip _LocalIp = new Ip(IpKind.V4Localhost);
        private static Ip _RemoteIp = new Ip(IpKind.V4Localhost);

        private SockInternal _pair;
        internal SockQueue _sockQueueRecv;


        public byte[] RecvBuf => throw new NotImplementedException();

        public string LastLineSend => _lastLineSend;
        private string _lastLineSend = "";


        private SockInternal(Kernel kernel) : base(kernel)
        {
            LocalAddress = new IPEndPoint(IPAddress.None, 0);
            RemoteAddress = new IPEndPoint(IPAddress.None, 0);
            _sockQueueRecv = SockQueuePool.Instance.Get();
            _sockQueueRecv.UseLf();
        }

        internal static SockInternalPair Create(Kernel kernel)
        {
            var pair = new SockInternalPair()
            {
                Client = new SockInternal(kernel),
                Server = new SockInternal(kernel)
            };
            pair.Server._pair = pair.Client;
            pair.Client._pair = pair.Server;
            pair.Server.SockState = SockState.Connect;
            pair.Client.SockState = SockState.Connect;
            return pair;
        }


        public override void Close()
        {
            if (this.disposedValue) return;
            this.Dispose();
        }


        public int Length()
        {
            return _sockQueueRecv.Length;
        }

        public async ValueTask<BufferData> BufferRecvAsync(int len, int sec)
        {
            var toutms = sec * 1000;
            var result = await _sockQueueRecv.DequeueBufferAsync(len, toutms, this.CancelToken);
            if (result.DataSize == 0 && SockState != SockState.Connect) return null;
            var length = (result != null ? result.DataSize.ToString() : "null");
            Kernel.Logger.DebugInformation(hashText, " SockInternal.BufferRecvAsync ", length);
            return result;
        }


        public BufferData LineBufferRecv(int sec, ILife iLife)
        {
            var toutms = sec * 1000;
            var resultTask = _sockQueueRecv.DequeueLineBufferAsync(toutms, this.CancelToken).AsTask();
            resultTask.Wait();
            var result = resultTask.Result;
            if (result.DataSize == 0) return null;
            var length = (result != null ? result.DataSize.ToString() : "null");
            Kernel.Logger.DebugInformation(hashText, " SockTcp.LineBufferRecv ", length);
            return result;
        }

        public async ValueTask<BufferData> LineBufferRecvAsync(int timeoutSec)
        {
            var toutms = timeoutSec * 1000;
            Kernel.Logger.DebugInformation(hashText, " SockTcp.LineBufferRecvAsync ");
            return await _sockQueueRecv.DequeueLineBufferAsync(toutms);
        }

        public byte[] LineRecv(int sec, ILife iLife)
        {
            var toutms = sec * 1000;
            var t = _sockQueueRecv.DequeueLineAsync(toutms, this.CancelToken).AsTask();
            t.Wait();
            var result = t.Result;
            if (result.Length == 0) return null;
            var length = (result != null ? result.Length.ToString() : "null");
            Kernel.Logger.DebugInformation(hashText, " SockTcp.LineRecv ", length);
            return result;
        }

        public byte[] Recv(int len, int sec, ILife iLife)
        {
            var toutms = sec * 1000;
            var t = _sockQueueRecv.DequeueAsync(len, toutms, this.CancelToken).AsTask();
            t.Wait();
            var result = t.Result;
            if (result.Length == 0 && SockState != SockState.Connect) return null;
            var length = (result != null ? result.Length.ToString() : "null");
            Kernel.Logger.DebugInformation(hashText, " SockTcp.Recv ", length);
            return result;
        }




        public int AsciiSend(string str)
        {
            _lastLineSend = str;
            var buf = Encoding.ASCII.GetBytes(str);
            //return LineSend(buf, operateCrlf);
            //とりあえずCrLfの設定を無視している
            var d = new[] { new ArraySegment<byte>(buf), new ArraySegment<byte>(CrLf) };
            return Send(d);
        }


        public int Send(byte[] buf, int offset, int length)
        {
            return _pair._sockQueueRecv.Enqueue(buf.ToBufferData(offset, length));
        }

        public int Send(IList<ArraySegment<byte>> buffers)
        {
            var cnt = 0;
            foreach (var buf in buffers)
            {
                cnt  += _pair._sockQueueRecv.Enqueue(buf.ToBufferData());
            }
            return cnt;
        }

        public int Send(BufferData buf)
        {
            _pair._sockQueueRecv.Enqueue(buf.Copy());
            return buf.DataSize;
        }

        public ValueTask<bool> SendAsync(BufferData buf)
        {
            _pair._sockQueueRecv.Enqueue(buf.Copy());
            return new ValueTask<bool>(true);
        }

        public int SendNoTrace(byte[] buffer)
        {
            return _pair._sockQueueRecv.Enqueue(buffer, buffer.Length);
        }

        public int SendNoTrace(ArraySegment<byte> buffer)
        {
            return _pair._sockQueueRecv.Enqueue(buffer.ToBufferData());
        }


        private bool disposedValue = false; // 重複する呼び出しを検出するには


        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
                if (disposing)
                {
                }

                try { this.Cancel(); }
                catch (Exception ex)
                { Kernel?.Logger.TraceError($"{hashText} Dispose Error Cancel {ex.Message} {ex.StackTrace} "); }

                if (_sockQueueRecv != null)
                {
                    _sockQueueRecv.Dispose();
                    _sockQueueRecv = null;
                }
            }
        }


    }
}
