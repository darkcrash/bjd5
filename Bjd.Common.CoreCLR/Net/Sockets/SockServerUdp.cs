using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Bjd.Net;
using Bjd.Utils;
using Bjd.Threading;

namespace Bjd.Net.Sockets
{
    public class SockServerUdp : SockObj
    {

        public ProtocolKind ProtocolKind { get; private set; }
        private Socket _socket;
        byte[] _udpBuf;
        ArraySegment<byte> _udpBufSegment;
        private Ip _bindIp;
        private int _bindPort;
        private object Lock = new object();

        private readonly Ssl _ssl;

        private AddressFamily Family
        {
            get
            {
                return (_bindIp.InetKind == InetKind.V4) ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;
            }
        }


        public SockServerUdp(Kernel kernel, ProtocolKind protocolKind, Ssl ssl) : base(kernel)
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer..ctor{protocolKind.ToString()}");
            ProtocolKind = protocolKind;
            _ssl = ssl;
        }

        public override void Close()
        {
            this.Cancel();
            if (_socket != null)
            {
                _socket.Dispose();
            }
            SetError("close()");
        }


        //UDP用
        public bool Bind(Ip bindIp, int port)
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer.Bind UDP Start {bindIp.ToString()} {port.ToString()} ");
            _bindIp = bindIp;
            _bindPort = port;
            if (ProtocolKind != ProtocolKind.Udp)
                Util.RuntimeException("use tcp version bind()");

            try
            {
                _socket = new Socket(this.Family, SocketType.Dgram, ProtocolType.Udp);
                _socket.Bind(new IPEndPoint(bindIp.IPAddress, port));
            }
            catch (Exception e)
            {
                SetError(Util.SwapStr("\n", "", Util.SwapStr("\r", "", e.Message)));
                return false;
            }

            Set(SockState.Idle, (IPEndPoint)_socket.LocalEndPoint, null);

            _udpBuf = new byte[1600]; //１パケットの最大サイズで受信待ちにする
            _udpBufSegment = new ArraySegment<byte>(_udpBuf);

            //受信開始
            BeginReceiveUdp();

            return true;
        }

        //受信開始
        void BeginReceiveUdp()
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer.BeginReceiveUdp");
            // UDP
            var ep = (EndPoint)new IPEndPoint((_bindIp.InetKind == InetKind.V4) ? IPAddress.Any : IPAddress.IPv6Any, _bindPort);
            var tUdp = _socket.ReceiveFromAsync(_udpBufSegment, SocketFlags.None, ep);
            var ts = TaskScheduler.Current;
            tUdp.ContinueWith(_ => this.Receive(_), this.CancelToken, TaskContinuationOptions.LongRunning, ts);
            this.SockState = SockState.Bind;
        }


        void Receive(Task<SocketReceiveFromResult> taskResult)
        {
            if (taskResult.IsCanceled)
                return;
            if (taskResult.IsCompleted)
            {
                try
                {
                    SocketReceiveFromResult srfr = taskResult.Result;
                    //int len = _socket.EndReceiveFrom(ar, ref ep);
                    SockUdp sockUdp = new SockUdp(Kernel, _socket, _udpBuf, srfr.ReceivedBytes, (IPEndPoint)srfr.RemoteEndPoint); //ACCEPT
                    lock (Lock)
                    {
                        sockQueue.Enqueue(sockUdp);
                    }
                }
                catch (Exception) { }
            }
            //受信開始
            BeginReceiveUdp();
        }

        protected internal override void Cancel()
        {
            base.Cancel();
            WaitSelect.Set();
        }

        Queue<SockUdp> sockQueue = new Queue<SockUdp>();
        ManualResetEventSlim WaitSelect = new ManualResetEventSlim(false);

        public SockUdp Select(ILife iLife)
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer.Select");

            while (iLife.IsLife())
            {
                lock (Lock)
                {
                    if (sockQueue.Count > 0)
                    {
                        return sockQueue.Dequeue();
                    }
                    else if (!IsCancel)
                    {
                        WaitSelect.Reset();
                    }

                }                //Ver5.8.1
                //Thread.Sleep(0);
                //Thread.Sleep(1);
                WaitSelect.Wait(2000, this.CancelToken);
            }
            SetError("isLife()==false");
            return null;
        }

    }

}
