using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Bjd.Net;
using Bjd.Utils;

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

                Set(SockState.Bind, (IPEndPoint)_socket.LocalEndPoint, null);

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

            SockTcp client = null;
            while (client == null)
            {
                try
                {
                    if (_socket == null) return null;
                    if (this.IsCancel) return null;

                    var tTcp = _socket.AcceptAsync();
                    while (true)
                    {
                        if (tTcp.Wait(2000, this.CancelToken))
                            break;
                        if (tTcp.Status == TaskStatus.Canceled)
                            break;
                        if (!iLife.IsLife())
                            break;
                    }

                    // キャンセル時、終了時はNullを返す
                    if (this.IsCancel || !iLife.IsLife() || tTcp.Result == null)
                    {
                        SetError("isLife()==false");
                        return null;
                    }

                    client = new SockTcp(Kernel, _ssl, tTcp.Result);
                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Trace.TraceInformation("SockServer.Select OperationCanceledException");
                    return null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.Message);
                    System.Diagnostics.Trace.TraceError(ex.StackTrace);
                }
            }
            return client;
        }


        //指定したアドレス・ポートで待ち受けて、接続されたら、そのソケットを返す
        //失敗した時nullが返る
        //Ver5.9.2 Java fix
        //public static SockTcp CreateConnection(Kernel kernel,Ip ip, int port,ILife iLife){
        public static SockTcp CreateConnection(Kernel kernel, Ip ip, int port, Ssl ssl, ILife iLife)
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer.CreateConnection");
            //Ver5.9.2 Java fix
            //var sockServer = new SockServer(kernel,ProtocolKind.Tcp);
            var sockServer = new SockServerTcp(kernel, ProtocolKind.Tcp, ssl);
            if (sockServer.SockState != SockState.Error)
            {
                const int listenMax = 1;
                if (sockServer.Bind(ip, port, listenMax))
                {
                    while (iLife.IsLife())
                    {
                        var child = sockServer.Select(iLife);
                        if (child == null) break;
                        //sockServer.Close(); //これ大丈夫？
                        return child;
                    }
                }
            }
            sockServer.Close();
            return null;
        }

        //bindが可能かどうかの確認
        public static bool IsAvailable(Kernel kernel, Ip ip, int port)
        {
            System.Diagnostics.Trace.TraceInformation($"SockServer.IsAvailable");
            var sockServer = new SockServerTcp(kernel, ProtocolKind.Tcp, null);
            if (sockServer.SockState != SockState.Error)
            {
                const int listenMax = 1;
                if (sockServer.Bind(ip, port, listenMax))
                {
                    sockServer.Close();
                    return true;
                }
            }
            sockServer.Close();
            return false;
        }

    }

}
