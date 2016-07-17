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
    public class SockServerTcp : SockObj
    {

        public ProtocolKind ProtocolKind { get; private set; }
        private Socket _socket;
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


        public SockServerTcp(Kernel kernel, ProtocolKind protocolKind, Ssl ssl) : base(kernel)
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer..ctor{protocolKind.ToString()}");
            ProtocolKind = protocolKind;
            _ssl = ssl;
            var pool = SockQueuePool.Instance;
            pool = null;
        }


        public override void Close()
        {
            this.Cancel();
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }
            SetError("close()");
        }


        //TCP用
        public bool Bind(Ip bindIp, int port, int listenMax)
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer.Bind TCP Start {bindIp.ToString()} {port.ToString()} {listenMax.ToString()}");
            _bindIp = bindIp;
            _bindPort = port;
            try
            {
                if (ProtocolKind != ProtocolKind.Tcp)
                    Util.RuntimeException("use udp version bind()");
                try
                {
                    _socket = new Socket(this.Family, SocketType.Stream, ProtocolType.Tcp);
                    _socket.Bind(new IPEndPoint(bindIp.IPAddress, port));
                    _socket.Listen(listenMax);
                }
                catch (Exception e)
                {
                    SetError(Util.SwapStr("\n", "", Util.SwapStr("\r", "", e.Message)));
                    return false;
                }

                Set(SockState.Idle, (IPEndPoint)_socket.LocalEndPoint, null);

                return true;

            }
            finally
            {
                System.Diagnostics.Trace.TraceInformation("SockServer.Bind End");
            }
        }

        public SockTcp Select(ILife iLife)
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer.Select");
            try
            {
                this.SockState = SockState.Bind;
                while (true)
                {
                    if (_socket == null) return null;
                    if (this.IsCancel) return null;
                    if (!iLife.IsLife()) return null;

                    if (_socket.Poll(1000000, SelectMode.SelectRead))
                    {
                        if (_socket == null) return null;
                        if (this.IsCancel) return null;
                        var tTcp = _socket.Accept();
                        return new SockTcp(Kernel, _ssl, tTcp);
                    }

                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Trace.TraceInformation("SockServer.Select OperationCanceledException");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                System.Diagnostics.Trace.TraceError(ex.StackTrace);
            }
            return null;
        }


    }

}
